#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, serial, io, time, sys, multiprocessing as mp, threading as th, config
files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB0'}

def loop(fun, xcpn = Exception):
	while True:
		try: sys.tracebacklimit = 1; fun()
		except (xcpn) as e:	print(type(e).__name__+" on "+fun.__name__+": "+str(e))
		finally: time.sleep(4)

def process(func):
	print("\n\nTrying "+func.__name__)
	p = mp.Process(target=func)
	p.start()
	p.join()

def hammer(callback):
	ser = serial.Serial(files[sys.platform])
	if not ser.isOpen(): print("WTF: %s isn't open" % ser.port) #DEBUG
	ser.write(b'S8\rO\r')
	def newlines(buffer = b''):
		while ser.isOpen():
			buffer += ser.read(1)
			if buffer[-1] is 13: yield buffer; buffer = b'' #b'\r'[0]
	for line in newlines():
		if line[0] != 116: continue #b't'[0]
		line = "{0:.6f}".format(time.time()).encode('utf8') + line[:-1]
		callback(line)
def hephaestus():
	con = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan = con.channel()
	chan.queue_declare(queue = config.afferent_client_outbox)
	loop(lambda:hammer(lambda x:chan.basic_publish(exchange='', routing_key = config.afferent_client_outbox, body=x)), 
		serial.SerialException)

def hermes():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue = config.afferent_client_outbox)
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host = config.server_name))
	chan2 = con2.channel()
	chan2.queue_declare(queue = config.afferent_server_inbox)
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key = config.afferent_server_inbox, body=pkt)
	chan1.basic_consume(callback,queue = config.afferent_client_outbox,no_ack=True)
	chan1.start_consuming() #WARNING may drop 1 packet upon crash!

if __name__ == '__main__':
	for job in (hephaestus, hermes):
		th.Thread(target = lambda:loop( lambda:process(job) ) ).start()
