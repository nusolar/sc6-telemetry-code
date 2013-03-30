telem
=====

NUSolar SC6 telemetry code

TODO
----

Done: <del>We need to coalesce data packets into ∆t intervals.</del>
<del>
* ∆t = 1s for now. 5s packets are copied over.
- Solution
	A consumer will reshape each packet into a row element, and its timestamp is compared to the "working row". If ∆t<1s, the row is overlain by the element, and timestamps are met. If ∆t>1s, the row is committed, and the element forms a new row.
</del>

New carside-datapoints:

* GPS sensor?
* Elevation sensor?
* Solar intensity
	-> Power per module (L,T)
* Array Temperature

Other variables:

* Track length
	- maybe from gps, or manually
* Air temperature
* <s>Time of Day</s>
* Driver

<del>
Events:</del>
<del>
* tire blowout, battery swap, hotpit, crash
* driver change
</del>

End result:

* Necessary laptime (from Energy and Power usage)
* Predicted laptime (from velocity, elapsed time & remaining distance)

Other – Tests:

* water spray.
	- probably negligible. reflects light, evaporation cools cell.
	measure (x10) array power (watching luminosity) vs water spray

<del>* Test consumer.py with dummy packets</del>
