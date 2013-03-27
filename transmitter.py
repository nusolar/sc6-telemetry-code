# Copyright Alex Chandel, 2013. All rights reserved.
import pika, time, config, signal, sys

halt = False
def send():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue = config.efferent_consumable)
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host=config.client_name))
	chan2 = con2.channel()
	chan2.queue_declare(queue = config.efferent_publish)
	print('Transmitter has connected.')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish('', config.efferent_inbox, pkt)
		if halt:
			chan1.stop_consuming()
			con1.close()
			con2.close()
		ch.basic_ack(method.delivery_tag)
	chan1.basic_consume(callback, queue = config.efferent_consumable)
	chan1.start_consuming()

def stop(num, frame):
	print('Transmitter stopping...')
	global halt
	halt = True
	time.sleep(2)
	sys.exit()

def run():
	"""Run the Transmitter. Must be run by laptop.py in a separate process."""
	signal.signal(signal.SIGINT, stop)
	while not halt:
		time.sleep(3)
		try: send()
		except (pika.exceptions.AMQPError, OSError): pass
