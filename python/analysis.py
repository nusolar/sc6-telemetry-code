#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import datetime as dt
import time
import sys
import os
import numpy as np
import matplotlib.pyplot as pyplot
from scipy.optimize import broyden1
import dropbox
import json
import csv


if (sys.version_info.major < 3) or (sys.version_info.major == 3 and sys.version_info.minor < 3):
	print("ERROR: OUTDATED PYTHON BUILD!\nYou're using "+str(sys.version[0:5]) +
		'\nPython 3.3+ required')

print("USE UTC FOR ALL TIMES! e.g. datetime(2014, 1, 1, 0, 0, 0, 0, timezone.utc)")


class Dropbox:
	app_key = "yd7c6qd3cl323p8"
	app_secret = "ptbp0vjz6721wto"

	def __init__(self):
		try:
			with open('Config.json', 'r') as config_fp:
				config = json.load(config_fp)
				self.access_token = config['access_token']
		except Exception as e:
			print('EXCEPTION: ' + str(e))
			print('Could not read access_token from Config.json')
			self.authorize()
			print('Add\t"access_token"='+self.access_token+"\t to Config.json")
		self.client = dropbox.client.DropboxClient(self.access_token)
		print('linked account: ' + self.client.account_info()["display_name"])

	def authorize(self):
		flow = dropbox.client.DropboxOAuth2FlowNoRedirect(self.app_key, self.app_secret)
		# Have the user sign in and authorize this token
		authorize_url = flow.start()
		print('1. Go to: ' + authorize_url)
		print('2. Click "Allow" (you might have to log in first)')
		print('3. Copy the authorization code.')
		code = input("Enter the authorization code here: ").strip()
		# This will fail if the user enters an invalid authorization code
		self.access_token, user_id = flow.finish(code)

	def pull_raw_data(self):
		# list Dropbox data
		folder_metadata = self.client.metadata('/')
		paths = [file['path'] for file in folder_metadata['contents']
			if not file['is_dir'] and 'solarcar-' in file['path'] and '.json' in file['path']]
		print("Dropbox:\tPulling " + str(len(paths)) + " files")
		for path in paths:
			# pull files to current directory
			f, metadata = self.client.get_file_and_metadata(path)
			with open('./' + path, 'wb') as out:
				out.write(f.read())
			# delete the Dropbox copy
			_metadata = self.client.file_delete(path)

	def read_data(self, name):
		headers = None
		data = []
		if not os.path.isdir(name + '-rawdata'):
			os.mkdir(name + '-rawdata')
		for path in os.listdir("."):
			if ('solarcar-' in path and '.json' in path):
				with open(path) as fp:
					_json = json.load(fp)
					if _json["count"] != len(_json["data"]):
						print("WARNING: check file " + path)
					if headers is None:
						headers = _json["headers"]
					for line in _json["data"]:
						data.append(line)
				os.rename(path, name + '-rawdata/' + path)

		with open(name + '.csv', 'w') as csvfile:
			writer = csv.writer(csvfile)
			writer.writerow(headers)
			for line in data:
				writer.writerow(line)


pi = 3.14159265358979323846264338327950
R = 8314.4621  # PER KILOMOL


def array_power(L=1365, T=59):
	"""The array's solar power (via MPPTs) for a given Luminosity and array Temp"""
	L_ref = 0.300  # W/cm^2
	T_ref = 30  # dC
	Isc_ref = 0.14  # A
	I_ref = lambda V: Isc_ref*(1-V**5/1)
	alpha, beta, K, Rs = (0, 0, 0, 0)
	Delta_Isc = lambda L, T: Isc_ref*(L/L_ref-1) + alpha*(T-T_ref)
	I = lambda V, L, T: I_ref(V) + Delta_Isc(L, T)
	V = lambda V, L, T: V - beta*(T-T_ref) - Delta_Isc(L, T)*Rs - K*(T-T_ref)*I(V, L, T)
	# Ia = Ib + Ig + Ir
	return (0.3382+0.3493+0.3603)*L  # [W] @ 59dC


