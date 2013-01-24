#!/usr/bin/env python

import db, can

con = db.con() #USE DIFFERENT SQL CONNECTION?
def cids():
	c = con.execute("select DISTINCT cid from packets")
	return c.fetchall().sort()
def packets(cid):
	if type(cid) is str: cid = can.addr[cid]
	c = con.execute("select * from packets where cid==? order by date asc", (cid,))
	return c.fetchall()
def packet(cid):
	if type(cid) is str: cid = can.addr[cid]
	c = con.execute("select * from packets where cid==? order by date desc", (cid,))
	return c.fetchone()
def time():
	c = con.execute("select date from packets order by date desc")
	return c.fetchone()[0]

def all_bms_volts():
	return packets('bms_tx_voltage')
def all_bms_temps():
	return packets('bms_tx_temp')
def all_speeds():
	return packets('ws20_tx_motor_velocity')
def all_motor_currents():
	return packets('ws20_tx_current_vector')

def array_current():
	bms_array_c = packet('bms_tx_current')[2] #Right packet for array current?
	mppt_c = packet('mppt_tx_mppt1')[2] #Right mppt current?
	return (bms_array_c, mppt_c)
def bms_volts():
	volts = packet('bms_tx_voltage')[2] #ERROR ignoring data processing
	return (volts, max(volts), min(volts), sum(volts)/len(volts))
def bms_temps():
	temps = packet('bms_tx_temp')[2] #ERROR ignoring data processing
	return (temps, max(temps), min(temps), sum(temps)/len(temps))
def motor_info():
	speed = packet('ws20_tx_motor_velocity')[2]
	current = packet('ws20_tx_current_vector')[2]
	return (speed, current, current/speed)