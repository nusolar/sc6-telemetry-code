#!/usr/bin/env python3
import serial
import struct
import json
import time
import threading as th

d = {}

def handle_line(line):
	# b'tiiiLdddddddddddddddd'
	id = line[1:4].decode()
	l = line[4]
	data = line[5:5+16].decode()
	databytes = bytes.fromhex(data)
	d[id] = [data, struct.unpack('<f', databytes[0:4])[0], struct.unpack('<f', databytes[4:8])[0]]


def hammer():
	halt = False

	with serial.Serial('/dev/tty.usbserial-LWR8N2L2') as ser:
		def newlines(buffer = b''):
			while not halt:
				while ser.isOpen() and halt is not None:
					buffer += ser.read(1)
					if buffer[-1] is 13: yield buffer; buffer = b''; break  # b'\r'[0]

		ser.write(b'S8\rO\r')
		for line in newlines():
			if line[0] != 116:  # b't'
				continue
			handle_line(line)


def printer():
	while True:
		j = json.dumps(d, indent='\t')
		d.clear()
		print(j)
		time.sleep(1)


def main():
	th.Thread(target = hammer).start()
	th.Thread(target = printer).start()

if __name__ == '__main__':
	main()
