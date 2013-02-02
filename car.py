#!/usr/bin/env python
import pika, serial, io, time, sys, multiprocessing as mp, threading at th

def loop(fun, xcpn = BaseException):
	while True:
		try: fun()
		except (xcpn) as e: print e
		time.sleep(4)

def hammer():
	files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB0'}
	ser = serial.Serial(files[sys.platform], baudrate=9600)
	if not ser.isOpen(): print("WTF: %s isn't open" % ser.port) #DEBUG
	ser.write('S8\rO\r')
	sio = io.TextIOWrapper(io.BufferedRWPair(ser, ser))
	while sio.readable():
		line = sio.readline()
		if line[0] != 't': continue
		line = str(time.time()) + line
		print line #DEBUG
		channel.basic_publish(exchange='',routing_key='telemetry',body=line)
def hephaestus():
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	loop(hammer, serial.SerialException)

def hermes():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = connection.channel()
	chan1.queue_declare(queue='telemetry')
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	chan2 = connection.channel()
	chan2.queue_declare(queue='telemetry')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key='telemetry',body=pkt)
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: chan1.start_consuming()
	except: chan2.close()
	
if __name__ == '__main__':
	th.Thread(target = lambda:mp.Process(target = hephaestus).start()).start()
	th.Thread(target = lambda:mp.Process(target = hermes).start()).start()