class ArrayPower:
	solar_constant = 1364.1  # W/m^2 at 1 AU
	J2000 = dt.datetime(2000, 1, 1, 12, 0, 0, 0, tzinfo=dt.timezone.utc).timestamp()

	def solar_radius(self, unix_time=time.time()):
		'''Returns the orbital radius of the Earth around the Sun at a given time.

		unix_time: the Unix timestamp of the desired time
		return: the orbital radius [AU]
		'''
		n_days = (unix_time - self.J2000)/86400  # num solar days since J2000

		ec_Lm = (280.460 + 0.9856474*n_days) % 360  # mean longitude [degrees]
		ec_g = (357.528 + 0.9856003*n_days) % 360 * pi/180  # mean anomaly [rad]
		ec_L = (ec_Lm + 1.915*np.sin(ec_g) + 0.020*np.sin(2*ec_g)) * pi/180  # ecliptic long [rad]
		return 1.00014 - 0.01671*np.cos(ec_g) + 0.00014*np.cos(2*ec_g)  # orbital radius [AU]

	# 110.9625W, 48.7317N
	def cos_sun(self, unix_time=time.time(), theta_long=-97.6411*pi/180, phi_lat=30.1328*pi/180):
		"""Computes the cosine of the angle between the Earth's surface and the Sun,
		at a given time, longitude, and latitude.

		unix_time: the Unix timestamp of the desired time
		theta_long: angle from the Prime Meridian, radians
		phi_lat: angle from the North Pole, radians
		"""
		n_days = (unix_time - self.J2000)/86400  # num solar days since J2000

		ec_Lm = (280.460 + 0.9856474*n_days) % 360  # mean longitude [degrees]
		ec_g = (357.528 + 0.9856003*n_days) % 360 * pi/180  # mean anomaly [rad]
		ec_L = (ec_Lm + 1.915*np.sin(ec_g) + 0.020*np.sin(2*ec_g)) * pi/180  # ecliptic long [rad]
		# R = 1.00014 - 0.01671*np.cos(ec_g) + 0.00014*np.cos(2*ec_g)  # orbital radius [AU]

		e = (23.439 - 0.0000004*n_days) * pi/180  # obliquity [rad]

		# asc = np.arctan(np.cos(e)*np.tan(ec_L))  # right asc [rad, -pi/2 pi/2]
		phi2 = pi/2 - np.arcsin(np.sin(e)*np.sin(ec_L))  # pi/2 - declination [rad, -pi/2 pi/2]

		# car's location on earth, spherical coordinates
		car_theta = ((unix_time + theta_long*86400/(2*pi)) % 86400 - 43200)*pi/43200
		car_phi = phi_lat

		# special dot product
		return np.sin(car_phi)*np.sin(phi2)*np.cos(car_theta) + np.cos(car_phi)*np.cos(phi2)

	def __init__(self, car: "SolarCar"):
		self.car = car

	def radiant_flux(self, time, geo):
		radii = self.solar_radius(time)
		cosines = self.cos_sun(time, *geo.spherical())
		return self.solar_constant/radii**2 * cosines

	def __call__(self, time, geo, weather):
		car = self.car
		return car.area * self.radiant_flux(time, geo)


class PowerLoss:
	rho_data = [[-25, -20, -15, -10, -5, 0, 5, 10, 15, 20, 25, 30, 35],
		[1.4224, 1.3943, 1.3673, 1.3413, 1.3163, 1.2922,
		1.269, 1.2466, 1.225, 1.2041, 1.1839, 1.1644, 1.1455]]
	rho_corl = lambda T: 352.9732/(273.15+T)

	def rho_air(self, T=25, P=101325, H=0):  # T[dC],P[Pa], H[]
		P_h2o = H*10**(8.07131-1730.63/(233.426+T))*101325/760  # H20 Antoine Eqn
		return ((P-P_h2o)*28.964 + P_h2o*18.01528)/R/(273.15+T)  # Sum Air & H20 contributions

	def W_car(self, v, rho_air) -> "N":
		# W = m g - .5 rho v^2 (Ap CL)
		m = self.car.m
		Ap_CL = self.car.Ap_CL		# 0.01010724337
		return 9.8*m - rho_air * v**2 * Ap_CL / 2

	def drag(self, v, rho_air):
		Af = self.car.Af		# 1.16864768
		CD = self.car.CD		# 0.105636
		return rho_air * v**3 * Af * CD / 2

	def rolling_res(self, v, rho_air):
		"""Estimate power loss, from rolling resistance."""
		Crr = self.car.Crr		# 0.00475 @100kph, 6kg/cm^2
		# W' = .5 rho v^3 Af CD  +  W Crr v
		return self.W_car(v, rho_air) * Crr * v

	def __init__(self, car):
		self.car = car

	def __call__(self, v, weather) -> "W":  # [m/s], (T[dC], P[Pa], H[])
		"""Estimate parasitic power loss, from velocity"""
		rho_air = self.rho_air(*weather)
		return self.drag(v, rho_air) + self.rolling_res(v, rho_air)


class Location:
	def __init__(self, long=-110.9625, lat=41.2683, theta=None, phi=None):
		if (theta is None or phi is None):
			self.long = long
			self.lat = lat

	def spherical(self, long=-110.9625, lat=41.2683):
		"""Converts longitude, latitude to spherical coordinates.

		float long: W is negative, E is positive
		float lat: N is positive, S is negative

		-> tuple(theta, phi)
		float theta: azimuthal
		float phi: polar
		"""
		return (self.long*pi/180, (90-self.lat)*pi/180)


