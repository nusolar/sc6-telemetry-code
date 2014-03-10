# namespace function from the coffeescript faq
namespace = (target, name, block) ->
  [target, name, block] = [(if typeof exports isnt 'undefined' then exports else window), arguments...] if arguments.length < 3
  top    = target
  target = target[item] or= {} for item in name.split '.'
  block target, top

window.MainTable = ($scope, $timeout, $interval) ->
	# enum for TurnSignals state
	$scope.TurnSignals =
		Off: 0,
		Left: 1,
		Right: 2,
		Hazards: 3

	# initialize button states. TODO: load from car
	$scope.signals_btn = $scope.TurnSignals.Off
	$scope.headlights_btn = false
	$scope.horn_btn = false
	$scope.motor_btn = false
	$scope.reverse_btn = false

	# This object is the state of the User Input. It is serialized and sent to
	# the driver-server by $interval, below.
	# KEEP THIS OBJECT IN SYNC with its complementary interface in
	# /driver-server/SolarCar/HttpServer.cs
	$scope.commands =
		signals: 0
		headlights: 0
		horn: 0
		drive: 0
		reverse: 0

	# initialize App buttons' states.
	$scope.set_panel_button = (panel) ->
		# Display 'panel', hide all other panels
		$scope.map_btn = false
		$scope.sensors_btn = false
		$scope.drive_btn = false
		$scope.camera_btn = false
		if panel
			# 1 ms delay necessary due to CSS bugs:
			$timeout (=>
				$scope.current_panel = panel
				$scope[$scope.current_panel] = true
			), 1 #ms
	$scope.set_panel_button('sensors_btn')

	# Peripheral Button callbacks - Hardware
	$scope.Left = ->
		# Turn off other buttons, toggle Left Button
		if $scope.signals_btn==$scope.TurnSignals.Left
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Left
		# if Left Button is depressed, specify Left signal (==1, from C# code)
		$scope.commands.signals = $scope.signals_btn

	$scope.Right = ->
		if $scope.signals_btn==$scope.TurnSignals.Right
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Right
		# if Right Button is depressed, specify Right signal (==2, from C# code)
		$scope.commands.signals = $scope.signals_btn

	$scope.Hazards = ->
		if $scope.signals_btn==$scope.TurnSignals.Hazards
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Hazards
		# if Hazards Button is depressed, specify Hazards (==3, from C# code)
		$scope.commands.signals = $scope.signals_btn

	$scope.Headlights = ->
		$scope.headlights_btn = not $scope.headlights_btn
		# if Headlights Button is depressed, activate Headlights
		$scope.commands.headlights = if $scope.headlights_btn then 1 else 0

	$scope.SetHorn = (horn_bool) ->
		$scope.horn_btn = horn_bool
		# if Horn Button is depressed, activate horn
		$scope.commands.horn = if horn_bool then 1 else 0

	# Peripheral Button callbacks - Apps
	$scope.Map = ->
		$scope.set_panel_button('map_btn')
	$scope.Sensors = ->
		$scope.set_panel_button('sensors_btn')
	$scope.Drive = ->
		$scope.set_panel_button('drive_btn')
	$scope.Camera = ->
		$scope.set_panel_button('camera_btn')

	# Motor Tab - Button callbacks
	$scope.Motor = ->
		$scope.motor_btn = not $scope.motor_btn
		# if Motor Button is depressed, activate Drive
		$scope.commands.drive = if $scope.motor_btn then 1 else 0
		# always deactivate Reverse, if on
		if $scope.reverse_btn
			$scope.Reverse()

	$scope.Reverse = ->
		if $scope.commands.drive
			$scope.reverse_btn = not $scope.reverse_btn
			$scope.commands.reverse = if $scope.reverse_btn then 1 else 0
		else
			$scope.reverse_btn = false
			$scope.commands.reverse = 0

	# Create timer to submit commands, and receive updated Telemetry values.
	timer_id = $interval((=>
		$scope.query_string = $.param($scope.commands)
		$.ajax
			url: window.location.origin + '/data.json'
			# url: 'http://localhost:8080/data.json'
			dataType: 'text'
			data: $scope.commands
			success: (json_text) =>
				try
					json = JSON.parse(json_text)
					# TODO update values for $scope.commands
				catch e
					window.console.log(e)
	), 1000) #ms
	# Stop timer if the scope is destroyed (this should never happen).
	$scope.$on '$destroy', ->
		$interval.cancel timer_id

