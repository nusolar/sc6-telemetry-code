app_module = angular.module('DriverApp', [])

app_module.controller 'MainTableController',
["$scope", "$timeout", "$interval", ($scope, $timeout, $interval) ->
	# enum for TurnSignals state
	$scope.TurnSignals =
		Off: 0,
		Left: 1,
		Right: 2,
		Hazards: 3

	# initialize button states.
	# View Handles:
	$scope.signals_btn = $scope.TurnSignals.Off
	$scope.headlights_btn = false
	$scope.horn_btn = false
	$scope.battery_btn = true
	$scope.motor_btn = false
	$scope.reverse_btn = false

	$scope.commands =
		turn_signals: 0
		headlights: 0
		horn: 0
		run_battery: 1
		drive: 0
		reverse: 0

	# This object is the state of the User Input. It is serialized and sent to
	# the driver-server by $interval, below.
	# KEEP THIS OBJECT IN SYNC with its complementary interface in
	# /driver-server/SolarCar/HttpServer.cs
	$scope.serialize_commands = ->
		commands =
			signals: ($scope.commands.turn_signals << 0 | $scope.commands.headlights << 2 | $scope.commands.horn << 3)
			gear:  ($scope.commands.run_battery << 0 | $scope.commands.drive << 1 | $scope.commands.reverse << 2)
		return commands


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

	# Peripheral Button callbacks - Hardware
	$scope.Left = ->
		# Turn off other buttons, toggle Left Button
		if $scope.signals_btn==$scope.TurnSignals.Left
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Left
		# if Left Button is depressed, specify Left signal (==1, from C# code)
		$scope.commands.turn_signals = $scope.signals_btn

	$scope.Right = ->
		if $scope.signals_btn==$scope.TurnSignals.Right
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Right
		# if Right Button is depressed, specify Right signal (==2, from C# code)
		$scope.commands.turn_signals = $scope.signals_btn

	$scope.Hazards = ->
		if $scope.signals_btn==$scope.TurnSignals.Hazards
			$scope.signals_btn = $scope.TurnSignals.Off
		else
			$scope.signals_btn = $scope.TurnSignals.Hazards
		# if Hazards Button is depressed, specify Hazards (==3, from C# code)
		$scope.commands.turn_signals = $scope.signals_btn

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

	# Driving Tab - Button callbacks
	$scope.Battery = ->
		$scope.battery_btn = not $scope.battery_btn
		# if Battery Button is depressed, activate Run
		$scope.commands.run_battery = if $scope.battery_btn then 1 else 0
		# always deactivate Motor and Reverse when Battery is toggled
		if $scope.motor_btn
			$scope.Motor() # this should also kill Reverse

	$scope.Motor = ->
		if $scope.commands.run_battery
			$scope.motor_btn = not $scope.motor_btn
			# if Motor Button is depressed, activate Drive
			$scope.commands.drive = if $scope.motor_btn then 1 else 0
			# always deactivate Reverse, if it's ON
			if $scope.reverse_btn
				$scope.Reverse()
		else
			$scope.motor_btn = false
			$scope.commands.drive = 0
			$scope.reverse_btn = false
			$scope.commands.reverse = 0

	$scope.Reverse = ->
		if $scope.commands.drive
			$scope.reverse_btn = not $scope.reverse_btn
			$scope.commands.reverse = if $scope.reverse_btn then 1 else 0
		else
			$scope.reverse_btn = false
			$scope.commands.reverse = 0


	# Create timer to submit commands, and receive updated Telemetry values.
	timer_id = $interval((=>
		$scope.print_query_string = $.param($scope.serialize_commands())
		$.ajax
			# url: 'http://localhost:8080/data.json'
			url: window.location.origin + '/data.json'
			dataType: 'text'
			data: $scope.serialize_commands()
			success: (json_text) =>
				try
					json = window.JSON.parse(json_text)
					$scope.title_string = "v: " + json["MotorVelocity"] + "m/s, " +
						"I: " + json["MotorCurrent"] + "A, " +
						"d: " + json["MotorOdometer"] + "m, " +
						"C: " + json["MotorAmpHours"] + "Ah"
					# TODO update values for $scope.commands
				catch e
					window.console.log(e)
	), 250) #ms
	# Stop timer if the scope is destroyed (this should never happen).
	$scope.$on '$destroy', ->
		$interval.cancel timer_id


	### INITIALIZE ###
	$scope.print_query_string = $.param($scope.serialize_commands())
	$scope.title_string = "v: 0m/s, I: 0A, d: 0m, C: 0Ah"
	$scope.set_panel_button('drive_btn')
]
