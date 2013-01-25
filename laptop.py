#!/usr/bin/env python

import consumer, analysis, threading, subprocess, psutil, db

class workers:
	con = db.con()

	def rabbitmq():
		for p in psutil.process_iter():
			if p.name == 'rabbitmq-server': return True
		return False
	def rabbitmq_start():
		if db.ready():
			subprocess.Popen('rabbitmq-server')
	def rabbitmq_stop():
		if rabbitmq() and not consumer(): subprocess.Popen(['rabbitmqctl','stop'])
	
	consumer_t = None
	def consumer():
		consumer_t.isAlive()
	def consumer_start():
		if db.ready() and not consumer(): #rabbitmq()
			consumer.halt = False
			consumer_t = threading.Thread(target = consumer.receive)
			consumer_t.start()
	def consumer_stop():
		if consumer(): consumer.halt = True
	
	def quit():
		consumer_stop()
		consumer.con.close() #should also kill RMQ conn
		#rabbitmq_stop()
		analysis.con.close()
