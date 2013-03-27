#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, serial, time, sys, multiprocessing as mp, threading as th, config
halt = False

def loop(fun, xcpn = Exception):
	while not halt:
		try: sys.tracebacklimit = 3; fun()
		except (xcpn) as e: print(type(e).__name__ + " on " + fun.__name__ + ": " + str(e))
		finally: time.sleep(config.loop_delay)

def process(func):
	print("\n\nTrying " + func.__name__)
	p = mp.Process(target=func)
	p.start()
	p.join()

def hammer(callback):
	with serial.Serial(config.files[sys.platform]) as ser:
		ser.write(b'S8\rO\r')

		def newlines(buffer = b''):
			while not halt:
				while ser.isOpen() and halt is not None:
					buffer += ser.read(1)
					if buffer[-1] is 13: yield buffer; buffer = b''; break # b'\r'[0]
		for line in newlines():
			if line[0] != 116: continue # b't'[0]
			line = str(time.time()).encode() + line[:-1]
			callback(line)
def hephaestus(callbacker = hammer):
	con = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan = con.channel()
	chan.queue_declare(queue = config.afferent_client_outbox)
	loop(lambda: callbacker(lambda x: chan.basic_publish(exchange='',
		routing_key = config.afferent_client_outbox, body=x)), serial.SerialException)
	con.close()

def hermes():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue = config.afferent_client_outbox)
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host = config.server_name))
	chan2 = con2.channel()
	chan2.queue_declare(queue = config.afferent_server_inbox)
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='', routing_key = config.afferent_server_inbox, body=pkt)
		ch.basic_ack(method.delivery_tag)
		if halt: ch.stop_consuming()
	chan1.basic_consume(callback, queue = config.afferent_client_outbox)
	chan1.start_consuming() # WARNING drops 1 packet upon crash!

if __name__ == '__main__':
	for job in (hephaestus, hermes):
		th.Thread(target = lambda: loop(lambda: process(job))).start()
