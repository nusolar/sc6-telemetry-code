telem
=====

NUSolar SC6 telemetry code

Prerequisites
-------------
Fedora >= 17                        (armel confirmed)
Debian >= Wheezy (testing)          (no armel currently for g++ >= 4.7.1)
Ubuntu >= 12.10 "Quantal Quetzal"   (armel unknown)
Arch Linux                          (armel confirmed)

* g++ >= 4.7.1

  NOTE: Most distributions do not provide a new enough g++.
  If this is the case for you, then you will need to install and use clang/LLVM (below) to compile the telemetry code.
  However, you should still follow these directions to install build-essential as it will be needed to compile the dependencies.
 
 
  In Debian/Ubuntu/Mint, can be installed with

  ```
  sudo apt-get install build-essential
  ```

  In Fedora, use
  ```
  sudo yum install make automake gcc gcc-c++
  ```

  Once installed, the g++ version can be checked with

  ```
  g++ -v
  ```

* (optional) clang/LLVM
 
  In Debian/Ubuntu/Mint, can be installed with

  ```
  sudo apt-get install clang
  ```

Installation
------------

Usage
-----

