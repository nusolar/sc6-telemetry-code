#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import datetime as dt, time, numpy as np, pylab
from scipy.optimize import broyden1

# con = db.con()
def last_time():
	c = con.execute("SELECT time FROM data ORDER BY time DESC LIMIT 1")
	return dt.datetime.utcnow() - c.fetchone()[0]
def derived():
	num = db.tables[0].last()
	(volts, max(volts), min(volts), sum(volts)/len(volts),
		temps, max(temps), min(temps), sum(temps)/len(temps),
		bms_array_c, mppt_c, speed, current, current/speed, num)

pi = 3.14159265358979323846264338327950
R = 8314.4621 # PER KILOMOL


rho_data = [[-25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35],
	[1.4224, 1.3943, 1.3673, 1.3413, 1.3163, 1.2922,
	1.269, 1.2466, 1.225, 1.2041, 1.1839, 1.1644, 1.1455]]
rho_corl = lambda T: 352.9732/(273.15+T)
def rho_air(T=25, P=101325, H=0): # T[dC],P[Pa], H[]
	P_h2o = H*10**(8.07131-1730.63/(233.426+T))*101325/760 # Antoine Eqn
	return ((P-P_h2o)*28.964 + P_h2o*18.01528)/R/(273.15+T) # Sum contributions
def W_car(v, T=25, P=101325, H=0):
	return 306 + .5*rho_air(T, P, H)* v**2 *0.01010724337


wheel_radius = 9 * 2.54 / 100 # [cm]


def powerOut(v, T=25, P=101325, H=0): # [m/s], T[dC], P[Pa], H[]
	"""Estimate parasitic power loss, from velocity"""
	P_h2o = H*10**(8.07131-1730.63/(233.426+T))*101325/760 # Antoine Eqn
	rho= ((P-P_h2o)*28.964 + P_h2o*18.01528)/R/(273.15+T) # Sum contributions
	# W = W0 + .5 rho v^2 (Ap CL)
	W = 9.8*306 - .5*rho* v**2 *0.01010724337
	Af = 1.16864768
	CD = 0.105636
	Crr = 0.00475 # @100kph, 6kg/cm^2
	# W' = .5 rho v^3 Af CD  +  W Crr v
	return .5*rho * v**3 *Af*CD + W*Crr* v


def array_power(L=1365, T=59):
	"""The array's solar power (via MPPTs) for a given Luminosity and array Temp"""
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


solar_constant = 1364.1 # W/m^2 at 1 AU


J2000 =dt.datetime(2000, 1, 1, 12, 0, 0, 0, tzinfo=dt.timezone.utc).timestamp()


def solar_radius(unix_time = time.time()):
	'''Returns the orbital radius of the Earth around the Sun at a given time.

	unix_time: the Unix timestamp of the desired time
	return: the orbital radius [AU]
	'''
	n_days = (unix_time - J2000)/86400 # num solar days since J2000

	ec_Lm = (280.460 + 0.9856474*n_days)%360 # mean longitude [degrees]
	ec_g = (357.528 + 0.9856003*n_days)%360 *pi/180 # mean anomaly [rad]
	ec_L = (ec_Lm+ 1.915*np.sin(ec_g)+ 0.020*np.sin(2*ec_g)) *pi/180 # ecliptic long [rad]
	return 1.00014 - 0.01671*np.cos(ec_g) + 0.00014*np.cos(2*ec_g) # orbital radius [AU]


# 110.9625W, 48.7317N
def cos_sun(unix_time = time.time(), theta_long=-97.6411*pi/180, phi_lat=30.1328*pi/180):
	"""Computes the cosine of the angle between the Earth's surface and the Sun,
	at a given time, longitude, and latitude.

	unix_time: the Unix timestamp of the desired time
	theta_long: angle from the Prime Meridian, radians
	phi_lat: angle from the North Pole, radians
	"""
	n_days = (unix_time - J2000)/86400 # num solar days since J2000

	ec_Lm = (280.460 + 0.9856474*n_days)%360 # mean longitude [degrees]
	ec_g = (357.528 + 0.9856003*n_days)%360 *pi/180 # mean anomaly [rad]
	ec_L = (ec_Lm+ 1.915*np.sin(ec_g)+ 0.020*np.sin(2*ec_g)) *pi/180 # ecliptic long [rad]
	# R = 1.00014 - 0.01671*np.cos(ec_g) + 0.00014*np.cos(2*ec_g) # orbital radius [AU]

	e = (23.439 - 0.0000004*n_days) *pi/180 # obliquity [rad]

	# asc = np.arctan(np.cos(e)*np.tan(ec_L)) # right asc [rad, -pi/2 pi/2]
	phi2= pi/2 - np.arcsin(np.sin(e)*np.sin(ec_L)) # pi/2 - declination [rad, -pi/2 pi/2]

	# car's location on earth, spherical coordinates
	car_theta = ((unix_time + theta_long*86400/(2*pi)) % 86400 - 43200)*pi/43200
	car_phi = phi_lat

	# special dot product
	return np.sin(car_phi)*np.sin(phi2)*np.cos(car_theta) + np.cos(car_phi)*np.cos(phi2)


def time_range(date = dt.datetime(2014, 7, 17, 7), delta = dt.timedelta(0, 12*3600, 0),
	numel = 100, step_size = None):
	"""Generate linspace'd array of Unix Time values, starting at date, ranging
	over delta.
	Default is July 17th 2014, 7 AM - 7 PM, in the local (Daylight Savings) timezone
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


COTA_location = geo2sph(-97.6343, 30.1355) # the COTA racetrack's coordinates


def cuml_solar_energy(trange, theta, phi):
	"""Trapezoidally integrate Solar Power on SC6 at (theta, phi) over trange.

	theta, phi: either float or numpy.ndarray. If an array, they must be
		*EXACTLY* as long as trange, or the longer will be truncated.
	"""

	radii = solar_radius(trange)
	cosines = cos_sun(trange, theta, phi)
	radiant_flux = solar_constant/radii**2 * cosines

	powers = array_power(L = radiant_flux)
	np.cumsum(np.r_[0, (powers[:-1]+powers[1:])/2/np.diff(trange)])
	return np.cumsum(powers*(trange[1]-trange[0])) # trapezoidal approx


def FSGP():
	"""Cumulative radiant energy at FSGP, savefig."""
	date = dt.datetime(2014, 7, 17, 10) # race from 10 AM
	delta = dt.timedelta(0, 7*3600) # till 5 PM
	trange = [time_range(date+dt.timedelta(1)*x, delta) for x in range(3)]
	trange_total = np.concatenate(trange)

	cuml_solar_E = cuml_solar_energy(trange_total, *americas_location)
	battery_E = [15.5 * 10**6]
	total_E = battery_E[0] + cuml_solar_E[-1]

	race_hour = [trange[0] - trange[0][0]]
	race_hour += [trange[1] - trange[1][0] + race_hour[0][-1]]
	race_hour += [trange[2] - trange[2][0] + race_hour[1][-1]]
	race_hour = np.concatenate(race_hour) / 3600

	P_avg = total_E / (race_hour[-1] * 3600)
	v = broyden1(lambda v: powerOut(v) - P_avg, 26)
	print(v)

	pylab.plot(race_hour, cuml_solar_E / 10**6)
	pylab.xticks(range(int(max(race_hour))+1))
	pylab.xlim(min(race_hour), max(race_hour))
	pylab.title("SC6 Solar Energy at FSGP")
	pylab.xlabel("Hour of race (7 hr/day)")
	pylab.ylabel("Megajoules received")
	pylab.savefig('race_received_energy.png')

