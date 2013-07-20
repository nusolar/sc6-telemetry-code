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

###
NOTE: Sub-class methods of Classes that extend/become MVC objects are always
	executed in the scope of the MVC class (e.g. Button), NOT the subclasses
	(e.g. model, view, or controller)
###
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

class Gap
	constructor: ->
		@model = {}
		@view = format: $('#gap_template').html()

class MotorToggle
	mousedown: false
	controller:
		'change:state': ->
			@view.$().css 'background-color': if @model.get('state') then '#FFFF33' else ''
			@model.set text: if @model.get 'state' then @model.get 'on_text' else @model.get 'off_text'
		'mousedown &': ->
			@mousedown = true
		'mouseup &': ->
			if @mousedown
				@mousedown = false
				@toggle @model.set 'state': not @model.get('state')
		'mouseout &': ->
			@mousedown = false
		'create': ->
			@view.$().addClass('MotorToggle')
			# This is done to trigger the Change Event
			@model.set 'state': @model.get 'state'
	constructor: (args = {}) ->
		@toggle = args.toggle ? ->
		@view = format: $('#button_template').html()
		@model =
			state: args.initial ? false
			on_text: args.on_text ? "ON"
			off_text: args.off_text ? (args.on ? "OFF")
			text: ""

class ToggleController
	'create': ->
		@mousedown = false


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
			@append @start = $$ new MotorToggle
				on_text: "MOTOR IS ON"
				off_text: "MOTOR IS OFF"
				state: false
			@start.view.$().css 'max-width': '20%'
		'child:change:state': (e) ->
			console.log @start.model.get('state')
	constructor: ->
		view = $('#drive_template').html()
		return $$ {}, view, new Drive.Controller()


class Camera extends $$
	class @Controller
		'create': ->
	constructor: ->
		view = $('#camera_template').html()
		return $$ {}, view, new Camera.Controller()


class MainTable extends $$
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
			@append @controller.panels['map'] = new Map(), '.CenterPanel'
			@append @controller.panels['sensors'] = new BatteryBox(), '.CenterPanel'
			@append @controller.panels['drive'] = new Drive(), '.CenterPanel'
			@append @controller.panels['camera'] = new Camera(), '.CenterPanel'

			@controller._current_panel = @controller.panels['sensors']

			@append @left = new Button("LEFT"), '.LeftPanel'
			@append @haz  = new Button("HAZARDS"), '.LeftPanel'
			@append @rev  = new Button("REVERSE"), '.LeftPanel'
			@append $$(new Gap()), '.LeftPanel'
			@append @map  = new Button("MAP", (s) => @controller.show(s, 'map')), '.LeftPanel'
			@append @sens = new Button("SENSORS", (s) => @controller.show(s, 'sensors')), '.LeftPanel'

			@append @right = new Button("RIGHT"), '.RightPanel'
			@append @heads = new Button("HEADLIGHTS"), '.RightPanel'
			@append @horn  = new Button("HORN"), '.RightPanel'
			@append $$(new Gap()), '.RightPanel'
			@append @drive = new Button("DRIVE" , (s) => @controller.show(s, 'drive')), '.RightPanel'
			@append @cam   = new Button("CAMERA", (s) => @controller.show(s, 'camera')), '.RightPanel'
	constructor: ->
		view = $('#main_table_template').html()
		return $$ {}, view, new MainTable.Controller()


$ ->
	window.mouse_down = false
	document.body.onmouseup   = (e) -> window.mouse_down = false if e.button == 0
	document.body.onmousedown = (e) -> window.mouse_down = true  if e.button == 0

	$$.document.append new MainTable()







