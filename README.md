telem
=====

NUSolar SC6 telemetry code
 
Prerequisites
-------------
POSIX operating system

Python >= <del>2.7.2</del> 3.0

* pika >= 0.9.8

* pyserial >= 2.6

RabbitMQ >= 3.0.2

* Do not configure RabbitMQ's server to run automatically.

* g++ >= 4.7.1

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

Usage
-----

* ```cd``` to the installation directory 
* type ```python laptop.py```

Advanced Configuration
----------------------

We can run arbitrary tasks and self-tests.

laptop.py — the central task manager

* Defines ```class Task```, wrapping a function. Also declares the ```laptop.roll``` dict, which contains all task instances to be run.

  Tasks currently include the RabbitMQ server "```rmq``` ", the packet consumer "```rmq_consumer```", the custom-packet transmitter "```rmq_producer```", and the web API "```json_server```".

  To add a task, instantiate ```Task``` with a function handle, and add it to ```roll```.

consumer.py — the inbox

* Incoming packets are passed to a ```Table``` in ```db.tables``` for processing. Also defines functions to handle various Packet Data Types (e.g. 2 floats, 2 int32's, 1 int64, etc).

db.py — the database controller

* Defines ```class Table```, wrapping a SQL table. To add a custom packet table, add an entry to ```db._names```, ```db._sql```, and ```db._handlers```, and add an instance of ```class Table``` to ```db.tables```. Current tables heavily rely on the Packet Data Type functions in ``consumer.py``.

test.py — powerful unit tests

* EVERYTHING is tested, obviating the need for factorial hardware testing. Tests include:
  
  * all communication: car-side packet sending, server-side packet reception, server-side sending, and car-side reception.
  
  * ```laptop.py```'s multiprocess managing
  
  * all analytics math, including: GeoLocation-->Luminosity; Time-of-Day/Year-->Luminosity; Luminosity-->Received Power; Car-Temperature-->Received Power; and Applied Power-->Velocity.
  
  * WebServer reachability
  
car.py — remote data collection

* The Magic Numbers in ```car.py``` are currently configured for py3k. To run ```car.py```/unit tests in py2x, substitute them for the commented values
