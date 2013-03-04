#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import subprocess as sp, consumer, server, multiprocessing as mp, psutil
import db, sys, os, signal

class rmq:
	def __init__(self): self.p = mp.Process()
	def on(self):
		if self.p.is_alive():
			return self.p.pid
		for ps in psutil.process_iter(): 
			if ps.name == 'rabbitmq-server': return True
		return False
	def start(self, group):
		if db.ready() and not self.on():
			self.p = mp.Process(target = lambda: sp.Popen(['rabbitmq-server']).wait())
			self.p.start()
	def stop(self):
		if self.p.is_alive(): sp.Popen(['rabbitmqctl','stop'])

class rmq_consumer:
	def __init__(self): self.p = mp.Process()
	def on(self): return self.p.is_alive()
	def start(self, group):
		if db.ready() and group[0].on() and not self.on():
			self.p = mp.Process(target = consumer.receive)
			self.p.start()
	def stop(self):
		if self.on(): os.kill(self.p.pid, signal.SIGINT)

class json_server:
	def __init__(self): self.p = mp.Process()
	def on(self): return self.p.is_alive()
	def start(self, group):
		if db.ready() and not self.on():
			self.p = mp.Process(target = server.run)
			self.p.start()
	def stop(self):
		if self.on(): os.kill(self.p.pid, signal.SIGINT)

roll = [rmq(), rmq_consumer(), json_server()]
def begin():
	for worker in roll:
		worker.start(roll)
		print worker.on()
def quit():
	for worker in roll: worker.stop()

if __name__ == '__main__':
	sys.tracebacklimit = 3
	try: begin() #now chill until all worker Processes terminate
	except (KeyboardInterrupt, SystemExit): quit()
	finally: pass
