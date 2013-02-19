#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import BaseHTTPServer, time, db, json
db.ready()
c = db.con()

def telemetry():
	bms_I 	= c.execute('SELECT * FROM modules WHERE cid==%s AND module==%s ORDER BY time DESC' % (db.addr['bms_tx_current'], 0)).fetchone()
	array_I = c.execute('SELECT * FROM modules WHERE cid==%s AND module==%s ORDER BY time DESC' % (db.addr['bms_tx_current'], 100)).fetchone()
	
	#SQL queries:
	d = {"bms":{"I":1, "CC":6, "Wh":7, "uptime":1}, "bms_V": [.421], "bms_T": [39], "bms_owV": [.421], 
		 "array":{"I":3, "CC":8}, "sw":{"buttons":0, "lights":0}, "ws": {"v":23, "I":2, "V":20, "T":50, "e":31}, 
		 "mppt":{"T":40, "I":1}}
	return {"telemetry": d}
def populate():
	return {"send": [(k, db.addr[k]) for k in sorted(db.addr.iterkeys())]}

api = {"populate": populate, "telemetry": telemetry}

class Handler(BaseHTTPServer.BaseHTTPRequestHandler):
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
		val = json.dumps( api[ s.path.split('?')[0].split('/')[1] ]() )
		s.wfile.write('callback( %s )' % val)
def run():
	httpd = BaseHTTPServer.HTTPServer(('0.0.0.0', 8080), Handler)
	print time.asctime(), "Server Starts"
	try:
		httpd.serve_forever()
	except (KeyboardInterrupt, SystemExit): pass
	finally: pass
	httpd.server_close()
	print time.asctime(), "Server Stops"

if __name__ == '__main__': run()