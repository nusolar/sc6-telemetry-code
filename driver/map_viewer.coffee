doctype 5
html ->
	head ->
		script src: 'jquery-2.0.3.js'
		script src: 'https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false'
		script src: 'map.js', type: 'text/javascript'
	body ->
		div '#map_canvas', ->
