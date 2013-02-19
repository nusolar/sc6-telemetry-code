#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import os, sqlite3
from collections import defaultdict

def con(): return sqlite3.connect(os.path.expanduser('~') + '/Desktop/packets.db')
def ready():
	sql = con()
	sql.execute("CREATE TABLE IF NOT EXISTS descr (time real, cid int, name text, data int)")
	#NOTE: TRIP CODE IS SIGNED INT
	sql.execute("CREATE TABLE IF NOT EXISTS tripPt(time real, cid int, low real, high real)")
	sql.execute("CREATE TABLE IF NOT EXISTS trips (time real, cid int, code int, module int)")
	#regular data
	sql.execute("CREATE TABLE IF NOT EXISTS modules(time real, cid int, module int, value real)")
	sql.execute("CREATE TABLE IF NOT EXISTS cmds (time real, cid int, vel real, cur real)")
	sql.execute("CREATE TABLE IF NOT EXISTS sw   (time real, cid int, bits text, flags text)")
	sql.execute("CREATE TABLE IF NOT EXISTS motor (time real, cid int, re real, im real)")
	sql.execute("CREATE TABLE IF NOT EXISTS mppt (time real, cid int, bits text, flags text)")
	sql.execute("CREATE TABLE IF NOT EXISTS other(time real, cid int, data text, unused null)")
	return True

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
