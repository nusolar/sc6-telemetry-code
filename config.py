# Copyright Alex Chandel, 2013. All rights reserved.

server_name = 'mbr.chandel.net'
client_name = 'rpi.chandel.net'

# Car --> Laptop
afferent_client_outbox = 'car_outbox'
afferent_server_inbox = 'laptop_inbox'

# Laptop --> Car
efferent_consumable = 'laptop_outbox'
efferent_publish = 'car_inbox'
