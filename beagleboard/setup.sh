#!/bin/bash
# BeagleBoard setup script, for new OS installations

cd "${HOME}"
mkdir "/usr/local/bin"

sudo apt-get update
sudo apt-get upgrade

sudo apt-get install locales
sudo apt-get install at


## configure USB Tethering
sudo tee "/usr/local/bin/dialer.sh" <<-EOF
	echo "sudo wvdial" | at now
EOF

sudo tee "/etc/udev/rules.d/tmobile.rules" <<-EOF
	SUBSYSTEM=="usb", ATTR{idVendor}=="19d2", ATTR{idProduct}=="1525", NAME="tmobile", RUN="/usr/local/bin/dialer.sh"
EOF


## configure telemetry
git clone git://github.com/nusolar/sc6-telemetry-code.git

sudo tee -a "/etc/inittab"  <<-EOF
	python3 /home/debian/sc6-telemetry-code/car.py
EOF
