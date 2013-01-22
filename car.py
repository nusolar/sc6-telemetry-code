#!/usr/bin/env python
import serial, io, time, pika, os, sys
# flow: loop( not_serial.serialutil.SerialException(loop( not_pika.exceptions.AMQPConnectionError(send) wait, reconnect)) wait, recheck)
def listen():
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	
	ser = serial.Serial(device())
	if not ser.isOpen():
		print("Error: couldn't open serial connection!")
	ser.write('S8\rO\r')
	
	sio = io.TextIOWrapper(io.BufferedRWPair(ser, ser))
	while sio.readable():
		line = sio.readline()
		if line[0] != 't':
			continue
		line = str(time.time()) + line
		channel.basic_publish(exchange='',routing_key='telemetry',body=packet)
	
	sio.close()
	ser.close()
	connection.close()

def device():
	files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB1'}
	if not os.path.exists(files[sys.platform]):
		print("Error: device file doesn't exist!")
	return files[sys.platform]