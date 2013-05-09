# Copyright Alex Chandel 2013. All rights reserved.
# require.config
# 	paths:
# 		svg: 'jquery.svg'
# 		flot: 'jquery.flot'
# 		jquery: 'jquery-1.9.0'
# 		jquery_ui: 'jquery-ui-1.10.0.custom'
# 	shim:
# 		jquery_ui:
# 			deps: ['jquery']

this.em = 16
this.toc = ["telemetry","plots","strategy","alerts","send"]
whoami = ->
	this.hash = window.location.hash[1..].toLowerCase()
	this.anchor = hash[0..hash.indexOf('?')]
	this.arg = hash[hash.indexOf('?')]
	this.page = toc.indexOf(anchor)
	if  this.page < 0
		this.page = 0
		this.hash = toc[page]

$ ->
	em = Number($('body').css('font-size')[0..$('body').css('font-size').indexOf('p')-1])
	whoami()
	# populate() must run before redraw() is ever called
	window.data = {}
	window.data.send = [['bms_rx_trip', 0x200], ['mc_rx_drive_cmd', 0x501], ['mc_rx_power_cmd', 0x502], ['mc_rx_reset_cmd',0x503]]
	window.data.plots = ["Example", "Velocity"]
	window.data.ranges = ["1 day"]
	window.data.telemetry = { "bms":{"I":1, "CC":6, "Wh":7, "uptime":1}, "bms_V": [.421], "bms_T": [39], "bms_owV":[.421]
	"array":{"I":3, "CC":8}, "sw":{"buttons":0, "lights":0}, "ws": {"v":22, "I":2, "V":20, "T":50, "e":31}, "mppt":{"T":40, "I":1} }
	grabSet('populate', grabStatic)
	# $('#nav a:first').tab('show')
	populate()
	$(window).resize redraw
	$('a').on 'shown', (event) ->
		window.location.hash = event.target.hash
		window.scrollTo(0, 0)
		whoami()
		redraw()
	$('footer').prepend "Via CoffeeScript 1.6, jQuery 1.9, jQuery UI 1.10, Flot " + $.plot.version + " &ndash; "
	window.setInterval tic, 5000

grabSet = (key, dest) ->
	$.ajax
		url: window.location.origin + ':8080/' + key
		jsonpCallback:'callback'
		dataType:'jsonp'
		success: dest

grabStatic = (json) ->
	data.send = json.send
	populate()
populate = ->
	populateCar()
	populatePlots()
	populateSend()
	redraw()
populateCar = ->
	$('#driver_svg').load 'css/driver.svg', drawCar
	$('#bbox_svg').load 'css/bbox.svg', drawCar
populatePlots = ->
	$('#dataset')[0].options[i] = new Option(g) for g, i in window.data.plots
	$('#dataset')[0].onchange = drawPlots
	$('#datatime')[0].options[0] = new Option(window.data.ranges[0])
	$('#datatime')[0].onchange = drawPlots
populateSend = ->
	$('#pkt_name')[0].options[i] = new Option(g[0],g[1]) for g,i in window.data.send
	$('#pkt_name')[0].onchange = drawSend

#Redraw UI with centered, updated data
redraw = ->
	centerTabs()
	[drawCar, drawPlots, drawStrategy , (->), drawSend][@page]()
centerTabs = ->
	return true unless $('.ui-corner-top').size()
	margin = $('.ui-corner-top').map( -> $(this).width() ).get().reduce (l,r) -> l+r
	$('#t1').css('margin-left', ($('#nav').width() - margin)/2)
drawCar = ->
	return true unless $('#driver_svg').children().size() && $('#bbox_svg').children().size()
	maxH = $("#nonfooter").height()-$("header").height()-10 -$("#nav").height()-1 -(2 + 0.2)*em
	ns = "http://www.w3.org/2000/svg"
	$('#telemetry').find('text').remove()
	setText = (selector, string, xoff) ->
		svg = $(selector)
		vel = document.createElementNS(ns, 'text')
		vel.textContent = string
		vel.setAttribute('font-size',14)
		b = svg[0].getBBox()
		vel.setAttribute('x', b.x + em/xoff)
		vel.setAttribute('y', b.y + b.height/2 + 5)
		svg.parent().append(vel)
	#Draw Velocities, Efficiencies
	setText("#svg_vel", "v = "+data.telemetry.ws.v+" m/s", 1)
	setText("#svg_eff", "eff = "+data.telemetry.ws.e+" m/C", 1)
	#Draw Battery data
	for x in [1..32]
		setText("#bat"+x, ".421V, "+data.telemetry.bms_T[0]+"ºC", 3)
drawPlots = ->
	maxH = $('#nonfooter').height()-$("header").height()-10 -$('#nav').height()-1 -(2 + 0.2 + 8)*em
	height = $('#plot_holder').width() / 1.618
	$('#plot_holder').height(if height < maxH then height else maxH)
	dataName = $("#dataset")[0].options[$("#dataset")[0].selectedIndex].value.toLowerCase()
	data = []
	switch dataName
		when "example"
			data.push( {data:([x, Math.sin(x)] for x in [0..13] by 0.5), lines:{show:true, fill:true}} )
			data.push( [[0, 3], [4, 8], [8, 5], [9, 13]] )
			data.push( [[0, 12], [7, 12], null, [7, 2.5], [12, 2.5]] )
		when "velocity"
			data.push( ([x, Math.atan(x)] for x in [0..13] by 0.1) )
	$.plot('#plot_holder', data)
drawStrategy = ->
	maxH = $('#nonfooter').height()-$("header").height()-10 -$('#nav').height()-1 -(2 + 0.2 + 8)*em
	height = $('#strategy_holder').width() / 1.618
	$('#strategy_holder').height(if height < maxH then height else maxH)
	$.plot('#strategy_holder', [[0,1],[1,1.2]])
drawSend = ->
	$('#pkt_addr').val($('#pkt_name').val())

tic = ->
	grabSet("telemetry", grabDynamic)
	redraw()
grabDynamic = (json) ->
	data.telemetry = json.telemetry
	redraw()


























