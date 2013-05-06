telem
=====

NUSolar SC6 telemetry code

Getting Prerequisites
-------------
### POSIX operating system
* OS X or Linux. Completely untested on Windows, multiprocessing will probably crash.

### Python ≥ 3.3

In OS X, can be installed with
  ```bash
  brew install python3
  ```

Currently most Linux distributions don't package Python 3.3. Check your unstable/experimental repository.

##### Note: car-side only requires Python ≥ 3.0

To install python ≥ 3.0, in Debian/Ubuntu/Mint,
  ```
  sudo apt-get install python3
  ```

* #### pika ≥ 0.9.8
  Use our custom Python 3.3 branch, at <a href="https://github.com/alexchandel/pika">alexchandel/pika</a>. To install, download or clone this repository, `cd` into it, and run

  ```bash
  python3 setup.py install
  ```

* #### pyserial ≥ 2.6

  Only used by the car code, to talk to the CANUSB. (However it's also used by self-testing, see Usage below)

  In OS X, install with `pip3 install pyserial`

  In Debian, install with `sudo apt-get install python3-serial`

* #### numpy ≥ 1.7, scipy ≥ 0.11, matplotlib ≥ 1.2

  Used only on the laptop, for analytics.

  In OS X, install with `pip3 install numpy scipy matplotlib`

### RabbitMQ ≥ 3.0.2

* Do not configure RabbitMQ's server to run automatically. Record server executable's location in ```config.py```, default is ```/usr/local/sbin/```

  In OS X, can be installed with `brew install rabbitmq`

  In Debian/Ubuntu/Mint, can be installed with `sudo apt-get install rabbitmq-server`

Installation
------------
Install the prerequisites.

Clone this project to desired installation directory.

Edit ```config.py```, recording your laptop-server & car-client hostnames, the CANUSB device location for your `sys.platform`, and the location of the `rabbitmq-server` executable.

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
