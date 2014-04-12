#!/bin/bash

if [[ ! -e `which miniterm.py` ]]; then
	echo "You must have miniterm.py. This comes with the python 'pyserial'"
	echo "package. If Python 2 or Python 3 is installed,"
	echo "try 'pip install pyserial' or 'pip3 install pyserial."
fi

CANUSB_DEV=/dev/tty.usbserial-LWR8N2L2

miniterm.py --cr $CANUSB_DEV -b 115200
