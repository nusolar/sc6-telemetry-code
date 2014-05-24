#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, time, config, signal, sys, binascii, logging

#PACKET DATA TYPES
def modules(table, match, data): # [uint32, float]
	column = match[2] + str(int(data[0:8], 16))
	table.row[table.cols[column]] = float.fromhex(data[8:])
def double(table, match, data): # [double count]
	table.row[table.cols[match[2]]] = float.fromhex(data)
def float2(table, match, data): # [float array, float batt]
	if match[2]: table.row[table.cols[match[2]]] = float.fromhex(data[:8])
	if match[3]: table.row[table.cols[match[3]]] = float.fromhex(data[8:])
def int64(table, match, data): # [11 bits, 21 unused] or [12 bits, 20 unused]
	table.row[table.cols[match[2]]] = int(data, 16)
	#bits = bin(int(data,16))[2:] #bin -> "0b0123456789AB..."
	#opts = ("Left Right Yes No Maybe Haz Horn CEn CMode CUp CDown", #11 btns
	#	"Left Right Marconi Yes Haz CEn CUp Maybe No Horn CMode CDown",
	#	)['lights' in db.name[addr]].split(' ') #12 lights
	#flags= ' '.join(opt for (bit,opt) in zip(bits[:len(opts)],opts) if bit=='1')
def int2(table, match, data):
	table.row[table.cols[match[2]]] = int(data[:8], 16)
	if match[3]: table.row[table.cols[match[3]]] = int(data[8:], 16)
def bit2(table, match, data):
	table.row[table.cols[match[2]]] = 0 if int(data[:8], 16)==0 else 1
	table.row[table.cols[match[3]]] = 0 if int(data[8:], 16)==0 else 1
def bit64(table, match, data):
	table.row[table.cols[match[2]]] = 0 if int(data, 16)==0 else 1
def mppt(table, match, data):
	pass
def trip(table, match, data): # [int32, uint32]
	s = int(data[:8], 16)
	table.row[table.cols[match[2]]] = s if s < 2**32/2 else s-2**32
	table.row[table.cols[match[3]]] = int(data[8:], 16)
def error(table, match, data): # [char*8]
	table.row[table.cols[match[2]]] += binascii.unhexlify(data)
def trash(table, match, data): # [char*4, uint32]
	logging.debug("Trashing: "+str(data))

import db
halt = False

def handle(ch, method, hProperties, pkt):
	"""Called to consume a RabbitMQ message. We extract the CAN data,
	find the correct table, and add it."""
	# print("Handling packet "+str(pkt)+" of type "+str(type(pkt)))
	t = pkt.find(b't')
	try:
		if t is -1: raise ValueError # v = [time, addr, len, data]
		v = (int(float(pkt[0:t])), int(pkt[t+1:t+4], 16),
			int(bytes([pkt[t+4]]).decode(), 16), pkt[t+5:].decode()) # PY2K v[2]
		print("Handling packet "+str(pkt)+" of type "+str(type(pkt)) +
			"    names: "+ db.name.get(v[1], '???') +" contents: "+ str(v[3]))
		for table in db.tables:
			for match in table.handlers:
				if match[0] in db.name.get(v[1], ''):
					table.add(match, v)
					break
			else: continue
			break
		else:
			print("Throwing out CAN packet: " + db.name.get(v[1], '???') +
				" (" + hex(v[1]) + "), contents: " + v[3])
	except (IndexError, ValueError):
		print("Unintelligible packet: " + str(pkt))
	finally:
		if ch is not None:
			ch.basic_ack(method.delivery_tag)
			if halt: ch.stop_consuming()

def receive(callback = handle):
	cxn = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = cxn.channel()
	channel.queue_declare(queue = config.afferent_server_inbox)
	print('Consumer has connected.')
	channel.basic_consume(callback, queue = config.afferent_server_inbox)
	channel.start_consuming()
	cxn.close()

def stop(num, frame):
	print('Consumer stopping...')
	global halt
	halt = True
	time.sleep(2)
	sys.exit() # TODO cleaner quit

def run():
	"""Run the Consumer. Must be run by laptop.py in a separate process."""
	signal.signal(signal.SIGINT, stop)
	while not halt:
		time.sleep(3)
		try: receive() # WARNING drops every temporary row on crash
		except (pika.exceptions.AMQPError, OSError): pass
