#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import subprocess as sp, consumer, transmitter, server, config
import multiprocessing as mp, sys, os, signal

class Task:
	"""Wraps a function, and runs it in a child process."""
	def __init__(self, name, runnable, kill = None):
		self.name = name
		self.run = runnable
		self.kill = (lambda: os.kill(self.p.pid, signal.SIGINT)) if kill is None else kill
		self.p = mp.Process()
	def on(self):
		return self.p.is_alive()
	def __str__(self):
		return self.name + " is " + ("on" if self.on() else "off")
	def stop(self):
		if self.on(): self.kill()
	def start(self):
		if not self.on():
			self.p = mp.Process(target = self.run)
			self.p.start()

### NEW TASKS HERE, IN ORDER OF DEPENDENCE ###
def rmq_main():
	signal.signal(signal.SIGINT, signal.SIG_IGN)
	sp.Popen([config.rmq_dir+'rabbitmq-server'], stdout = None if config.rmq_logging else sp.DEVNULL, start_new_session=True).wait()

roll = (Task('rmq', rmq_main, kill = lambda: sp.Popen([config.rmq_dir + 'rabbitmqctl', 'stop']).wait()),
		Task('rmq_consumer', consumer.run),
		Task('rmq_producer', transmitter.run),
		Task('json_server', server.run),)

def quit(num = None, frame = None):
	print('Task Manager quitting...')
	for worker in reversed(roll):
		worker.stop()

def begin():
	"""Main Telemetry function! Spawns all Tasks in roll in separate processes."""
	for worker in roll:
		worker.start()
		print(worker)

if __name__ == '__main__':
	sys.tracebacklimit = 3
	try: begin() # chill until all worker Processes terminate
	except (KeyboardInterrupt, SystemExit): quit()
	finally:
		signal.signal(signal.SIGINT, quit)
		signal.signal(signal.SIGTERM, quit)
