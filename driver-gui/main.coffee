# namespace function from the coffeescript faq
namespace = (target, name, block) ->
  [target, name, block] = [(if typeof exports isnt 'undefined' then exports else window), arguments...] if arguments.length < 3
  top    = target
  target = target[item] or= {} for item in name.split '.'
  block target, top

window.MainTable = ($scope, $timeout) ->
	$scope.Signals =
		Off: 0,
		Left: 1,
		Right: 2,
		Hazards: 3

	# initialize HW members. TODO: load from car
	$scope.left_btn = false
	$scope.right_btn = false
	$scope.hazards_btn = false
	$scope.headlights_btn = false
	$scope.horn_btn = false
	$scope.signals_btn = $scope.Signals.Off

	$scope.motor_btn = false

	$scope.commands =
		drive: 0
		horn: 0
		signals: 0
		headlights: 0

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

	# Button callbacks - Hardware control
	$scope.Left = ->
		# Turn off other buttons, toggle Left Button
		if $scope.signals_btn==$scope.Signals.Left
			$scope.signals_btn = $scope.Signals.Off
		else
			$scope.signals_btn = $scope.Signals.Left
		# if Left Button is depressed, specify Left signal (==1, from C# code)
		$scope.commands.signals = $scope.signals_btn

	$scope.Right = ->
		$scope.hazards_btn = false
		$scope.left_btn = false
		$scope.right_btn = not $scope.right_btn
		if $scope.signals_btn==$scope.Signals.Right
			$scope.signals_btn = $scope.Signals.Off
		else
			$scope.signals_btn = $scope.Signals.Right
		# if Right Button is depressed, specify Right signal (==2, from C# code)
		$scope.commands.signals = $scope.signals_btn

	$scope.Hazards = ->
		if $scope.signals_btn==$scope.Signals.Hazards
			$scope.signals_btn = $scope.Signals.Off
		else
			$scope.signals_btn = $scope.Signals.Hazards
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

	# Button callbacks - Apps. TODO: Inline these OnClick() delegates into the Angular HTML
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
		# if Motor Button is depressed, activate Drive
		$scope.commands.drive = if $scope.motor_btn then 1 else 0


	# Create timer to submit commands, and receive updated Telemetry values.
	# Because AngularJS doesn't have $interval yet, we use the $apply hack
	timer_id = setInterval((=>
		$scope.$apply(=>
			$scope.query_string = $.param($scope.commands)
			$.ajax
				# url: window.location.origin + ':8080/data.json'
				url: 'http://localhost:8080/data.json'
				dataType:'jsonp'
				data: $scope.commands
				success: (json) =>
					# TODO update values
					$scope.commands
		)
	), 100) #ms

