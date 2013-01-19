#!/usr/bin/env python

import pika
def sendPika(packet):
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = connection.channel()
	channel.queue_declare(queue='hello')
	if True:
		channel.basic_publish(exchange='',routing_key='hello',body=packet)
	print " [x] Sent '%'" % packet
	connection.close()

import serial, time
def listen:
	ser = serial.Serial(device())
	ser.baudrate = 9600
	ser.write('S8\rO\r')
	if !ser.isOpen():
		print("Error: couldn't open serial cxn!")
	sio = io.TextIOWrapper(io.BufferedRWPair(ser, ser))
	while ser.readable():
		line = readline()
		if line[0]!='t':
			print("warning: received non-T packet, discarding")
			continue
		line = str(time.time()) + line
	sio.close()
	ser.close()

import os, sys
def device:
	files = {'darwin':'/dev/tty.usbserial-LWR8N2L2', 'linux2':'/dev/ttyUSB0'}
	if !os.path.exists(file[sys.platform]):
		print("Error: /dev file doesn't exist!")
	return files[sys.platform]