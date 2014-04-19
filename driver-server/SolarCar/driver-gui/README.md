driver-gui
=====

The Driver GUI is a single-page, single-HTTP-call web app. The App's HTML is written in the Jade templating language, and the JS code is written in Coffeescript. The project also uses JQuery and AngularJS libraries.

#### Build Dependencies
* `Node.js`
* The following Node.js packages:
	* `coffee-script`
	* `jade`
	* `less`

#### The final product
All scripts and CSS are directly copied into the final `index.html` file, including the libraries, by Jade's '`include`" directives.

#### Compiling
To generate the final product, run the compilation script:
```
	COMPILE.sh
```

The file `index.html` is a self-contained application, and may be included into the Telemetry binary.
