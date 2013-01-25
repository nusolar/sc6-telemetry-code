#!/usr/bin/env python

import os, sqlite3

def ready():
	con.execute("CREATE TABLE IF NOT EXISTS volts(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS temps(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS currents(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS energies(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS trips(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS motorinfo(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS carinfo(date real, cid integer, data integer)")
	con.execute("CREATE TABLE IF NOT EXISTS other(date real, cid integer, data integer)")

#make DbConn factory
def con(): return sqlite3.connect(os.path.expanduser('~') + '/Desktop/packets.db')
