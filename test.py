#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import unittest, datetime, time, sys, os, pty, threading as th, pika, config
# Set up testing variables before importing Telemetry
config.server_name = 'localhost'
config.client_name = config.server_name
config.loop_delay = 0.1
import car, laptop, consumer, analysis, db

class TestCanUsb(unittest.TestCase):
	def setUp(self):
		car.halt = False

	def test_car_reading(self):
		print("TEST: Car activates CANUSB, reads & processes sample garbage packet.")
		master, slave = pty.openpty()
		name = os.ttyname(slave)
		config.files[sys.platform] = name
		pkt = None

		def hammer_callback(line):
			nonlocal pkt
			pkt = line
		hammer_th = th.Thread(target = lambda: car.hammer(hammer_callback))
		hammer_th.start()

		x = os.read(master, 1000)
		print('car wrote to CAN: ' + x.replace(b'\r', b'\\r').decode('utf8'))
		self.assertEqual(x, b'S8\rO\r')
		car.halt = True

		os.write(master, b't7FF8AlexTest\r')
		time.sleep(0.1)
		print('car read from CAN: ' + pkt.decode('utf8'))
		t = pkt.find(b't')
		self.assertGreater(60, abs(time.time() - float(pkt[:t])))
		self.assertEqual(pkt[t:], b't7FF8AlexTest')
		os.close(master)

class TestAnalysis(unittest.TestCase):
	def test_angles(self):
		print("TEST: Heliophysics calculations")
		self.assertAlmostEqual(analysis.cosTheta(0, analysis.pi / 2, datetime.datetime(2013, 3, 20, 12, 0, 0)), 1)
		self.assertAlmostEqual(analysis.cosTheta(-analysis.pi, analysis.pi / 2, datetime.datetime(2013, 3, 20, 12, 0, 0)), -1)

	def test_array_power(self):
		print("TEST: Calculate array power from luminosity & PV cell temperature")
		self.assertAlmostEqual(analysis.arrayPower(950, 59), 995.41, places=2)

	def test_drag(self):
		print("TEST: Drag/rolling resistance/lift force calcuations")
		self.assertGreater(analysis.powerOut(10), analysis.powerOut(5))

class TestMessageQueues(unittest.TestCase):
	@classmethod
	def setUpClass(cls):
		print('RMQ going up')
		cls.worker = next(filter(lambda x: x.name=='rmq', laptop.roll))
		cls.worker.start()
		time.sleep(6)

	@classmethod
	def tearDownClass(cls):
		print('RMQ going down')
		cls.worker.stop()
		time.sleep(3)

	def setUp(self):
		self.cxn = pika.BlockingConnection(pika.ConnectionParameters(host = 'localhost'))
		self.channel = self.cxn.channel()
		# self.channel.queue_delete(queue = config.afferent_client_outbox)
		# self.channel.queue_delete(queue = config.afferent_server_inbox)
		car.halt = False
		consumer.halt = False

	def tearDown(self):
		#self.channel.close()
		self.cxn.close()

	def test_afferent_car1(self):
		config.loop_delay = 0
		print("TEST: Car caches CAN packet on local queue. (\"hephaestus\")")

		def heph_callbacker(callback):
			car.halt = True
			callback(b'succeeded!')
		hephaestus_th = th.Thread(target = lambda: car.hephaestus(heph_callbacker))
		hephaestus_th.start()
		time.sleep(0.1)

		method, header, body = self.channel.basic_get(queue = config.afferent_client_outbox)
		self.assertNotEqual(method.NAME, 'Basic.GetEmpty')
		print("hephaestus... " + str(body))

	def test_afferent_car2(self):
		car.halt = True
		print("TEST: Car sending packet to Laptop queue. (\"hermes\")")
		hermes_th = th.Thread(target = car.hermes)
		hermes_th.start()
		time.sleep(0.1)

		self.channel.basic_publish(exchange='', routing_key = config.afferent_client_outbox, body = b'succeeded!')
		time.sleep(0.1)

		method, header, body = self.channel.basic_get(queue = config.afferent_server_inbox)
		self.assertNotEqual(method.NAME, 'Basic.GetEmpty')
		print("hermes... " + str(body))

	def test_afferent_server(self):
		print("TEST: Server receives & consumes packets.")
		pkt = None

		def receive_callback(ch, method, header, body):
			nonlocal pkt
			pkt = body
			ch.basic_ack(method.delivery_tag)
			ch.stop_consuming()
		receive_th = th.Thread(target = lambda: consumer.receive(receive_callback))
		receive_th.start()
		time.sleep(0.1)

		self.channel.basic_publish(exchange='', routing_key = config.afferent_server_inbox, body = b'succeeded!')
		time.sleep(0.1)

		self.assertNotEqual(pkt, None)
		print("consumer... " + str(pkt))

class TestPacketHandling(unittest.TestCase):
	"""Tests parsing packets/entering into dictionary"""
	def test_handling(self):
		data = db.tables[0]
		data.clear_row()
		self.assertIsNone(data.row[0])
		consumer.handle(None, None, None, b'Garbage Packet')
		pkt = str(time.time()).encode() + b't0008AlexTest'
		consumer.handle(None, None, None, pkt)
		now = time.time()
		pkt = str(now).encode() + b't2109HeartTest' # 0x210 heartbeat packet
		consumer.handle(None, None, None, pkt)
		self.assertIs(type(data.row[0]), int)
		self.assertEqual(data.row[0], int(now))

class TestProcessIntegration(unittest.TestCase):
	"""Make sure the Task Manager, Consumer, and DB all play nicely"""
	def setUp(self):
		laptop.begin()
		time.sleep(6)
		self.cxn = pika.BlockingConnection(pika.ConnectionParameters(host = 'localhost'))
		self.channel = self.cxn.channel()
		car.halt = False
		consumer.halt = False

	def tearDown(self):
		if self.cxn.is_open: self.cxn.close()
		laptop.quit()
		time.sleep(3)

	def test_process_halting(self):
		"""Tests the raising and killing of worker processes"""
		print("BIGTEST: Gracefully spawn/terminate all processes.")
		# [self.assertIs(worker.on(), True) for worker in laptop.roll]
		self.cxn.close()
		laptop.quit()
		time.sleep(3.1)
		for worker in reversed(laptop.roll):
			self.assertFalse(worker.on())

	def test_server_integration(self):
		print("BIGTEST: Packet travels to Queue, to Consumer, to Packet Handler, to SQLite DB")
		now = time.time()
		self.channel.basic_publish(exchange='', routing_key = config.afferent_server_inbox, body = str(now).encode()+b't2109SysIntTst')
		time.sleep(1.5)
		self.channel.basic_publish(exchange='', routing_key = config.afferent_server_inbox, body = str(time.time()).encode()+b't2109SysIntTs2')
		time.sleep(0.1)
		self.assertEqual(db.tables[0][-1, 0][0][0], int(now))

if __name__ == '__main__':
	unittest.main()
