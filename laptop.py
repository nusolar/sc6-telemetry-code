#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import subprocess as sp, consumer, transmitter, server
import multiprocessing as mp, db, sys, os, signal

class task:
	def __init__(self, runnable, kill = lambda: os.kill(self.p.pid, signal.SIGINT)):
		self.run = runnable
		self.kill = kill
		self.p = mp.Process()
	def on(self): return self.p.is_alive()
	def stop(self):
		if self.on(): self.kill()
	def start(self):
		if not self.on():
			self.p = mp.Process(target = self.run)
			self.p.start()

### NEW TASKS HERE ###
roll = {'rmq':			task(lambda: sp.Popen(['rabbitmq-server']).wait(), lambda:sp.Popen(['rabbitmqctl','stop'])),
		'rmq_consumer':	task(consumer.run),
		'rmq_producer': task(transmitter.run),
		'json_server':	task(server.run)}

def begin():
	for key,worker in roll.iteritems():
		worker.start()
		print key + " is " + ("on" if worker.on() else "off")
def quit():
	for worker in roll: worker.stop()

if __name__ == '__main__':
	sys.tracebacklimit = 3
	try: begin() #chill until all worker Processes terminate
	except (KeyboardInterrupt, SystemExit): quit()
	finally: pass
