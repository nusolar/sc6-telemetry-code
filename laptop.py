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
	
	class consumer:
		p = mp.Process()
		def on(): return p.is_alive()
		def start():
			if db.ready() and rmq.on() and not on():
				p = mp.Process(target = consumer.receive)
				p.start()
		def stop(): p.terminate()
	
	def begin():
		pass

	def quit():
		consumer_stop()
		consumer.con.close() #should also kill RMQ conn
		#rabbitmq_stop()
		analysis.con.close()
