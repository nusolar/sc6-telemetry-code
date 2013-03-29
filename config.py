# Copyright Alex Chandel, 2013. All rights reserved.

# CANUSB device file dictionary. the key is sys.platform
files = {'darwin': '/dev/tty.usbserial-LWR8N2L2', 'linux2': '/dev/ttyUSB0'}
loop_delay = 4

# laptop RMQ config
rmq_dir = '/usr/local/sbin/'
rmq_logging = False

# Server hostnames
server_name = 'mbr.chandel.net'
client_name = 'rpi.chandel.net'
# Car --> Laptop
afferent_client_outbox = 'car_outbox'
afferent_server_inbox = 'laptop_inbox'
# Laptop --> Car
efferent_consumable = 'laptop_outbox'
efferent_publish = 'car_inbox'

# Database location
db_relative_path = '/Desktop/telemetry.db'
default_period = 1
