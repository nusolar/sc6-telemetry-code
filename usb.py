#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.

import serial

record = b'\x1e'
unit = b'\x1f'

class Com:
	def __init__(self, tty):
		self.tty = tty

	def write(self, key, value = None):
		with serial.Serial(self.tty) as ser:
			ser.write(record)
			ser.write(key)
			ser.write(unit)
			if type(value) is bytes:
				ser.write(value)
			ser.write(record)

	def read(self):
		with serial.Serial(self.tty) as ser:
			buffer = ser.read(1)
			if buffer == record:
				buffer = b''
				while buffer[len(buffer)-1] != record:
					buffer += ser.read(1)
				return buffer

	def on(self):
		self.write(b'on')

	def off(self):
		self.write(b'off')

	def toggle(self):
		self.write(b'togg')

	def stat(self):
		self.write(b'stat')
		return self.read()

	def name(self):
		self.write(b'name')
		return self.read()

tty = "/dev/ttyserial"
devices = {
	"head_l": Com(tty),
	"brake_l": Com(tty),
	"left_l": Com(tty),
	"right_l": Com(tty),
	"horn": Com(tty),

	"accel": Com(tty), # Analog In
	"regen": Com(tty), # Analog In
	"brake": Com(tty) # Digital In
}
