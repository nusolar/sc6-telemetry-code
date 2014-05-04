#!/usr/bin/env bash

if [[ $(basename "$(pwd)") != "driver-gui" ]]; then
	echo "COMPILE.sh must be run from the 'driver-gui' folder."
	exit
fi

if [[ ! -e $(which coffee) ]]; then
	echo "The CoffeeScript compiler 'coffee' is required. Try installing it with 'npm'."
	echo "'npm' is the Node Package Manager, installed with Node.js."
	echo "In OS X, Node.js can be installed with Homebrew, 'the OS X package manager'."
	exit
fi

if [[ ! -e $(which coffee) ]]; then
	echo "The Jade compiler 'Jade' is required. Try installing it with 'npm'."
	echo "'npm' is the Node Package Manager, installed with Node.js."
	echo "In OS X, Node.js can be installed with Homebrew, 'the OS X package manager'."
	exit
fi

coffee -c index.coffee
jade -P index.jade

echo "All files compiled and inlined into 'index.html'."
echo "The single-page, single-file website is ready."
echo
