telem
=====

NUSolar SC6 telemetry code

TODO
----

Done: <s>We need to coalesce data packets into ∆t intervals.

* ∆t = 1s, for now.
* What about packets sent every 5s?
- Solution
	A consumer will reshape each packet into a row element, and its timestamp is compared to the "working row". If ∆t<1s, the row is overlain by the element, and timestamps are met. If ∆t>1s, the row is committed, and the element forms a new row.</s>

New carside-datapoints:

* GPS sensor?
* Elevation sensor?
* Solar intensity
	* Luminosity on array
		* solar angle
	-> Power per cell

Other variables:

* Track length
	- maybe from gps, or manual
* Air temperature
* <s>Time of Day</s>
* Driver

Events:

* tire blowout
* battery swap
* driver change
* hotpit
* crash

End result:

* Necessary laptime (from Energy and Power usage)
* Predicted laptime (from velocity, elapsed time & remaining distance)

Other – Tests:

* water spray. probably negligible. reflects light, evaporation cools cell.
	measure (x10) array power (watching luminosity) vs water spray

