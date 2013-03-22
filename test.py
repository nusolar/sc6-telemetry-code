# Copyright Alex Chandel, 2013. All rights reserved.
import unittest, datetime, time, sys, os, pty, threading as th
import car, laptop, analysis

class TestTelemetry(unittest.TestCase):
	def test_car_reading(self):
		master, slave = pty.openpty()
		s_name = os.ttyname(slave)
		car.files[sys.platform] = s_name
		pkt = []
		def ham_f():
			try: car.hammer(pkt.append)
			except (IOError): pass
		ham_t = th.Thread(target = ham_f)
		ham_t.start()
		x = os.read(master,1000)
		print('car wrote to CAN: '+x.replace('\r','\\r'))
		self.assertEqual(x,"S8\rO\r")
		os.write(master, 'tTESTPACKET\r')
		time.sleep(1)
		print('car read from CAN: '+pkt[0])
		t = pkt[0].find('t')
		self.assertGreater(60, abs(time.time()-float(pkt[0][:t])))
		self.assertEqual(pkt[0][t:], 'tTESTPACKET')
		os.close(master)
		
	def test_angles(self):
		self.assertAlmostEqual(analysis.cosTheta(0,analysis.pi/2,datetime.datetime(2013,3,20,12,0,0)), 1)
		self.assertAlmostEqual(analysis.cosTheta(-analysis.pi,analysis.pi/2,datetime.datetime(2013,3,20,12,0,0)), -1)
	def test_drag(self):
		self.assertGreater(analysis.powerOut(10),analysis.powerOut(5))
	
	def test_laptop_halting(self):
		laptop.begin()
		time.sleep(5)
		laptop.quit()
		time.sleep(5)
		for key,worker in laptop.roll.iteritems():
			print(key + " is " + ("on" if worker.on() else "off"))
			self.assertFalse(worker.on())

if __name__ == '__main__':
	unittest.main()
