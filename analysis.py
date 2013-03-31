#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import db, datetime as dt, time, numpy as np, pylab

con = db.con()
def last_time():
	c = con.execute("SELECT time FROM data ORDER BY time DESC LIMIT 1")
	return dt.datetime.utcnow() - c.fetchone()[0]
def derived():
	num = db.tables[0].last()
	(volts, max(volts), min(volts), sum(volts)/len(volts),
		temps, max(temps), min(temps), sum(temps)/len(temps),
		bms_array_c, mppt_c, speed, current, current/speed, num)

rho_data = [[-25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35],
	[1.4224, 1.3943, 1.3673, 1.3413, 1.3163, 1.2922,
	1.269, 1.2466, 1.225, 1.2041, 1.1839, 1.1644, 1.1455]]
rho_corl = lambda T: 352.9732/(273.15+T)
def rho_air(T, P, H): # T[dC],P[Pa], H[]
	P/287.058/(273.15+T) + H* 10**(8.07131-1730.63/(233.426+T))*101325/760/461.495/(273.15+T)
def W_car(v, T, P, H):
	306 + .5*rho_air(T, P, H)* v**2 *0.01010724337
def PowerOut(v, T, P, H): # [m/s],[Pa],[dC]
	.5*rho_air(T, P, H)* v**3 *1.16864768*0.105636 + W_car(v, T, P, H)*0.00475*v

pi = 3.14159265358979323846264338327950

def powerOut(v, T=25, P=101325, H=0): # [m/s], T[dC], P[Pa], H[]
	rho= P/287.058/(273.15+T)+ H*10**(8.07131-1730.63/(233.426+T))*101325/760/461.495/(273.15+T)
	# W = W0 + .5 rho v^2 (Ap CL)
	W = 306 + .5*rho* v**2 *0.01010724337
	Af = 1.16864768
	CD = 0.105636
	Crr = 0.00475 # @100kph, 6kg/cm^2
	# W' = .5 rho v^3 Af CD  +  W Crr v
	return .5*rho * v**3 *Af*CD + W*Crr* v

def arrayPower(L=1365, T=59):
	L_ref = 0.300 # W/cm^2
	T_ref = 30 # dC
	Isc_ref = 0.14 # A
	I_ref = lambda V: Isc_ref*(1-V^5/1)
	alpha, beta, K, Rs = (0, 0, 0, 0)
	Delta_Isc = lambda L, T: Isc_ref*(L/L_ref-1) + alpha*(T-T_ref)
	I = lambda V, L, T: I_ref(V) + Delta_Isc(L, T)
	V = lambda V, L, T: V - beta*(T-T_ref) - Delta_Isc(L, T)*Rs - K*(T-T_ref)*I(V, L, T)
	#Ia = Ib + Ig + Ir
	return (0.3382+0.3493+0.3603)*L # [W] @ 59dC

J2000 =dt.datetime(2000, 1, 1, 12, 0, 0, 0, tzinfo=dt.timezone.utc).timestamp()

