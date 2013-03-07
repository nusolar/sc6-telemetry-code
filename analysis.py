#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import db

con = db.con()
def time(t):
	c = con.execute("SELECT time FROM data ORDER BY time DESC LIMIT 1")
	return c.fetchone()[0]
def derived():
	num = db.tables.data.last()
	(volts, max(volts), min(volts), sum(volts)/len(volts),
		temps, max(temps), min(temps), sum(temps)/len(temps),
		bms_array_c, mppt_c, speed, current, current/speed)

rho_data = [[-25,	-20,	-15,	-10,	-5,	0,	5,	10,	15,	20,	25,	30,	35],
	[1.4224,	1.3943,	1.3673,	1.3413,	1.3163,	1.2922,	1.269,	1.2466,	1.225,	1.2041,	1.1839,	1.1644,	1.1455]]
rho = lambda T: 1.2978 - 0.0046 * T
rho = lambda T,P: P/287.058/(273.15+T)
rho = lambda T,P,H: P/287.058/(273.15+T) + H* 10**(8.07131-1730.63/(233.426+T))*101325/760/461.495/(273.15+T) #T[ºC],P[Pa], H[]

Af = 1.16864768
Cd = 0.105636
Crr = 0.004 #?

P = lambda v,T: .5*rho(T)* v**3 *Af*Cd  +  W(v)*Crr* v
