#!/bin/bash
# BeagleBoard setup script, for new OS installations

cd "${HOME}"
mkdir "/usr/local/bin"

sudo apt-get update
sudo apt-get upgrade

sudo apt-get install locales


echo WARNING configure Dynamic DNS - must be done manually
echo WARNING apt-get install ddclient
echo WARNING domain name: nusolar.no-ip.biz

## configure USB Tethering
sudo apt-get install at
sudo tee "/usr/local/bin/dialer.sh" <<-EOF
	echo "sudo wvdial" | at now
EOF

sudo tee "/etc/udev/rules.d/tmobile.rules" <<-EOF
	SUBSYSTEM=="usb", ATTR{idVendor}=="19d2", ATTR{idProduct}=="1525", NAME="tmobile", RUN="/usr/local/bin/dialer.sh"
EOF


## configure telemetry
sudo tee -a "/etc/inittab"  <<-EOF
	python3 /home/debian/SolarCar.exe
EOF
