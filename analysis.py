#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import db, datetime, math

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
rho = lambda T,P,H: P/287.058/(273.15+T) + H* 10**(8.07131-1730.63/(233.426+T))*101325/760/461.495/(273.15+T) #T[dC],P[Pa], H[]

Af = 1.16864768
Cd = 0.105636
Crr = 0.00475 #@100kph, 6kg/cm^2

P = lambda v,T: .5*rho(T)* v**3 *Af*Cd  +  W(v)*Crr* v

L_ref = 0.300 #W/cm^2
T_ref = 30 #dC
Isc_ref = 0.14 #A
I_ref = lambda V: Isc_ref*(1-V^5/1)
#regression to alpha, beta, K, Rs

Delta_Isc = lambda L, T: Isc_ref*(L/L_ref-1) + alpha*(T-T_ref)
I = lambda V, L, T: I_ref(V) + Delta_Isc(L, T)
V = lambda V, L, T: V - beta*(T-T_ref) - Delta_Isc(L,T)*Rs - K*(T-T_ref)*I(V,L,T)

Ilamb = lambda I0, theta: I0 * cos(theta)
#Ia = Ir + Ib + Ig
P = lambda I,V: I*V

def theta(longitude, latitude):
	utcnow = datetime.datetime.utcnow()
	n_days = (utcnow - datetime.datetime(2000, 1, 1, 12, 0, 0, 0))
	n_days = n_days.days + float(n_days.seconds)/86400 #num Julian days
	ec_Lm = (280.460 + 0.9856474*n_days)%360 #mean longitude
	ec_g = (357.528 + 0.9856003)%360 #mean anomaly
	ec_L = ec_Lm + 1.915*math.sin(ec_g) + 0.020*math.sin(2*ec_g) #ecliptic longitude
	R = 1.00014 - 0.01671*math.cos(ec_g) + 0.00014*math.cos(2*ec_g) #orbital radius
	e = 23.439 - 0.0000004*n_days #obliquity
	asc = math.atan(math.cos(e)*math.tan(ec_L)) #right ascension
	decl= math.asin(math.sin(e)*math.sin(ec_L)) #declination

	utcAngleFromNoon = (utcnow - datetime.datetime.combine(utcnow,datetime.time(0))).total_seconds()*7.2921150*10**-5
	car = [utcAngleFromNoon + longitude, latitude]
	return math.acos(math.cos(car[0])*math.sin(car[1])*math.sin(decl) + math.cos(car[1])*math.cos(decl)) #special case of dot product

class vector (list):
	def __mul__(l,r): return sum([x+y for x,y in zip(l,r)])
