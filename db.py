#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import os, sqlite3, inspect, config, logging
from consumer import *

db_path = os.path.expanduser('~') + config.db_relative_path
def con(): return sqlite3.connect(db_path)

sql = con()
class Table:
	def __init__(self, name, period = config.default_period, handler = None):
		self.name = name
		self.handlers = _handlers[name]

		sql.execute(_create[name])
		self.cols = {colname: num for colname, num in ((x[1], x[0]) for x in sql.execute('PRAGMA table_info(%s)' % name).fetchall())}
		self.clear_row()
		self._insert = "INSERT INTO %s VALUES (%s)" % (name, ','.join(['?'] * len(self.cols))) # generate and save
		self._select = "SELECT * FROM " + self.name + " ORDER BY time %s LIMIT %s OFFSET %s" # cache sql select string
		self._select_last = "SELECT * FROM %s ORDER BY time DESC LIMIT 1" % self.name

		self.period = period
		if inspect.isfunction(handler): self.add = handler
	def add(self, match, v):
		if self.row[0] is None: self.row[0] = v[0]
		if v[0] < self.row[0]:
			return
		elif v[0] - self.period > self.row[0]:
			logging.debug('Commiting, old '+str(self.row[0])+', new '+str(v[0])+', packet: '+str(v[3]))
			self.commit()
			self.row[0] = v[0] # WARNING should zero/null row? Analysis could overwrite nulls, but could overflow
		match[1](self, match, v[3])
	def clear_row(self):
		self.row = [None] * len(self.cols)
	def commit(self):
		sql.execute(self._insert, self.row)
		sql.commit()
	def count(self):
		return sql.execute("SELECT COUNT(*) FROM " + self.name).fetchone()[0]
	def last(self): # TODO combine accessor methods into __getitem__(self,key)
		return sql.execute(self._select_last).fetchone()
	def __getitem__(self, key):
		"""x[key]
		key is an int, slice, or tuple

		x[-1] -> the last/most recent row (row vector)
		x[-5:] -> the last 5 rows (matrix)
		x[-1,0] -> the 0th column of the last row (scalar)
		x[-1,:] <==> x[-1] (row vector)
		x[:,0] -> the 0th column (col vector)
		x[-5:,:] <==> x[-5:] (matrix)

		Backwards slices x[5:0] are not supported!
		"""
		# non-tuple requests to tuples
		if type(key) is int:
			key = (key, slice(None)) # int --> single row, all cols
		elif type(key) is slice:
			key = (key, slice(None)) # slice --> matrix of rows, all cols
		# scalar to vector
		key = [slice(k, k+1) if type(k) is int else k for k in key]
		# TODO Test for backwards slices / Flip forwards
		if key[0].start is None:
			key = (slice(0, key[0].stop), key[1]) # x[:5] -> x[0:5], x[:-1] -> x[0:-1]
		if key[0].stop is None or key[0].stop == 0:
			key = (slice(key[0].start, -0.5), key[1])	 # wat. x[0:] -> x[0:len], x[-1:] -> x[-1:len], x[-1:0] -> x[-1:]
		# TODO Efficient 1 row
		# TODO Efficient 1 column
		ascLimOff = None
		if key[0].start>=0 and key[0].stop>=0: # 0 anchored
			ascLimOff = ["ASC", key[0].stop - key[0].start, key[0].start]
			if key[0].stop == -0.5: ascLimOff[1] = -1
		elif key[0].start<0 and key[0].stop<0: # -1 anchored
			sl = slice(int(-key[0].stop), -key[0].start) # int(--0.5)==0
			ascLimOff = ("DESC", sl.stop - sl.start, sl.start)
		elif key[0].start>=0 and key[0].stop<0: # 0->-1 spanning
			ascLimOff = ["ASC", self.count() + key[0].stop - key[0].start, key[0].start]
			if key[0].stop == -0.5: ascLimOff[1] = -1

		matrix = sql.execute(self._select % tuple(ascLimOff)).fetchall()
		if key[0].start<0 and key[0].stop<0: # -1 anchored, second correction
			matrix = reversed(matrix)
		return [r[key[1]] for r in matrix] # apply column selection

