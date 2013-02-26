#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, db, sys, time

halt = False
con = db.con()
temp = (None,None)*(76/2)

def descr(time, addr, data): #[char*4, uint32] 16 HEX'S TOTAL!!!
	return ("descr", (time, addr, data[0:8].decode('hex'), int(data[8:],16)))
def tripPt(time, addr, data): #[float low, float high]
	return ("tripPt", (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def trips(time, addr, data): #[int32, uint32] ERROR Signed Int NOT HANDLED
	return ("trips", (time, addr, int(data[0:8],16), int(data[8:],16)))
def modules(match, data): # [uint32, float]
	column = match[2] + str(int(data[0:8],16))
	temp[db.columns[column]] = float.fromhex(data[8:])
def double(match, data): #[double count]
	temp[db.columns[match[2]]] = float.fromhex(data)
def float2(match, data): #[float array, float batt]
	temp[db.columns[match[2]]] = float.fromhex(data[:8])
	temp[db.columns[match[3]]] = float.fromhex(data[8:])
def sw(match, data): #[11 bits, 21 unused] or [12 bits, 20 unused]
	temp[db.columns[match[2]]] = int(data,16)
	#bits = bin(int(data,16))[2:] #bin -> "0b0123456789AB..."
	#opts = ("Left Right Yes No Maybe Haz Horn CEn CMode CUp CDown", #11 buttons
	#	"Left Right Marconi Yes Haz CEn CUp Maybe No Horn CMode CDown")['lights' in db.name[addr]].split(' ') #12 lights
	#flags = ' '.join(opt for (bit,opt) in zip(bits[:len(opts)],opts) if bit=='1')
def cmds(time, addr, data): #[float, float] TODO
	return ("cmds",  (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def mppt(time, addr, data):
	pass #TODO
def dc(time, addr, data):
	pass #TODO
def other(time, addr, data): #should never be called
	print "Unrecognized CAN packet: "+db.name.get(addr,'?')+" ("+addr+")."
	return ("other", (time, addr, data, None))

#bms_rx_reset_ unhandled
handlers = ((('_heartbeat','_id','_error'), descr), #all hb, id, errors?
			(('bms_tx_trip_pt_',), tripPt), #3 trip_pt
			(('_trip', '_batt_bypass', '_last_reset'), trips), #(int32, uint32) codes

			(('bms_tx_voltage','bms_tx_owvoltage','bms_tx_temp'), modules), #float bms
			(('_uptime','_cc_array','_cc_batt','_wh_batt'), circuit_d), #double bms
			(('bms_tx_current',), can_bms_tx_current), #float*2 bms (last bms)

			(('sw_',), sw), #remaining sw
			(('_cmd', 'dc_rx_cruise_velocity_current'), cmds), #ws cmds,
			(('_phase','_vector','_backemf'), motor_swapped), #backwards ws
			(('ws20_tx_',), motor), #remaining ws
			(('mppt_',), mppt), #WARNING mppt_rx unhandled
			(('dc_',), dc), #remaining dc
			(('',), other), ) #should never catch anything

handlers2= (('_uptime', double, 'bms_Uptime'),
			('bms_tx_current',float2, 'array_I', 'bms_I'),
			('_cc_batt', double, 'bms_CC'),
			('_wh_batt', double, 'bms_Wh'),
			('bms_tx_voltage',	modules, 'V'),
			('bms_tx_temp',		modules, 'T'),
			('bms_tx_owvoltage',modules, 'owV'),
			('_cc_array', double, 'array_CC'),
			('ws20_tx_velocity', float2, 'mc_Rpm', 'mc_Vel'),
			('ws20_tx_voltage_vector', float2, 'mc_Vim', 'mc_Vre'),
			('ws20_tx_current_vector', float2, 'mc_Iim', 'mc_Ire'),
			('ws20_tx_backemf', float2, 'mc_emf', ''), #first
			('ws20_tx_sink_temp', float2, 'mc_Tin', 'mc_Tsink'),
			('sw_', sw, 'sw_l'),
			)

def handle(pkt):
	t = pkt.find('t') #v = [time, addr, len, data]
	v = (float(pkt[0:t]), int(pkt[t+1:t+4],16), int(pkt[t+4]), pkt[t+5:])
	name = db.name.get(v[1],'')

	return

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
			if any(x in db.name.get(v[1],'') for x in pair[0]):
				v.append(pair[1](v[0],v[1],v[3])) #(time, id, str)
		if v[4][0] > '':
			con.execute("INSERT INTO %s VALUES (?,?,?,?)" % v[4][0], v[4][1])
			con.commit()
		if halt: quit()
	
	channel.basic_consume(callback,queue='telemetry',no_ack=True)
	try: channel.start_consuming()
	except: quit()

def run():
	try: receive()
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(4)
	finally: pass
