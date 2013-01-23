#!/usr/bin/env python

import threading, subprocess, pika, psutil, db


halt = False
def receive():
	con = db.con()
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	def callback(ch, method, properties, pkt):
		t = pkt.find('t')
		v = (float(pkt[0:t]),int('0x'+pkt[t+1:t+4],16),int('0x'+pkt[t+5:],16))
		print v
		con.execute("INSERT INTO packets VALUES (?,?,?)", v)
		con.commit()
		if halt: channel.close()
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: channel.start_consuming()
	except: channel.close()



class workers:
	con = db.con()
	def db():
		con.execute("CREATE TABLE IF NOT EXISTS packets(date real, cid integer, data integer)")
	def rabbitmq():
		for p in psutil.process_iter():
			if p.name == 'rabbitmq-server': return True
		return False
	def rabbitmq_start():
		if db():
			subprocess.Popen('rabbitmq-server')
	def rabbitmq_stop():
		if rabbitmq() and not consumer(): subprocess.Popen(['rabbitmqctl','stop'])
	consumer_t = None
	def consumer():
		consumer_t.isAlive()
	def consumer_start():
		if not consumer() and db() and (rabbitmq() or rabbitmq_start()):
			halt = False
			consumer_t = threading.Thread(target = receive)
			consumer_t.start()
	def consumer_stop():
		if consumer(): halt = True
	def quit():
		consumer_stop()
		consumer.con.close()
		rabbitmq_stop()
		analysis.con.close()
