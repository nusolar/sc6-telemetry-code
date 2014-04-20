telem
=====

NUSolar ZELDA - telemetry code

The telemetry consists of three programs: one that transmits data from the car, one that receives this data remotely, and one for analysis. The first is run onboard the car, the second and third should be run remotely on a laptop. The analysis software is based on models described in ```Solar Power Estimation.docx``` which is located in the NUSolar Dropbox `/Resources` folder.

Note: The majority of the telemetry is no longer in Python. Python is better suited for prototyping, while C# is appropriate for maturing projects.

Getting Prerequisites
-------------
Both the Car and Laptop:
* Mono .NET framework, or Microsoft .NET framework
* Web browser

Laptop side only:
* Python, NumPy, SciPy, and Matplotlib

Installation
------------
Clone this repository into the desired installation directory on the BeagleBoard.

The ```/driver-server``` folder currently contains the data gathering code. The ```SolarCar``` project runs on the car's onboard Linux board, the BeagleBoard-xM. The ```BaseStation``` project runs on the laptop, and receives telemetry over the internet from ```SolarCar```.

The ```/python``` folder houses data analysis scripts.

Tools for bootstrapping the BeagleBoard, installing the telemetry, debugging the CAN-USB cables, can be found in the ```/tools``` folder.

Usage
-----

Edit ```Config.cs``` and change the laptop hostname, the car hostnames, and the CAN-USB port location/name for your platform. Compile the projects.

* to start telemetry: run ```SolarCar.exe``` on the BeagleBoard
* to receive telemetry: run ```BaseStation.exe``` on your laptop
* to analyze data: use the ```python/analysis.py``` module
* to manually read the CAN-BUS: connect the CAN-USB cable and run `tools/canusb.sh`
* to talk to the BeagleBoard over Serial-USB: connect the Serial-USB cable and run `tools/serialusb.sh`

Configuration
----------------------

old information.

analysis.py — data analysis for race strategy

db.py — database controller

* Defines ```class Table```, wrapping a SQL table. Also defines the ```db.tables``` tuple, which holds active ```Table``` instances.

  To add a custom packet table, add an entry to ```db._names```, ```db._sql```, and ```db._handlers```, and append a ```Table``` instance to ```db.tables```'s definition. Current tables heavily rely on the Packet Data Type functions in ``consumer.py``.

config.py — configuration options

* Defines CANUSB location, (laptop-side) RabbitMQ details, car & laptop hostnames, Message Queue names, DB file location, and various time-delays.
