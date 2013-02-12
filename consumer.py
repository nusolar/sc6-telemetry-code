#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, db, sys

halt = False
con = db.con()

def descr(time, addr, data): #[char*4, uint32] 16 HEX'S TOTAL!!!
	return ("descr", (time, addr, data[0:8].decode('hex'), int(data[8:],16)))
def tripPt(time, addr, data): #[float low, float high]
	return ("tripPt", (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def trips(time, addr, data): #[int32, uint32] ERROR Signed Int NOT HANDLED
	return ("trips", (time, addr, int(data[0:8],16), int(data[8:],16)))
def modules(time, addr, data): # [uint32, float]
	return ("modules", (time, addr, int(data[0:8],16), float.fromhex(data[8:])))
def circuit_d(time, addr, data): #[double count]
	return ("modules", (time, addr, 0, float.fromhex(data)))
def can_bms_tx_current(time, addr, data): #[float array, float batt]
	row = ("modules", (time, addr, 0, float.fromhex(data[0:8])))
	con.execute("INSERT INTO %s VALUES (?,?,?,?)" % row[0], row[1])
	row[1][3] = float.fromhex(data[8:])
	return row
def sw(time, addr, data): #[11 bits, 21 unused] or [12 bits, 20 unused]
	bits = bin(int(data,16))[2:] #bin -> "0b0123456789AB..."
	variants = ("Left Right Yes No Maybe Haz Horn CEn CMode CUp CDown", #11 buttons
		"Left Right Marconi Yes Haz CEn CUp Maybe No Horn CMode CDown") #12 lights
	opts = variants['lights' in db.name[addr]].split(' ')
	flags = ' '.join(opt for (bit,opt) in zip(bits[:len(opts)],opts) if bit=='1')
	return ("sw", (time, addr, bits, flags))
def cmds(time, addr, data): #[float, float] TODO
	return ("cmds",  (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def motor(time, addr, data): #[float Re, float Im]
	return ("motor", (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def motor_swapped(time, addr, data): #[float Im, float Re]
	return ("motor", (time, addr, float.fromhex(data[8:]), float.fromhex(data[0:8])))
def mppt(time, addr, data):
	pass #TODO
def dc(time, addr, data):
	pass
def other(time, addr, data): #should never be called
	print "Unrecognized CAN packet: "+db.name[addr]+" ("+addr+")."
	return ("other", (time, addr, data, None))

handlers = ((('_heartbeat','_id','_error'),descr), #all hb, id, errors?
		  (('bms_tx_trip_pt_',), tripPt), #3 trip_pt
		  (('_trip', '_batt_bypass', '_last_reset'), trips), #int32 codes

		  (('bms_tx_voltage','bms_tx_owvoltage','bms_tx_temp'), modules), #float bms
		  (('_uptime','_cc_array','_cc_batt','_wh_batt'), circuit_d), #double bms
		  (('can_bms_tx_current',), can_bms_tx_current), #float*2 bms (last bms)

		  (('sw_',),sw), #remaining sw
		  (('_cmd', 'dc_rx_cruise_velocity_current'),cmds), #ws cmds,
		  (('_phase','_vector','_backemf',),motor_swapped), #backwards ws
		  (('ws20_tx_',),motor), #remaining ws
		  (('mppt_'),mppt), #WARNING mppt_rx unhandled
		  (('dc_'),dc), #remaining dc
		  ((''),other), ) #should never catch anything

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
