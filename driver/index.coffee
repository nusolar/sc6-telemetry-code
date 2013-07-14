doctype 5
html ->
	head ->
		meta charset: 'utf-8'
		title "NU Solar's ZELDA OS"
		link rel: 'stylesheet', href: 'main.css'
		script src: 'jquery-2.0.3.js'
		script src: 'agility.min.js'
		script src: 'main.js'

	body '#body', ->
		header '.HeaderBar', ->
			"Title PLACEHOLDER"
		article '.CentralRowWrapper', ->
		footer '.FooterBar', ->
			"no content"

		div '#templates', ->
			script '#button_template', type: 'text/html', ->
				div '.Button', data: {bind: 'text'}, ->
					# div '.ButtonText', ->
			script '#left_template', type: 'text/html', ->
				section '.LeftPanel', ->

			script '#right_template', type: 'text/html', ->
				section '.RightPanel', ->

			script '#battery_box_template', type: 'text/html', ->
				section '.BatteryBoxPanel', ->
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
			script '#map_template', type: 'text/html', ->
				section '.MapPanel', ->
					"MAP"
			script '#drive_template', type: 'text/html', ->
				section '.DrivePanel', ->
			script '#camera_template', type: 'text/html', ->
				section '.CameraPanel', ->
					"CAMERA"
			script '#central_template', type: 'text/html', ->
				div '.CenterPanel', ->

			script '#central_row_template', type:'text/html', ->
				div '.CentralRow', ->
