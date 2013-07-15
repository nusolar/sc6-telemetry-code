doctype 5
html ->
	head ->
		meta charset: 'utf-8'
		title "NU Solar's ZELDA OS"
		link rel: 'stylesheet', href: 'main.css'
		script src: 'jquery-2.0.3.js'
		script src: 'agility.min.js'
		script src: 'main.js'

	body ->
		div '.Templates', ->
			script '#gap_template', type: 'text/html', ->
				div '.Gap', ->

			script '#button_template', type: 'text/html', ->
				div '.Button', data: {bind: 'text'}, ->

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
					img '.Map', src: 'map.png', ->
			script '#drive_template', type: 'text/html', ->
				section '.DrivePanel', ->
			script '#camera_template', type: 'text/html', ->
				section '.CameraPanel', ->
					"CAMERA"

			script '#main_table_template', type:'text/html', ->
				article '.MainTable', ->
					header '.HeaderRow', ->
						div '.TextCell.Title.TopLeftCorner', ->
						div '.TextCell.Title', colspan: '2', ->
							"Title PLACEHOLDER"
						div '.TextCell.Title.TopRightCorner', ->
					div '.CentralRow', ->
						section '.LeftPanel', ->
						div '.CenterPanel', ->
						section '.RightPanel', ->
					footer '.FooterRow', colspan: '2', ->
						div '.TextCell.Bottom.BottomLeftCorner', ->
						div '.TextCell.Bottom', colspan: '2', ->
							"no content"
						div '.TextCell.Bottom.BottomRightCorner', ->