def cosSun(unix_time = time.time(), thetaLong=-110.9625*pi/180, phi=48.7317*pi/180):
	"""Computes the cosine of the angle between the array and the sun,
	i.e. the proportion of solar radiation absorbed.

	unix_time *MUST* be a float. You must comprehend over cosSun. Don't pass
	lists or arrays. Ergo this function is slow.
	"""
	# [unix time] [rad] [rad]
	n_days = (unix_time - J2000)/86400 # num solar days since J2000
	# n_days = n_days.days + n_days.seconds/86400
	ec_Lm = (280.460 + 0.9856474*n_days)%360 # mean longitude [degrees]
	ec_g = (357.528 + 0.9856003*n_days)%360 *pi/180 # mean anomaly [rad]
	ec_L = (ec_Lm+ 1.915*np.sin(ec_g)+ 0.020*np.sin(2*ec_g)) *pi/180 # ecliptic long [rad]
	# R = 1.00014 - 0.01671*np.cos(ec_g) + 0.00014*np.cos(2*ec_g) # orbital radius [AU]
	e = (23.439 - 0.0000004*n_days) *pi/180 # obliquity [rad]
	# asc = np.arctan(np.cos(e)*np.tan(ec_L)) # right asc [rad, -pi/2 pi/2]
	phi2= pi/2 - np.arcsin(np.sin(e)*np.sin(ec_L)) # pi/2 - declination [rad, -pi/2 pi/2]

	utcnow = dt.datetime.utcfromtimestamp(unix_time) # .replace(tzinfo = dt.timezone.utc)
	utcAngleFromNoon= (utcnow-dt.datetime.combine(utcnow, dt.time(12))).total_seconds()/43200*pi
	car = [utcAngleFromNoon + thetaLong, phi] # car's location, earth, spherical coords
	# special dot product
	return np.sin(car[1])*np.sin(phi2)*np.cos(car[0])+ np.cos(car[1])*np.cos(phi2)

def timeRange(date = dt.datetime(2013, 6, 27), delta = dt.timedelta(3, 0, 0),
	numel = 100, step_size = None):
	"""Generate linspace'd list of Unix Time values, starting at date, ranging
	over delta.
	"""
	initial = date.timestamp()
	final = initial + delta.total_seconds()
	if step_size is None:
		step_size = (final - initial) / numel
	else:
		if np.sign(step_size) != np.sign(final - initial):
			step_size *= -1
	return np.r_[initial:final+step_size:step_size]

def geo2sph(long = -110.9625, lat = 41.2683):
	"""Converts longitude, latitude to spherical coordinates.

	float long: W is negative, E is positive
	float lat: N is positive, S is negative

	-> tuple(theta, phi)
	float theta: azimuthal
	float phi: polar
	"""
	return (long*pi/180, (90-lat)*pi/180)

def cuml_radiant_energy(trange, theta, phi):
	"""Trapezoidally integrate Radiant Flux on Earth (theta, phi) over trange.

	theta, phi: either float or numpy.ndarray. If an array, they must be
		*EXACTLY* as long as trange, or the longer will be truncated.
	"""
	def yielder(x):
		while True: yield x
	if type(theta) is float:
		theta = yielder(theta)
	if type(phi) is float:
		phi = yielder(phi)
	angles = np.array([max((cosSun(*x), 0)) for x in zip(trange, theta, phi)])
	powers = arrayPower(L = 1300*angles)
	np.cumsum(np.r_[0, (powers[:-1]+powers[1:])/2/np.diff(trange)])
	return np.cumsum(powers*(trange[1]-trange[0])) # left-corner approx

circuit_americas_geoloc = (-97.6343, 30.1355) # Circuit of the Americas

def demo():
	"""Compute cumul radiant energy at FSGP, savefig"""
	location = geo2sph(*circuit_americas_geoloc)
	date = dt.datetime(2013, 6, 27, 10) # race from 10 AM
	delta = dt.timedelta(0, 7*3600) # till 5 PM
	trange = [timeRange(date+dt.timedelta(1)*x, delta) for x in range(3)]
	trange_total = np.concatenate(trange)
	energies = cuml_radiant_energy(trange_total, *location)

	race_hour = [trange[0] - trange[0][0]]
	race_hour += [trange[1] - trange[1][0] + race_hour[0][-1]]
	race_hour += [trange[2] - trange[2][0] + race_hour[1][-1]]
	race_hour = np.concatenate(race_hour) / 3600

	pylab.plot(race_hour, energies / 10**6)
	pylab.xticks(range(int(max(race_hour))+1))
	pylab.xlim(min(race_hour), max(race_hour))
	pylab.title("SC6 Radiant Energy at FSGP")
	pylab.xlabel("Hour of race (7 hr/day)")
	pylab.ylabel("Megajoules received")
	pylab.savefig('race_received_energy.png')


