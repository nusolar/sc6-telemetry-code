# namespace function from the coffeescript faq
namespace = (target, name, block) ->
  [target, name, block] = [(if typeof exports isnt 'undefined' then exports else window), arguments...] if arguments.length < 3
  top    = target
  target = target[item] or= {} for item in name.split '.'
  block target, top

# namespace 'Model', (exports) ->
# 	exports.serial_devices =
# 		horn: "/dev/ttyACM.horn"
# 		left: "/dev/ttyACM.left"
# 		right: "/dev/ttyACM.right"
# 		headlights: "/dev/ttyACM.headlights"
# 		brakelights: "/dev/ttyACM.brakelights"

# 	class exports.UsbSerial
# 		_value: 0
# 		value: () ->
# 			_value
# 		ab2str: (buff) ->
# 			return String.fromCharCode.apply null, new UInt8Array(buff)
# 		str2ab: (str) ->
# 			buf = new ArrayBuffer(str.length)
# 			bufView = new UInt8Array(buf)
# 			for i in [0..str.length-1]
# 				bufView[i] = str.charCodeAt(i)
# 		write: (str, call = ->) ->
# 			chrome.serial.write @connection_id, str2ab(str), call
# 		off: () ->
# 			@write "off", ->
# 				_value = 0
# 		on: () ->
# 			@write "on", ->
# 				_value = 1
# 		toggle: () ->
# 			_value = !_value
# 		constructor: (@name) ->
# 			chrome.serial.open @name,
# 				bitrate: 115200,
# 				(connection_info) ->
# 					@connection_id = connection_info.connectionId
# 					console.log "Serial port opened: "+@name
# 		# MUST BE CALLED
# 		destroy: () ->
# 			chrome.serial.close @connection_id, ->
# 				console.log "Serial port closed: "+@name

# 	exports.usb = new Object()
# 	for k, v of exports.serial_devices
# 		exports.usb[k] = new exports.UsbSerial(v)


window.MainTable = ($scope, $timeout) ->

	# initialize HW members. TODO: load from car
	$scope.left_btn = false
	$scope.right_btn = false
	$scope.hazards_btn = false
	$scope.headlights_btn = false
	$scope.horn_btn = false

	$scope.motor_btn = false
	$scope.motor_state = ["OFF", "ON"]

	# initialize App buttons' states.
	$scope.set_panel_button = (panel) ->
		# Display 'panel', hide all other panels
		$scope.map_btn = false
		$scope.sensors_btn = false
		$scope.drive_btn = false
		$scope.camera_btn = false
		if panel
			# 0 ms delay necessary due to CSS bugs:
			$timeout (=>
				$scope.current_panel = panel
				$scope[$scope.current_panel] = true
			), 0
	$scope.set_panel_button('sensors_btn')

	# Button callbacks - HW
	$scope.Left = ->
		$scope.hazards_btn = false
		$scope.right_btn = false
		$scope.left_btn = not $scope.left_btn
		# TODO set HW

	$scope.Right = ->
		$scope.hazards_btn = false
		$scope.left_btn = false
		$scope.right_btn = not $scope.right_btn
		# TODO set HW

	$scope.Hazards = ->
		$scope.left_btn = false
		$scope.right_btn = false
		$scope.hazards_btn = not $scope.hazards_btn
		# TODO set HW

	$scope.Headlights = ->
		$scope.headlights_btn = not $scope.headlights_btn
		# TODO set HW

	$scope.SetHorn = (horn_bool) ->
		$scope.horn_btn = horn_bool
		# TODO set HW

	# Button callbacks - Apps. TODO: Merge into HTML
	$scope.Map = ->
		$scope.set_panel_button('map_btn')
	$scope.Sensors = ->
		$scope.set_panel_button('sensors_btn')
	$scope.Drive = ->
		$scope.set_panel_button('drive_btn')
	$scope.Camera = ->
		$scope.set_panel_button('camera_btn')

	$scope.Motor = ->
		$scope.motor_btn = not $scope.motor_btn


