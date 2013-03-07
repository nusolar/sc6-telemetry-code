#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import db

con = db.con()
def time(t):
	c = con.execute("SELECT time from data order by time desc")
	return c.fetchone()[0]

def derived():
	num = db.tables.data.last()
	(volts, max(volts), min(volts), sum(volts)/len(volts),
		temps, max(temps), min(temps), sum(temps)/len(temps),
		bms_array_c, mppt_c, speed, current, current/speed)
