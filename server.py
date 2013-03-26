#!/usr/bin/env python3
# Copyright Alex Chandel, 2013. All rights reserved.
import time, db, json, signal, threading as th
try:
	from http.server import HTTPServer, BaseHTTPRequestHandler
except ImportError:
	from BaseHTTPServer import HTTPServer, BaseHTTPRequestHandler

def telemetry():
	row = db.tables[0].last()
	# SQL queries:
	d = {"bms":{"I":1, "CC":6, "Wh":7, "uptime":1}, "bms_V": [.421], "bms_T": [39], "bms_owV": [.421], 
		 "array":{"I":3, "CC":8}, "sw":{"buttons":0, "lights":0}, "ws": {"v":23, "I":2, "V":20, "T":50, "e":31}, 
		 "mppt":{"T":40, "I":1}, "time":row[0]}
	return {"telemetry": d}
def populate():
	return {"send": [(k, db.addr[k]) for k in sorted(db.addr.keys())]}

api = {"populate": populate, "telemetry": telemetry}

class Handler(BaseHTTPRequestHandler):
	def log_request(self, code='-', size='-'):
		pass
	def do_HEAD(s):
		s.send_response(200)
		s.send_header("Content-type", "application/json")
		s.end_headers()
	def do_GET(s):
		"""Respond to a GET request."""
		s.send_response(200)
		s.send_header("Content-type", "application/json")
		s.end_headers()
		# "http://www.example.com/foo/bar/" --> s.path == "/foo/bar/"
		val = json.dumps(api[s.path.split('?')[0].split('/')[1]]())
		s.wfile.write('callback( %s )' % val)

def run():
	httpd = HTTPServer(('0.0.0.0', 8080), Handler)
	httpd_th = th.Thread(target = httpd.serve_forever)
	def stop(num, frame):
		httpd.shutdown()
		httpd.server_close()
		print(time.asctime(), "HTTP Server Stops")
	signal.signal(signal.SIGINT, stop)
	print(time.asctime(), "HTTP Server Starts")
	httpd_th.start()

if __name__ == '__main__': run()
