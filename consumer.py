#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, db, sys

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

def descr(time, name, data): #[4 char, uint32]
	row = (time, name, data[0:8].decode('hex'), int(data[8:],16))
	con.execute("INSERT INTO descr VALUES (?,?,?,?)", row)
def trips(time, name, data): #TODO
	pass
def modules(time, name, data): #assume [uint32, float], 16 CHAR TOTAL!!!
	row = (time, name, int(data[0:8],16), float.fromhex(data[8:]))
	con.execute("INSERT INTO modules VALUES (?,?,?,?)", row)
def circuit_d(time, cid, data): #[double count]
	pass #could add to modules table
def can_bms_tx_current(time, cid, data): #[float array, float batt]
	pass #+-> modules table?
def sw(time, cid, data): pass #COMPLICATED TODO
def cmds(time, cid, data): #TODO [float, float]?
	row = (time, name, float.fromhex(data[0:8]), float.fromhex(data[8:]))
	con.execute("INSERT INTO cmds VALUES (?,?,?,?)", row) 
def motor(time, name, data): #[float Re, float Im]
	row = (time, name, float.fromhex(data[0:8]), float.fromhex(data[8:]))
	con.execute("INSERT INTO motor VALUES (?,?,?,?)", row)
def motor_swapped(time, name, data): #[float Im, float Re]
	row = (time, name, float.fromhex(data[8:]), float.fromhex(data[0:8]))
	con.execute("INSERT INTO motor VALUES (?,?,?,?)", row)
def mppt(): pass #COMPLICATED TODO
tables = ((('_heartbeat','_id'),descr),
		  (('bms_tx_voltage','bms_tx_owvoltage','bms_tx_temp'), modules),
		  (('_uptime','_cc_array','_cc_batt','_wh_batt'), circuit_d),
		  (('can_bms_tx_current',), can_bms_tx_current),
		  (('_cmd', 'dc_rx_cruise_'),cmds),
		  (('_phase','_vector','_backemf',),motor_swapped),
		  (('ws20_tx_',),motor),)

def receive():
	connection = pika.BlockingConnection(pika.ConnectionParameters(host='mbr.chandel.net'))
	channel = connection.channel()
	channel.queue_declare(queue='telemetry')
	
	def quit():
		connection.close(); sys.exit()

	def callback(ch, method, properties, pkt):
		t = pkt.find('t')
		v = [float(pkt[0:t]), int(pkt[t+1:t+4],16), int(pkt[t+5:],16)]
		#   [float timestamp, int can_address, int data]
		for pair in tabulae:
			if any(x in db.name[v[1]] for x in pair[0]):
				table = pair[1]; break #get table from second element
		con.execute("INSERT INTO %s VALUES (?,?,?)" % table, v)
		con.commit()
		if halt: quit()
	
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: channel.start_consuming()
	except: quit()
