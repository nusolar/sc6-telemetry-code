telem
=====

NUSolar SC6 telemetry code
 
Prerequisites
-------------
A POSIX operating system

Python >= 2.7.2 

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
Install all dependencies first.

Clone this project to desired installation directory.

Usage
-----

* ```cd``` to the installation directory 
* type ```python laptop.py```

Advanced Configuration
----------------------

We can run arbitrary telemetry tasks.

laptop.py

* The central task manager. Defines ```class task```, which wraps a function. Also defines the ```roll``` dict, which contains all task instances to be run.

  Default tasks are the RabbitMQ server "```rmq``` ", the packet consumer "```rmq_consumer```", the artificial-packet transmitter "```rmq_producer```", and the web API "```json_server```".

  To add a task, instantiate ```task``` with a function handle, and add it to ```roll```.

consumer.py

* The incoming mailroom. Incoming packets are sorted into one of ```db.tables```.

db.py

* The database controller. To add a new packet table, add an instance of ```class table``` to ```db.tables```. For each custom table, add an entry to ```_names```, ```_sql```, and ```_handlers```.

