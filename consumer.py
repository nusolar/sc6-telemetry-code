#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, sys, time

def modules(table, match, data): # [uint32, float]
	column = match[2] + str(int(data[0:8],16))
	table.row[table.cols[column]] = float.fromhex(data[8:])
def double(table, match, data): #[double count]
	table.row[table.cols[match[2]]] = float.fromhex(data)
def float2(table, match, data): #[float array, float batt]
	if match[2]: table.row[table.cols[match[2]]] = float.fromhex(data[:8])
	if match[3]: table.row[table.cols[match[3]]] = float.fromhex(data[8:])
def int64(table, match, data): #[11 bits, 21 unused] or [12 bits, 20 unused]
	table.row[table.cols[match[2]]] = int(data,16)
	#bits = bin(int(data,16))[2:] #bin -> "0b0123456789AB..."
	#opts = ("Left Right Yes No Maybe Haz Horn CEn CMode CUp CDown", #11 buttons 
	#	"Left Right Marconi Yes Haz CEn CUp Maybe No Horn CMode CDown")['lights' in db.name[addr]].split(' ') #12 lights
	#flags = ' '.join(opt for (bit,opt) in zip(bits[:len(opts)],opts) if bit=='1')
def int2(table, match, data):
	table.row[table.cols[match[2]]] = int(data[:8],16)
	if match[3]: table.row[table.cols[match[3]]] = int(data[8:],16)
def bit2(table, match, data):
	table.row[table.cols[match[2]]] = 0 if int(data[:8],16)==0 else 1
	table.row[table.cols[match[3]]] = 0 if int(data[8:],16)==0 else 1
def bit64(table, match, data):
	table.row[table.cols[match[2]]] = 0 if int(data,16)==0 else 1
def trip(table, match, data): #[int32, uint32] 
	s = int(data[:8],16)
	table.row[table.cols[match[2]]] = s if s < 2**32/2 else s-2**32
	table.row[table.cols[match[3]]] = int(data[8:],16)
def error(table, match, data): #[char*8]
	table.row[table.cols[match[2]]] += data.decode('hex')
def trash(table, match, data): #[char*4, uint32]
	pass

import db
halt = False

def receive():
	cxn = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = cxn.channel()
	channel.queue_declare(queue='telemetry')
	def handle(ch, method, properties, pkt):
		t = pkt.find('t') #v = [time, addr, len, data]
		v = (int(float(pkt[0:t])), int(pkt[t+1:t+4],16), int(pkt[t+4]), pkt[t+5:])
		for table in db.tables:
			for match in table.handlers:
				if match[0] in db.name.get(v[1],''):
					table.add(match, v)
					break
			else: continue
			break
		else: print "Unrecognized CAN packet: "+db.name.get(v[1],'???')+" ("+v[1]+"), " + v[3]
		ch.basic_ack(method.delivery_tag)
		if halt: channel.stop_consuming(); cxn.close()
	channel.basic_consume(handle, queue='telemetry')
	channel.start_consuming()

def run():
	try:
		while not halt:
			try: receive()
			except (pika.exceptions.AMQPError): pass
			finally: time.sleep(4)
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(4)
	finally: pass #WARNING drops all temporary rows on crash
