telem
=====

NUSolar SC6 telemetry code

Prerequisites
-------------
POSIX operating system

Python ≥ 3.3

* (car-side only requires ≥ 3.0)

* pika ≥ 0.9.8

* pyserial ≥ 2.6

RabbitMQ ≥ 3.0.2

* Do not configure RabbitMQ's server to run automatically. Telemetry assumes it's installed to ```/usr/local/sbin/```

* g++ ≥ 4.7.1

  NOTE: Most distributions do not provide a new enough g++.
  If this is the case for you, then you will need to install and use clang/LLVM (below) to compile the telemetry code.
  However, you should still follow these directions to install build-essential as it will be needed to compile the dependencies.


  In Debian/Ubuntu/Mint, can be installed with

  ```
  sudo apt-get install build-essential
  ```

Installation
------------
Install the dependencies.

Clone this project to desired installation directory.

In ```config.py```, set the server & laptop hostname, and the CANUSB device location for your ```sys.platform```.

Usage
-----

* ```cd``` to the installation directory
* type ```./laptop.py```
* to run self-test, type ```./test.py```

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

* EVERYTHING is tested, obviating the need for factorial hardware testing. Tests include:

  * all communication: car-side packet sending, server-side packet reception, server-side sending, and car-side reception.

  * ```laptop.py```'s multiprocess management

  * analytics math, including: GeoLocation-->Luminosity; Time-of-Day/Year-->Luminosity; Luminosity-->Received Power; Car-Temperature-->Received Power; and Applied Power-->Velocity.

  * WebServer reachability