class TimeRange:
	race_day = 8*3600
	solar_day = 86400

	def __init__(self, start=dt.datetime(2014, 7, 17, 10, 0, 0, 0),
						end=dt.datetime(2014, 7, 17+2, 12+6, 0, 0, 0), end_t=None):
		'''Start your race at 10AM. This class assumes 7 hour race days. Resolution = 1 sec.'''
		if start.tzinfo != dt.timezone.utc:
			start.replace(tzinfo=dt.timezone.utc)
		self.start = start.timestamp()

		if end_t is not None:
			self.end = end_t
		else:
			if end.tzinfo != dt.timezone.utc:
				end.replace(tzinfo=dt.timezone.utc)
			self.end = end.timestamp()

		# include only race times
		self.intervals = []
		while self.start < self.end:
			if self.start + self.race_day < self.end:
				self.intervals.append([self.start, self.start+self.race_day])
			else:
				self.intervals.append([self.start, self.end])
			self.start += self.solar_day

		self._total = sum(b-a for a, b in self.intervals)

	def total(self):
		return self._total

	def __call__(self, res=100):
		self._array = np.array([np.r_[a:b:res] for a, b in self.intervals]).flatten()
		return self._array


def cuml_integral_left(f_prime, time):
	"Left-sum integrate f_prime. Assumes constant delta_x. "
	delta = time[1]-time[0]
	return np.cumsum(f_prime*delta)


class SC6:
	wheel_radius = 9 * 2.54 / 100  # [m]

	# drag
	Af = 1.16864768		# m^2 @ 60mph
	CD = 0.105636

	# lift
	Ap_CL = 0.01010724337		# m^2 @ 60mph

	# rolling
	Crr = 0.00475		# @ 100kph, 6kg/cm^2
	m = 306				# kg

	# array
	area = 5.9951287		# (0.3382+0.3493+0.3603)
	eff = 0.9242492889		# effective m^2 @ 69dC

	# batteries
	V_nom = 120			# V
	Q_nom = 34*3600		# Coulomb
	E_0 = V_nom*Q_nom


class SC6LeadAcids:
	wheel_radius = 9 * 2.54 / 100  # [m]

	# drag
	Af = 1.16864768		# m^2 @ 60mph
	CD = 0.105636

	# lift
	Ap_CL = 0.01010724337		# m^2 @ 60mph

	# rolling
	Crr = 0.00475		# @ 100kph, 6kg/cm^2
	m = 306 + 40		# kg

	# array
	area = (0.3382+0.3493+0.3603)		# effective m^2 @ 59dC

	# batteries
	V_nom = 120			# V
	Q_nom = 18*3600		# Coulomb
	E_0 = V_nom*Q_nom
	batt_R = 0.018*10		# Ohms


class COTA:
	location = Location(-97.6343, 30.1355)  # the COTA racetrack's coordinates
	length = 5513  # m == 3.427mi

clear = (25, 101325, 0)  # T[dC], P[Pa], H[]


class StrategyFsgp14:
	def __init__(self, car):
		self.car = car
		self.Pin = ArrayPower(car)
		self.Pout = PowerLoss(car)
		self.track = COTA
		self.time = TimeRange()

	def __call__(self):
		"""Cumulative radiant energy at FSGP, savefig."""
		self.pin = self.Pin(self.time(), self.track.location, clear)
		self.pin *= (self.pin >= 0)
		self.E_in = cuml_integral_left(self.pin, self.time())
		self.E_total = self.car.E_0 + self.E_in[-1]
		self.P_avg = self.E_total / self.time.total()
		print(str(self.E_total) + " J")
		print(str(self.time.total()) + " s")
		print(str(self.P_avg) + " W")
		v = broyden1(lambda v: self.Pout(v, clear) - self.P_avg, 10)
		return v


def FSGP():
	fsgp = StrategyFsgp14(SC6)
	v = fsgp()

	print(str(v) + " m/s")
	print(str(v * fsgp.time.total() / fsgp.track.length) + " laps")

	pyplot.plot(fsgp.time(), (fsgp.car.E_0 + fsgp.E_in) / 10**6)
	pyplot.xticks(list(range(0, int(fsgp.time.total()), int(fsgp.time.total()/10))))
	pyplot.xlim(min(fsgp.time()), max(fsgp.time()))
	pyplot.title("SC6 Solar Energy at FSGP")
	pyplot.xlabel("Hour of race (7 hr/day)")
	pyplot.ylabel("Megajoules received")
	pyplot.savefig('race_received_energy_2014.png')
	# pyplot.show()


def main():
	d = Dropbox()
	d.pull_raw_data()
	d.read_data('debug')
	# FSGP()

if __name__ == '__main__':
	main()
