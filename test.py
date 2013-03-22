# Copyright Alex Chandel, 2013. All rights reserved.
import unittest, datetime, time
import laptop, analysis

class TestTelemetry(unittest.TestCase):
	def test_laptop(self):
		laptop.begin()
		time.sleep(5)
		laptop.quit()
		time.sleep(5)
		for key,worker in laptop.roll.iteritems():
			print(key + " is " + ("on" if worker.on() else "off"))
			self.assertFalse(worker.on())
	def test_angles(self):
		self.assertAlmostEqual(analysis.cosTheta(0,analysis.pi/2,datetime.datetime(2013,3,20,12,0,0)), 1)
		self.assertAlmostEqual(analysis.cosTheta(-analysis.pi,analysis.pi/2,datetime.datetime(2013,3,20,12,0,0)), -1)
	def test_drag(self):
		self.assertGreater(analysis.powerOut(10),analysis.powerOut(5))
	

if __name__ == '__main__':
	unittest.main()
