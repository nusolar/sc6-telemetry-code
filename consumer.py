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

def descr(time, addr, data): #[4 char, uint32]
	return ("descr", (time, addr, data[0:8].decode('hex'), int(data[8:],16)))
def trips(time, addr, data): #TODO trip member orders
	pass
def can_bms_tx_current(time, cid, data): #[float array, float batt]
	pass #+-> modules table?
def modules(time, addr, data): #assume [uint32, float], 16 CHAR TOTAL!!!
	return ("modules", (time, addr, int(data[0:8],16), float.fromhex(data[8:])))
def circuit_d(time, addr, data): #[double count]
	return ("modules", (time, addr, 0, float.fromhex(data)))
def sw(time, addr, data):
	pass #COMPLICATED TODO
def cmds(time, addr, data): #[float, float] TODO
	return ("cmds",  (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def motor(time, addr, data): #[float Re, float Im]
	return ("motor", (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def motor_swapped(time, addr, data): #[float Im, float Re]
	return ("motor", (time, addr, float.fromhex(data[8:]), float.fromhex(data[0:8])))
def mppt(time, addr, data):
	pass #COMPLICATED TODO
handlers = ((('_heartbeat','_id'),descr),
		  (('bms_tx_voltage','bms_tx_owvoltage','bms_tx_temp'), modules),
		  (('_uptime','_cc_array','_cc_batt','_wh_batt'), circuit_d),
		  (('can_bms_tx_current',), can_bms_tx_current),
		  (('_cmd', 'dc_rx_cruise_'),cmds),
		  (('_phase','_vector','_backemf',),motor_swapped),
		  (('ws20_tx_',),motor),)

def receive():
	cxn = pika.BlockingConnection(pika.ConnectionParameters(host='chandel.org'))
	channel = cxn.channel()
	channel.queue_declare(queue='telemetry')
	
	def quit():
		cxn.close(); sys.exit()
	def callback(ch, method, properties, pkt):
		t = pkt.find('t')
		v = [float(pkt[0:t]), int(pkt[t+1:t+4],16), int(pkt[t+4]), pkt[t+5:]]
		for pair in handlers:
			if any(x in db.name[v[1]] for x in pair[0]):
				v.append(pair[1](v[0],v[1],v[3])) #(time, id, str)
		if v[4][0] > '':
			con.execute("INSERT INTO %s VALUES (?,?,?,?)" % v[4][0], v[4][1])
			con.commit()
		if halt: quit()
	
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: channel.start_consuming()
	except: quit()
