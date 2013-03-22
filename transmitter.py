# Copyright Alex Chandel, 2013. All rights reserved.
import pika, time

local_queue = 'laptop_outbox'
remote_queue = 'car_inbox'

halt = False
def send():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue=local_queue)
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host='rpi.chandel.net'))
	chan2 = con2.channel()
	chan2.queue_declare(queue=remote_queue)
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key=remote_queue,body=pkt)
		if halt:
			chan1.stop_consuming()
			con1.close()
			con2.close()
		ch.basic_ack(method.delivery_tag)
	chan1.basic_consume(callback, queue=local_queue)
	chan1.start_consuming()

def run():
	global halt
	try: 
		while not halt:
			try: send()
			except (pika.exceptions.AMQPError): pass
			finally: time.sleep(3)
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(1)
	finally: pass
