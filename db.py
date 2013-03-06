#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import os, sqlite3

def con(): return sqlite3.connect(os.path.expanduser('~') + '/Desktop/packets.db')
def ready():
	sql = con()
	sql.execute("""CREATE TABLE IF NOT EXISTS data(time real, bms_uptime real, bms_bypass int, bms_I real, bms_CC real, bms_Wh real, \
		V1 real, V2 real, V3 real, V4 real, V5 real, V6 real, V7 real, V8 real, V9 real, V10 real, V11 real, V12 real, V13 real, V14 real, V15 real, V16 real, \
		V17 real, V18 real, V19 real, V20 real, V21 real, V22 real, V23 real, V24 real, V25 real, V26 real, V27 real, V28 real, V29 real, V30 real, V31 real, V32 real, \
		T1 real, T2 real, T3 real, T4 real, T5 real, T6 real, T7 real, T8 real, T9 real, T10 real, T11 real, T12 real, T13 real, T14 real, T15 real, T16 real, \
		T17 real, T18 real, T19 real, T20 real, T21 real, T22 real, T23 real, T24 real, T25 real, T26 real, T27 real, T28 real, T29 real, T30 real, T31 real, T32 real, \
		array_I real, array_CC real, \
		mppt_tx real, \
		mc_Rpm real, mc_Vel real, mc_Iim real, mc_Ire real, mc_Vim real, mc_Vre real, mc_Tin real, mc_Tsink real, mc_emf real, mc_e real, \
		sw_b int, sw_l int)""")
	dataColumns = {name:num for name,num in ((x[1],x[0]) for x in sql.execute('PRAGMA table_info(data)').fetchall())}
	
	sql.execute("CREATE TABLE IF NOT EXISTS cmds (time real, mc_driveVel real, mc_driveI real, mc_power real, \
		dc_horn bit, dc_leftSig bit, dc_rightSig bit, dc_reverse bit, dc_cruiseEn bit, dc_cruiseVel real, dc_cruiseI real)")
	cmdsColumns = {name:num for name,num in ((x[1],x[0]) for x in sql.execute('PRAGMA table_info(cmds)').fetchall())}
	
	sql.execute("CREATE TABLE IF NOT EXISTS trip (time real, code int, module int, Ihi real, Ilow real, Vhi real, Vlow real, Thi real Tlow real)")
	tripColumns = {name:num for name,num in ((x[1],x[0]) for x in sql.execute('PRAGMA table_info(trip)').fetchall())}
	
	sql.execute("CREATE TABLE IF NOT EXISTS errors (time real, message text)")
	return True
dataColumns = {}
cmdsColumns = {}
tripColumns = {}


#CAN_ADDRESSES.h
bases = (0x200, 0x210, 0x300, 0x310, 0x500, 0x400, 0x710, 0x770, 0x110, 0x500)
roots = ['_'.join([t,m,'']) for t in ('bms','sw','ws20','mppt','dc') for m in ('rx','tx')]
groups= (['trip','reset_cc_batt','reset_cc_array','reset_cc_mppt1',
		  'reset_cc_mppt2','reset_cc_mppt3', 'reset_wh','reset_all'],
		 ['heartbeat','error','uptime','last_reset','batt_bypass',
		  'current',
		  'cc_array','cc_batt','cc_mppt1','cc_mppt2','cc_mppt3',
		  'wh_batt','wh_mppt1','wh_mppt2','wh_mppt3',
		  'voltage','owvoltage','temp',
		  'trip','last_trip', 'trip_pt_current','trip_pt_voltage','trip_pt_temp'],
		 
		 ['lights'],
		 ['heartbeat','error','buttons','lights'],
		 
		 ['dc_id','drive_cmd','power_cmd','reset_cmd'],
		 ['motor_id','status','bus_voltage_current','velocity',
		  'phase','voltage_vector','current_vector','backemf'
		  '15V_1pt65V','2pt5V_1pt2V','fanspeed','sink_motor_temp',
		  'airin_cpu_temp','airout_cap_temp','odom_bus_ah'],
		 
		 ['mppt1','mppt2','mppt3'],
		 ['mppt1','mppt2','mppt3'],
		 
		 ['horn','signals','cruise','cruise_velocity_current'],
		 ['drv_id'])
temp = [[(r+end,id) for end,id in zip(gs,range(n,n+len(gs))) ] for r,gs,n in zip(roots,groups,bases)]
idList = [canid for group in temp for canid in group]
addr = dict(idList) #can.addr[x] returns numerical address for name
name = {v:k for k,v in addr.items()} #returns name for int address
