# Copyright Alex Chandel, 2013. All rights reserved.
import pika, time, config

halt = False
def send():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host = 'localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue = config.efferent_consumable)
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host = config.client_name))
	chan2 = con2.channel()
	chan2.queue_declare(queue = config.efferent_publish)
	print('Transmitter has connected.')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key = config.efferent_inbox, body=pkt)
		if halt:
			chan1.stop_consuming()
			con1.close()
			con2.close()
		ch.basic_ack(method.delivery_tag)
	chan1.basic_consume(callback, queue = config.efferent_consumable)
	chan1.start_consuming()

def run():
	global halt
	time.sleep(2)
	try: 
		while not halt:
			try: send()
			except (pika.exceptions.AMQPError, OSError): pass
			finally: time.sleep(3)
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(1)
	finally: pass
