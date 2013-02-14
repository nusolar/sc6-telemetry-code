#!/usr/bin/env python
# Copyright Alex Chandel, 2013. All rights reserved.
import BaseHTTPServer, time, db, json

def populate():
	return {"send": db.addr.items()}

api = {"populate": populate}

class Handler(BaseHTTPServer.BaseHTTPRequestHandler):
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
		val = json.dumps( api[s.path.split('?')[0].split('/')[1]]() )
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