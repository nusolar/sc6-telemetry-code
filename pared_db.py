#!/usr/bin/env python3
import os, sqlite3, inspect, config, logging

db_path = os.path.expanduser('~/Desktop/data.db')
sql = sqlite3.connect(db_path)

class Table:
	def __init__(self, name, period = config.default_period, handler = None):
		self.name = name
		# create SQL table & temporary row
		sql.execute(_create[name])
		self.cols = {colname: num for colname, num in ((x[1], x[0])
			for x in sql.execute('PRAGMA table_info(%s)' % name).fetchall())}
		self.clear_row()
		# generate and cache SQL strings:
		self._insert = "INSERT INTO %s VALUES (%s)" % (name, ','.join(['?'] * len(self.cols)))
		self._select = "SELECT * FROM " + self.name + " ORDER BY time %s LIMIT %s OFFSET %s"
		self._select_last = "SELECT * FROM %s ORDER BY time DESC LIMIT 1" % self.name
		# attach custom behavior
		self.handlers = _handlers[name]
		self.period = period
		if inspect.isfunction(handler):
			self.add = handler

	def add(self, match, v):
		if self.row[0] is None: self.row[0] = v[0]
		if v[0] < self.row[0]:
			return
		elif v[0] - self.period > self.row[0]:
			logging.debug('Commiting, old ' + str(self.row[0]) + ', new ' +
				str(v[0]) + ', packet: ' + str(v[3]))
			self.commit()
			self.row[0] = v[0] # WARNING should 0/null row? overflow, but could overwrite nulls
		match[1](self, match, v[3])
	def clear_row(self):
		self.row = [None] * len(self.cols)
	def commit(self, row = None):
		sql.execute(self._insert, self.row if row is not tuple else row)
		sql.commit()
	def count(self):
		return sql.execute("SELECT COUNT(*) FROM " + self.name).fetchone()[0]
	def last(self): # TODO combine accessor methods into __getitem__(self,key)
		return sql.execute(self._select_last).fetchone()
	def __getitem__(self, key):
		"""x[key]
		key is an int, slice, or tuple

		x[-1] -> the last/most recent row (row vector)
		x[-5:] -> the last 5 rows (matrix)
		x[-1,0] -> the 0th column of the last row (scalar)
		x[-1,:] <==> x[-1] (row vector)
		x[:,0] -> the 0th column (col vector)
		x[-5:,:] <==> x[-5:] (matrix)

		x[slice m, slice n] -> a matrix!
		x[int m, slice n] -> a vector!
		x[slice m, int n] -> a vector!
		x[int m, int n] -> a scalar!

		Backwards slices like x[5:0] are not supported!
		"""
		# non-tuple requests to tuples
		if type(key) is int:
			key = (key, slice(None)) # int --> single row, all cols
		elif type(key) is slice:
			key = (key, slice(None)) # slice --> matrix of rows, all cols
		# scalar to vector
		slices = [slice(k, k+1) if type(k) is int else k for k in key]
		# TODO Test for backwards slices / Flip forwards
		if slices[0].start is None:
			slices[0] = slice(0, slices[0].stop) # x[:5] -> x[0:5], x[:-1] -> x[0:-1]
		if slices[0].stop is None or slices[0].stop == 0:
			# x[0:] -> x[0:end], x[-1:] -> x[-1:end], -0.5 := end
			slices[0] = slice(slices[0].start, -0.5)
		# TODO Efficient 1 row
		# TODO Efficient 1 column
		ascLimOff = None
		if slices[0].start>=0 and slices[0].stop>=0: # positive slice
			ascLimOff = ["ASC", slices[0].stop - slices[0].start, slices[0].start]
			if slices[0].stop == -0.5:
				ascLimOff[1] = -1 # -> LIMIT ALL
		elif slices[0].start<0 and slices[0].stop<0: # negative slice
			sl = slice(int(-slices[0].stop), -slices[0].start) # int(--0.5) == 0 -> OFFSET 0
			ascLimOff = ("DESC", sl.stop - sl.start, sl.start)
		elif slices[0].start>=0 and slices[0].stop<0: # positive to negative
			ascLimOff = ["ASC", self.count()+slices[0].stop - slices[0].start, slices[0].start]
			if slices[0].stop == -0.5:
				ascLimOff[1] = -1 # -> LIMIT ALL

		matrix = sql.execute(self._select % tuple(ascLimOff)).fetchall() # apply row selection
		if slices[0].start<0 and slices[0].stop<0: # must row-reverse for negative slices
			matrix = reversed(matrix)

		# matrix = [r[slices[1]] for r in matrix] # apply col selection, PRESERVE n-by-1-MATRIX
		matrix = [r[key[1]] for r in matrix] # apply col selection. COLLAPSE IF int col index
		if type(key[0]) is int: # int row index --> COLLAPSE TO VECTOR
			matrix = matrix[0] # yields a scalar if both indices are integers

		return matrix