_names = ('data', 'cmds', 'trip', 'error')
_create = {
	'data': "CREATE TABLE IF NOT EXISTS data(time real, bms_bypass int, bms_I real, bms_CC real, bms_Wh real, \
		V1 real, V2 real, V3 real, V4 real, V5 real, V6 real, V7 real, V8 real, V9 real, V10 real, V11 real, V12 real, V13 real, V14 real, V15 real, V16 real, \
		V17 real, V18 real, V19 real, V20 real, V21 real, V22 real, V23 real, V24 real, V25 real, V26 real, V27 real, V28 real, V29 real, V30 real, V31 real, V32 real, \
		T1 real, T2 real, T3 real, T4 real, T5 real, T6 real, T7 real, T8 real, T9 real, T10 real, T11 real, T12 real, T13 real, T14 real, T15 real, T16 real, \
		T17 real, T18 real, T19 real, T20 real, T21 real, T22 real, T23 real, T24 real, T25 real, T26 real, T27 real, T28 real, T29 real, T30 real, T31 real, T32 real, \
		array_I real, array_CC real, \
		mppt_tx real, \
		mc_Rpm real, mc_Vel real, mc_Iim real, mc_Ire real, mc_Vim real, mc_Vre real, mc_Tin real, mc_Tsink real, mc_emf real, mc_e real, \
		sw_b int, sw_l int)",
	'cmds': "CREATE TABLE IF NOT EXISTS cmds (time real, mc_driveVel real, mc_driveI real, mc_power real, \
		dc_horn bit, dc_leftSig bit, dc_rightSig bit, dc_reverse bit, dc_cruiseEn bit, dc_cruiseVel real, dc_cruiseI real)",
	'trip': "CREATE TABLE IF NOT EXISTS trip (time real, code int, module int, Ilow real, Ihi real, Vlow real, Vhi real, Tlow real, Thi real)",
	'error': "CREATE TABLE IF NOT EXISTS error (time real, message text)"}
_handlers = {
	'data': (('bms_tx_batt_bypass',	int2, 'bms_bypass', ''), # bms_tx_last_reset[int32,:] & bms_tx_last_trip[int32,uint32] unhandled
			('bms_tx_current',	float2, 'array_I', 'bms_I'),
			('_cc_batt', 		double, 'bms_CC'),
			('_wh_batt', 		double, 'bms_Wh'),
			('bms_tx_voltage',	modules, 'V'),
			('bms_tx_temp',		modules, 'T'),
			('bms_tx_owvoltage',modules, 'owV'), # WARNING Unimplemented
			('_cc_array', 		double, 'array_CC'),
			('ws20_tx_velocity',	float2, 'mc_Rpm', 'mc_Vel'),
			('ws20_tx_voltage_vector', float2, 'mc_Vim', 'mc_Vre'),
			('ws20_tx_current_vector', float2, 'mc_Iim', 'mc_Ire'),
			('ws20_tx_backemf',		float2, 'mc_emf', ''), # 2nd is 0 by definition
			('ws20_tx_sink_temp',	float2, 'mc_Tin', 'mc_Tsink'),
			('sw_',		int64, 'sw_b'),
			('sw_',		int64, 'sw_l'),
			('mppt_tx',	mppt, 'mppt_tx'), # WARNING mppt_rx unimplemented
			('_heartbeat',		trash),
			('_id',				trash)),
	'cmds':(('ws20_rx_drive_cmd',	float2, 'mc_driveVel', 'mc_driveI'),
			('ws20_rx_power_cmd',	float2, '', 'mc_power'),
			('ws20_rx_reset_cmd',	trash,	''), # bms_rx_reset_ unhandled
			('dc_rx_horn',			bit64,	'dc_horn'),
			('dc_rx_signals',		bit2,	'dc_leftSig', 'dc_rightSig'),
			('dc_rx_reverseEnable',	bit64,	'reverseEnabled'),
			('dc_rx_cruise',		bit64,	'cruiseEnabled'),
			('dc_rx_cruise_vel',	float2, 'dc_cruiseVel', 'dc_cruiseI'), # dc_rx_cruise_velocity_current
			('bms_rx_trip',			trip,	'',	''),), # WARNING unimplemented
	'trip':(('_uptime', 			double, 'bms_uptime'), # keep last
			('_tx_trip_pt_current',	float2, 'Ilow',	'Ihi'),
			('_tx_trip_pt_voltage',	float2,	'Vlow',	'Vhi'),
			('_tx_trip_pt_temp',	float2,	'Tlow',	'Thi'),
			('_tx_trip',			trip,	'code',	'module'),), # trip code
	'error': (('_error',			error,	'message'),)}

def error_handler(self, match, v):
	if self.row[0] is None: self.row[0] = v[0]
	match[1](self, match, v[3])
	if '\0' in self.row: self.commit()

tables = (Table('data'), Table('cmds'), Table('trip', period=5), Table('error', handler=error_handler))

#CAN_ADDRESSES.h
_bases = (0x200, 0x210, 0x300, 0x310, 0x500, 0x400, 0x710, 0x770, 0x110, 0x500)
_roots = ['_'.join([t, m, '']) for t in ('bms', 'sw', 'ws20', 'mppt', 'dc') for m in ('rx', 'tx')]
_groups=(['trip','reset_cc_batt','reset_cc_array','reset_cc_mppt1',
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
_temp = [[(r+end, id) for end, id in zip(gs, range(n, n+len(gs)))] for r, gs, n in zip(_roots, _groups, _bases)]
_idList = [canid for group in _temp for canid in group]
addr = dict(_idList) # can.addr[x] returns numerical address for name
name = {v: k for k, v in addr.items()} # returns name for int address
