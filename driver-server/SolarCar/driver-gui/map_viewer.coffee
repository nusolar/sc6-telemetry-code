
google.maps.event.addDomListener window, 'load', ->
	@map = new google.maps.Map $('#map_canvas')[0],
		zoom: 8
		center: new google.maps.LatLng(-34.397, 150.644)
		mapTypeId: google.maps.MapTypeId.ROADMAP
