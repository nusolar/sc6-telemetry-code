#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, serial, io, time, sys, multiprocessing as mp, threading as th

def loop(fun, xcpn = BaseException):
	while True:
		try: sys.tracebacklimit = 1; fun()
		except (xcpn) as e:	print type(e).__name__+" on "+fun.func_name+": "+str(e)
		time.sleep(4)

def process(func):
	print "\n\nTrying "+func.func_name
	p = mp.Process(target=func)
	p.start()
	p.join()

def hammer():
	files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB0'}
	ser = serial.Serial(files[sys.platform], baudrate=9600)
	if not ser.isOpen(): print("WTF: %s isn't open" % ser.port) #DEBUG
	ser.write("S8\rO\r")
	sio = io.TextIOWrapper(io.BufferedRWPair(ser, ser))
	while sio.readable():
		line = sio.readline()
		if line[0] != 't': continue
		line = "{0:.6f}".format(time.time()) + line
		print line #DEBUG
		channel.basic_publish(exchange='',routing_key='telemetry',body=line)
def hephaestus():
	con = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = con.channel()
	channel.queue_declare(queue='telemetry')
	loop(hammer, serial.SerialException)

def hermes():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue='telemetry')
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	chan2 = con2.channel()
	chan2.queue_declare(queue='telemetry')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key='telemetry',body=pkt)
	chan1.basic_consume(callback,queue='telemetry',no_ack=True)
	try: chan1.start_consuming()
	except: con1.close(); con2.close()

if __name__ == '__main__':
	for job in (hephaestus, hermes):
		th.Thread(target = lambda:loop( lambda:process(job) ) ).start()
