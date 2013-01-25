#!/usr/bin/env python

import pika, db, thread, can

halt = False
con = db.con()

tabulae = ((('trip',), 'trips'),
		   (('voltage',), 'volts'),
		   (('temp',), 'temps'),
		   (('bms_tx_current','ws20_tx_current_vector'), 'currents'),
		   (('tx_cc_','tx_wh_'), 'energies'),
		   (('ws20_',), 'motorinfo'),
		   (('horn','signals','cruise','lights'), 'carinfo'),
		   (('',), 'other'))

def receive():
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	def quit():
		channel.close(); connection.close(); thread.exit()
	def callback(ch, method, properties, pkt):
		t = pkt.find('t')
		v = [float(pkt[0:t]), int('0x'+pkt[t+1:t+4],16), int('0x'+pkt[t+5:],16)]
		for pair in tabulae:
			if any(x in can.name[v[1]] for x in pair[0]): table = pair[1]; break
		con.execute("INSERT INTO %s VALUES (?,?,?)" % table, v)
		con.commit()
		if halt: quit()
	
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: channel.start_consuming()
	except: quit()