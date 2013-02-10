#!/usr/bin/env python

import consumer, analysis, threading, multiprocessing as mp, subprocess as sp, psutil, db

class workers:
	con = db.con()

	class rmq:
		p = mp.Process()
		def on():
			if p.is_alive():
				return p.pid
			for ps in psutil.process_iter(): 
				if ps.name == 'rabbitmq-server': return True
			return False
		def start():
			if db.ready() and not on():
				p = mp.Process(target = lambda: sp.Popen(['rabbitmq-server']).wait())
				p.start()
		def stop():
			if p.is_alive(): sp.Popen(['rabbitmqctl','stop'])
	
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
	
	def begin():
		pass

	def quit():
		consumer_stop()
		consumer.con.close() #should also kill RMQ conn
		#rabbitmq_stop()
		analysis.con.close()
