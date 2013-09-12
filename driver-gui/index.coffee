# This is a COFFEE-CUP file. It compiles to HTML.
doctype 5
html lang: "en", 'ng-app': '', 'ng-csp': '', ->
	head ->
		meta charset: 'utf-8'
		title "NU Solar's ZELDA OS"
		link rel: 'stylesheet', href: 'main.css'
		script src: 'jquery-2.0.3.js'
		script src: 'angular.min.js'
		script src: 'main.js'

	body ->
		article '.MainTable', 'ng-controller': 'MainTable', ->
			header '.HeaderRow', ->
				div '.TextCell.Title.TopLeftCorner', ->
				div '.TextCell.Title', colspan: '2', ->
					"Title PLACEHOLDER"
				div '.TextCell.Title.TopRightCorner', ->

			div '.CentralRow', ->
				section '.LeftPanel', ->
					div '.Button', 'ng-class': "{ButtonOn: signals_btn==Signals.Left}", 'ng-click': 'Left()', ->
						"LEFT"
					div '.Button', 'ng-class': "{ButtonOn: signals_btn==Signals.Hazards}", 'ng-click': 'Hazards()', ->
						"HAZARDS"
					div '.Button', {'ng-class': "{ButtonOn: horn_btn}",
					'ng-mousedown': 'SetHorn(true)', 'ng-mouseup': 'SetHorn(false)'}, ->
						"HORN"
					div '.Gap', ->
					div '.Button', 'ng-class': "{ButtonOn: map_btn}", 'ng-click': 'Map()', ->
						"MAP"
					div '.Button', 'ng-class': "{ButtonOn: sensors_btn}", 'ng-click': 'Sensors()', ->
						"SENSORS"


				div '.CenterPanel.ng-cloak',  ->
					section '.MapPanel', 'ng-show': 'map_btn', ->
						img '.Map', src: 'map.png', ->

					section '.BatteryBoxPanel', 'ng-show': 'sensors_btn', ->
						table '.BatteryBoxTable', ->
							battery_cell = (num) ->
								td '#battery_'+String(num)+'.Battery', ->
									String(num)
							tr battery_cell(31)
							tr battery_cell(30)
							for i in [29..25] by -1
								tr ->
									for j in [i..0] by -10
										for k in [j, j - (2*(j%5) + 1)]
											battery_cell(k)
					section '.DrivePanel', 'ng-show': 'drive_btn', ->
						div '.Button.MotorToggle', 'ng-class': "{MotorToggleOn: motor_btn}", 'ng-click': 'Motor()', ->
							"MOTOR IS {{ (motor_btn? 'ON': 'OFF') }}"

					section '.CameraPanel', 'ng-show': 'camera_btn', ->
						"CAMERA"

				section '.RightPanel', ->
					div '.Button', 'ng-class': "{ButtonOn: signals_btn==Signals.Right}", 'ng-click': 'Right()', ->
						"RIGHT"
					div '.Button', 'ng-class': "{ButtonOn: headlights_btn}", 'ng-click': 'Headlights()', ->
						"HEADLIGHTS"
					div '.Button', 'ng-class': "{ButtonOn: reverse_btn}", 'ng-click': 'Reverse()', ->
						"REVERSE"
					div '.Gap', ->
					div '.Button', 'ng-class': "{ButtonOn: drive_btn}", 'ng-click': 'Drive()', ->
						"DRIVE"
					div '.Button', 'ng-class': "{ButtonOn: camera_btn}", 'ng-click': 'Camera()', ->
						"CAMERA"


			footer '.FooterRow', colspan: '2', ->
				div '.TextCell.Bottom.BottomLeftCorner', ->
				div '.TextCell.Bottom', colspan: '2', 'ng-bind': 'query_string', ->
				div '.TextCell.Bottom.BottomRightCorner', ->
