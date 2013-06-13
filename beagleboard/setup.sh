#!/bin/bash
# BeagleBoard setup script, for new OS installations

cd "${HOME}"
# I assume you already did this:
# git clone git://github.com/nusolar/sc6-telemetry-code.git

sudo apt-get update
sudo apt-get upgrade

sudo apt-get install rabbitmq-server
sudo apt-get install python3 # currently 3.2.3-6
sudo apt-get install python3-serial
sudo apt-get install python3-setuptools

git clone "git://github.com/alexchandel/pika.git"
cd "pika"
sudo python3 "setup.py" install
cd ".."

sudo cat >> "/etc/inittab"  <<-EOF

	python3 /home/debian/sc6-telemetry-code/car.py

EOF
