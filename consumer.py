#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import pika, db, sys, time

halt = False
quit = None
con = db.con()

dataRow = tuple([None]*len(db.dataColumns))
cmdsRow = tuple([None]*len(db.cmdsColumns))
tripRow = tuple([None]*len(db.tripColumns))

def modules(match, data): # [uint32, float]
	column = match[2] + str(int(data[0:8],16))
	dataRow[db.dataColumns[column]] = float.fromhex(data[8:])
def double(match, data): #[double count]
	dataRow[db.dataColumns[match[2]]] = float.fromhex(data)
def float2(match, data): #[float array, float batt]
	dataRow[db.dataColumns[match[2]]] = float.fromhex(data[:8])
	dataRow[db.dataColumns[match[3]]] = float.fromhex(data[8:])
def int64(match, data): #[11 bits, 21 unused] or [12 bits, 20 unused]
	dataRow[db.dataColumns[match[2]]] = int(data,16)
	#bits = bin(int(data,16))[2:] #bin -> "0b0123456789AB..."
	#opts = ("Left Right Yes No Maybe Haz Horn CEn CMode CUp CDown", #11 buttons
	#	"Left Right Marconi Yes Haz CEn CUp Maybe No Horn CMode CDown")['lights' in db.name[addr]].split(' ') #12 lights
	#flags = ' '.join(opt for (bit,opt) in zip(bits[:len(opts)],opts) if bit=='1')
def int2(match, data):
	dataRow[db.dataColumns[match[2]]] = int(data[0:8],16)

def bit2():
	pass
def bit64():
	pass
def cmds(time, addr, data): #[float, float] TODO
	return ("cmds",  (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))

def tripPt(time, addr, data): #[float low, float high]
	return ("tripPt", (time, addr, float.fromhex(data[0:8]), float.fromhex(data[8:])))
def trip(time, addr, data): #[int32, uint32] ERROR Signed Int NOT HANDLED
	return ("trips", (time, addr, int(data[0:8],16), int(data[8:],16)))

def error(time, addr, data): #[char*4, uint32] 16 HEX'S TOTAL!!!
	return ("descr", (time, addr, data[0:8].decode('hex'), int(data[8:],16)))
def trash(match, data):
	pass
def other(match, addr, data): #should never be called
	print "Unrecognized CAN packet: "+db.name.get(addr,'???')+" ("+addr+"), " + data

# bms_tx_last_reset[int32,:] & bms_tx_last_trip[int32,uint32] unhandled
data = (('bms_tx_batt_bypass',	int2, 'bms_bypass', ''),
		('bms_tx_current',	float2, 'array_I', 'bms_I'),
		('_cc_batt', 		double, 'bms_CC'),
		('_wh_batt', 		double, 'bms_Wh'),
		('bms_tx_voltage',	modules, 'V'),
		('bms_tx_temp',		modules, 'T'),
		('bms_tx_owvoltage',modules, 'owV'), #Unimplemented
		('_cc_array', 		double, 'array_CC'),
		('ws20_tx_velocity',	float2, 'mc_Rpm', 'mc_Vel'),
		('ws20_tx_voltage_vector', float2, 'mc_Vim', 'mc_Vre'),
		('ws20_tx_current_vector', float2, 'mc_Iim', 'mc_Ire'),
		('ws20_tx_backemf',		float2, 'mc_emf', ''), #second is 0 by definition
		('ws20_tx_sink_temp',	float2, 'mc_Tin', 'mc_Tsink'),
		('sw_',		int64, 'sw_b'),
		('sw_',		int64, 'sw_l'),
		('mppt_tx',	int64, 'mppt_tx'), #56 bits int #WARNING mppt_rx is uncaught & unhandled
		('_heartbeat',		trash),
		('_id',				trash),
		('_error',			trash), #WARNING error packets?
		('',	other,	''), #should never catch anything
		)
#bms_rx_reset_ unhandled
cmds = (('ws20_rx_drive_cmd', float2, 'mc_driveVel', 'mc_driveI'),
		('ws20_rx_power_cmd', float2, '', 'mc_power'),
		('ws20_rx_reset_cmd',	trash, ''),
		('dc_rx_horn', bit64, 'dc_horn'),
		('dc_rx_signals', bit2, 'dc_leftSig', 'dc_rightSig'),
		('dc_rx_reverseEnable',bit64, 'reverseEnabled'),
		('dc_rx_cruise', bit64, 'cruiseEnabled'),
		('dc_rx_cruise_velocity_current', float2, 'dc_cruiseVel', 'dc_cruiseI'),
		('bms_rx_trip', trip), #trip code
		)
trip = (('_uptime', 	double, 'bms_uptime'), #keep last
		('tx_trip',		trip) #3 float2, 1 trip code
		)
tabulae = {"data":(data, dataRow, 1)}

def handle(ch, method, properties, pkt):
	t = pkt.find('t') #v = [time, addr, len, data]
	v = (float(pkt[0:t]), int(pkt[t+1:t+4],16), int(pkt[t+4]), pkt[t+5:])
	for name,table in tabulae.iteritems():
		row = table[1]
		period = table[2]
		for match in table[0]:
			if match[0] in db.name.get(v[1],''):
				if row[0]!=None and v[0]-period >= row[0]:
					con.execute( ''.join(["INSERT INTO %s VALUES ("%name, "?,"*(len(db.dataColumns)-1), "?)"]) , row)
				if v[0]-period >= row[0]:
					row[0] = v[0] #WARNING SHOULD NULLIFY ROW???
				match[1](match, v[3])
				break
		else: continue
		break
	if halt: quit()

def receive():
	cxn = pika.BlockingConnection(pika.ConnectionParameters(host='localhost'))
	channel = cxn.channel()
	channel.queue_declare(queue='telemetry')
	channel.basic_consume(handle, queue='telemetry', no_ack=True)
	quit = lambda: channel.stop_consuming(); cxn.close(); sys.exit()
	try: channel.start_consuming()
	except: quit()

def run():
	try: receive()
	except (KeyboardInterrupt, SystemExit): halt=True; time.sleep(4)
	finally: pass
