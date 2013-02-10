#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import consumer, analysis, multiprocessing as mp, subprocess as sp, psutil, db

class rmq:
	p = mp.Process()
	def on(self):
		if p.is_alive():
			return p.pid
		for ps in psutil.process_iter(): 
			if ps.name == 'rabbitmq-server': return True
		return False
	def start(self):
		if db.ready() and not on():
			p = mp.Process(target = lambda: sp.Popen(['rabbitmq-server']).wait())
			p.start()
	def stop(self):
		if p.is_alive(): sp.Popen(['rabbitmqctl','stop'])
class rmq_consumer:
	p = mp.Process()
	def on(self): return p.is_alive()
	def start(self):
		if db.ready() and rmq.on() and not on():
			p = mp.Process(target = consumer.receive)
			p.start()
	def stop(self): p.terminate()
# Need to hire a JsonServer
# Hire an Analyst

roll = [rmq(), rmq_consumer()]
def begin():
	for worker in roll: worker.start()
def quit():
	for worker in roll: worker.stop()

if __name__ == '__main__':
	begin() #now Python chills until all worker Processes terminate
