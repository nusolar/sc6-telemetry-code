#!/usr/bin/env python

import os, sqlite3

#make DbConn factory
def con(): return sqlite3.connect(os.path.expanduser('~') + '/Desktop/packets.db')
