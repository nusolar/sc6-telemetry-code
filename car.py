#!/usr/bin/env python
import serial, io, time, pika, os, sys

def loop(fun):
	while True:
		try: fun()
		except: pass
		time.sleep(5)

def listen(ser):
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	
	if not ser.isOpen(): print("WTF: %s isn't open" % ser.port)
	ser.write('S8\rO\r')
	sio = io.TextIOWrapper(io.BufferedRWPair(ser, ser))
	while sio.readable():
		line = sio.readline()
		if line[0] != 't': continue
		line = str(time.time()) + line
		channel.basic_publish(exchange='',routing_key='telemetry',body=line)
	
	sio.close()
	ser.close()
	connection.close()

def device():
	files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB1'}
	ser = serial.Serial(files[sys.platform])
	loop(lambda: listen(ser))

if __name__ == '__main__':
	loop(device)