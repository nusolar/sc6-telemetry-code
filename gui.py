#!/usr/bin/env python

import Tkinter as tk, ttk

class Gui:
	root = None
	def __init__(self):
		self.root = tk.Tk()
		self.root.title("Solar Telemetry")
		frame = ttk.Frame(self.root, padding="3 12 3 12") #W N E S
		
		frame.grid(column=0, row=0)
		frame.columnconfigure(0, weight=1)
		frame.rowconfigure(0, weight=1)
		
		w = tk.Label(frame, text="Component Monitor")
		w.grid(column=2, row=1)
		w = tk.Label(frame, text="RabbitMQ Server")
		w.grid(column=1, row=2)
		w = tk.Label(frame, text="Packet Listener")
		w.grid(column=1, row=3)
		w = tk.Label(frame, text="Client Pulse")
		w.grid(column=1, row=4)
		
		w = tk.Label(frame, text="Component Monitor")
		w.grid(column=2, row=1)
		w = tk.Label(frame, text="RabbitMQ Server")
		w.grid(column=1, row=2)
		w = tk.Label(frame, text="Packet Listener")
		w.grid(column=1, row=3)
		w = tk.Label(frame, text="Client Pulse")
		w.grid(column=1, row=4)

		frame.pack()
		
		#tree = ttk.Treeview(frame)
		#tree.grid(column=2, row=2)
		#tree.insert('', 'end', text='A Packet!')