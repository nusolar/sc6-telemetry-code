telem
=====

NUSolar ZELDA - telemetry code

Warning
----
This README is mostly out of date. The telemetry is no longer in Python.

Getting Prerequisites
-------------
* Mono or equivalent .NET framework
* Web browser

Installation
------------
Clone this repository into the desired installation directory on the BeagleBoard.

The ```/driver-server``` and ```/driver-gui``` subprojects will be run on the car's onboard Linux board, the BeagleBoard-xM. Tools for bootstrapping the BeagleBoard can be found in the ```/beagleboard``` folder.

Edit ```Config.cs``` and record the laptop-server & car-client hostnames and the CANUSB device location for your `sys.platform`.

Usage
-----

First `cd` to the installation directory
* to start telemetry: type ```./laptop.py```
* to do self-testing: type ```./test.py```
* to talk to the BeagleBoard over SerialUSB: connect cable and type `./beagleboard.sh`

Advanced Configuration
----------------------

We can run arbitrary tasks and self-tests.

laptop.py — the central task manager

* Defines ```class Task```, wrapping a function. Also defines the ```laptop.roll``` tuple, which holds every ```Task``` instance to be run.

  Current tasks include the RabbitMQ server "```rmq```", the packet consumer "```rmq_consumer```", the custom-packet transmitter "```rmq_producer```", and the web API "```json_server```".

  To add a task, append a new ```Task``` instance to ```laptop.roll```'s definition. The ```Task``` constructor requires a function handle.

consumer.py — the inbox

* Incoming packets are passed to a ```Table``` in ```db.tables``` for processing. Also defines functions to handle various Packet Data Types (e.g. 2 floats, 2 int32's, 1 int64, etc).

db.py — the database controller

* Defines ```class Table```, wrapping a SQL table. Also defines the ```db.tables``` tuple, which holds active ```Table``` instances.

  To add a custom packet table, add an entry to ```db._names```, ```db._sql```, and ```db._handlers```, and append a ```Table``` instance to ```db.tables```'s definition. Current tables heavily rely on the Packet Data Type functions in ``consumer.py``.

car.py — remote data collection

* The magic numbers in ```car.py``` are currently configured for py3k. To run ```car.py``` unit tests in py2x, substitute them for the commented values.

config.py — configuration options

* Defines CANUSB location, (laptop-side) RabbitMQ details, car & laptop hostnames, Message Queue names, DB file location, and various time-delays.

test.py — powerful unit tests

* EVERYTHING is tested, obviating the need for much hardware testing. Tests include:

  * all communication: car-side packet sending, server-side packet reception, server-side sending, and car-side reception.

  * ```laptop.py```'s multiprocess management

  * analytics math, including: GeoLocation-->Luminosity; Time-of-Day/Year-->Luminosity; Luminosity-->Received Power; Car-Temperature-->Received Power; and Applied Power-->Velocity.

  * WebServer reachability
