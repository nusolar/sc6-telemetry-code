# namespace function from the coffeescript faq
namespace = (target, name, block) ->
  [target, name, block] = [(if typeof exports isnt 'undefined' then exports else window), arguments...] if arguments.length < 3
  top    = target
  target = target[item] or= {} for item in name.split '.'
  block target, top

namespace 'Model', (exports) ->
	exports.serial_devices =
		horn: "/dev/ttyACM.horn"
		left: "/dev/ttyACM.left"
		right: "/dev/ttyACM.right"
		headlights: "/dev/ttyACM.headlights"
		brakelights: "/dev/ttyACM.brakelights"

	class exports.UsbSerial
		_value: 0
		value: () ->
			_value
		ab2str: (buff) ->
			return String.fromCharCode.apply null, new UInt8Array(buff)
		str2ab: (str) ->
			buf = new ArrayBuffer(str.length)
			bufView = new UInt8Array(buf)
			for i in [0..str.length-1]
				bufView[i] = str.charCodeAt(i)
		write: (str, call = ->) ->
			chrome.serial.write @connection_id, str2ab(str), call
		off: () ->
			@write "off", ->
				_value = 0
		on: () ->
			@write "on", ->
				_value = 1
		toggle: () ->
			_value = !_value
		constructor: (@name) ->
			chrome.serial.open @name,
				bitrate: 115200,
				(connection_info) ->
					@connection_id = connection_info.connectionId
					console.log "Serial port opened: "+@name
		# MUST BE CALLED
		destroy: () ->
			chrome.serial.close @connection_id, ->
				console.log "Serial port closed: "+@name

	exports.usb = new Object()
	for k, v of exports.serial_devices
		exports.usb[k] = new exports.UsbSerial(v)

class Button extends $$
	class @Controller
		constructor: (@click) ->
		'mousedown &': ->
			@view.$().css('background-color', 'blue')
			@controller.click(true)
		'mouseup &': ->
			@view.$().css('background-color', '')
			@controller.click(false)
		'mouseout &': ->
			@controller['mouseup &']()
	constructor: (title, click = -> true) ->
		model = text: title
		view = $('#button_template').html()
		return $$ model, view, new Button.Controller(click)

class LeftPanel extends $$
	class @Controller
		'create': ->
			@append left = new Button("LEFT")
			@append haz  = new Button("HAZARDS")
			@append rev  = new Button("REVERSE")
	constructor: ->
		view = $('#left_template').html()
		return $$ {}, view, new LeftPanel.Controller()


class RightPanel extends $$
	class @Controller
		'create': ->
			@append right = new Button("RIGHT")
			@append heads = new Button("HEADLIGHTS")
			@append horn  = new Button("HORN")
	constructor: ->
		view = $('#right_template').html()
		return $$ {}, view, new RightPanel.Controller()


class BatteryBox extends $$
	class @Controller
		'create': ->
	constructor: ->
		view = $('#battery_box_template').html()
		return $$ {}, view, new BatteryBox.Controller()

class Map extends $$
	class @Controller
		'create': ->
	constructor: ->
		view = $('#map_template').html()
		return $$ {}, view, new Map.Controller()


class Drive extends $$
	class @Controller
		'create': ->
			@append start = new Button("START")
			start.view.$().css 'max-width': '20%'
	constructor: ->
		view = $('#drive_template').html()
		return $$ {}, view, new Drive.Controller()


class Camera extends $$
	class @Controller
		'create': ->
	constructor: ->
		view = $('#camera_template').html()
		return $$ {}, view, new Camera.Controller()

class CenterPanel extends $$
	class @Controller
		'create': ->
	constructor: ->
		view = $('#central_template').html()
		return $$ {}, view, new CenterPanel.Controller()

class CentralRow extends $$
	class @Controller
		panels: {}
		_current_panel: null
		show: (state, panel) ->
			if state
				@controller._current_panel.view.$().css('display', 'none')
				@controller._current_panel = @controller.panels[panel]
				# 0 ms delay necessary due to CSS bugs:
				window.setTimeout (=> @controller._current_panel.view.$().css('display', 'block')
				), 0
		'create': ->
			center = new CenterPanel()
			center.append @controller.panels['map'] = new Map()
			center.append @controller.panels['sensors'] = new BatteryBox()
			center.append @controller.panels['drive'] = new Drive()
			center.append @controller.panels['camera'] = new Camera()

			@controller._current_panel = @controller.panels['sensors']

			left = new LeftPanel()
			left.append new Button("MAP", (state) => @controller.show(state, 'map'))
			left.append new Button("SENSORS", (state) => @controller.show(state, 'sensors'))

			right = new RightPanel()
			right.append new Button("DRIVE", (state) => @controller.show(state, 'drive'))
			right.append new Button("CAMERA", (state) => @controller.show(state, 'camera'))

			@append left
			@append center
			@append right
	constructor: ->
		view = $('#central_row_template').html()
		return $$ {}, view, new CentralRow.Controller()

$ ->
	window.mouse_down = false
	document.body.onmouseup   = (e) -> window.mouse_down = false if e.button == 0
	document.body.onmousedown = (e) -> window.mouse_down = true  if e.button == 0

	body = new CentralRow()
	$$.document.append body, '.CentralRowWrapper'






