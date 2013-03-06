# Copyright Alex Chandel, 2013. All rights reserved.
import pika

def send():
	con1 = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	chan1 = con1.channel()
	chan1.queue_declare(queue='transmitter')
	con2 = pika.BlockingConnection(pika.ConnectionParameters(host='rpi.chandel.net'))
	chan2 = con2.channel()
	chan2.queue_declare(queue='transmitter')
	def callback(ch, method, properties, pkt):
		chan2.basic_publish(exchange='',routing_key='transmitter',body=pkt)
	chan1.basic_consume(callback,queue='transmitter',no_ack=True)
	try: chan1.start_consuming()
	except: con1.close(); con2.close()

def run():
	try: send()
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(4)
	finally: pass
