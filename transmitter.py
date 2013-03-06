# Copyright Alex Chandel, 2013. All rights reserved.
import pika

halt = False
def send():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue='transmitter')
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host='rpi.chandel.net'))
	chan2 = con2.channel()
	chan2.queue_declare(queue='transmitter')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key='transmitter',body=pkt)
		if halt:
			chan1.stop_consuming()
			con1.close()
			con2.close()
		ch.basic_ack(method.delivery_tag)
	chan1.basic_consume(callback, queue='transmitter')
	chan1.start_consuming()

def run():
	try: 
		while not halt:
			try: send()
			except (pika.exceptions.AMQPError): pass
			finally: time.sleep(4)
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(4)
	finally: pass
