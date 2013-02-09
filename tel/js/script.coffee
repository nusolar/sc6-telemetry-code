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
	$(window).resize resize
	$('.tabs').tabs
		activate:	(event, ui) ->
			window.location.hash = ui.newPanel.attr('id')
			window.scrollTo(0, 0)
			whoami()
			resize()
	populate()
	$('footer').prepend "Via CoffeeScript 1.4, jQuery 1.9, jQuery UI 1.10, Flot " + $.plot.version + " &ndash; "

populate = ->
	populateCar()
	populatePlots()
	populateSend()
	resize()
populateCar = ->
	$('#driverSvg').load 'css/driver.svg', null
	$('#bboxSvg').load 'css/bbox.svg', drawCar
populatePlots = ->
	groups = ["Velocity", "Example"]
	$('#dataset')[0].options.add(new Option(g)) for g in groups
	$('#dataset')[0].onchange = drawPlots
populateSend = ->
	groups = [['bms_rx_trip', 0x200], ['mc_rx_drive_cmd', 0x501], ['mc_rx_power_cmd', 0x502], ['mc_rx_reset_cmd',0x503]]
	$('#pktName')[0].options.add(new Option(g[0],g[1])) for g in groups
	$('#pktName')[0].onchange = drawSend

resize = ->
	centerTabs()
	[drawCar, drawPlots, drawStrategy , (->), drawSend][page]()
centerTabs = ->
	return true unless $('.ui-corner-top').size()
	margin = $('.ui-corner-top').map( -> $(this).width() ).get().reduce (l,r) -> l+r 
	$('#t1').css('margin-left', ($('#tabBar').width() - margin)/2)
drawCar = ->
	return true unless $('#driverSvg').children().size() and $('#bboxSvg').children().size()
	maxH = $("#nonfooter").height()-$("header").height()-10 -$("#tabBar").height()-1 -(2 + 0.2)*em
	ns = "http://www.w3.org/2000/svg"
	$('#telemetry').find('text').remove()
	#Draw Velocities
	svg = $("#svg_vel")
	vel = document.createElementNS(ns, 'text')
	vel.textContent = "v = 30 m/s"
	vel.setAttribute('font-size',14)
	b = svg[0].getBBox()
	vel.setAttribute('x',b.x + em)
	vel.setAttribute('y',b.y + b.height/2 + 5 )
	svg.parent().append(vel)
	#Draw Efficiencies
	svg = $("#svg_eff")
	vel = document.createElementNS(ns, 'text')
	vel.textContent = "eff = 30 m/C"
	vel.setAttribute('font-size',14)
	b = svg[0].getBBox()
	vel.setAttribute('x',b.x + em)
	vel.setAttribute('y',b.y + b.height/2 + 5 )
	svg.parent().append(vel)
	#Draw Battery data
	for x in [1..32]
		svg = $("#bat"+x)
		vel = document.createElementNS(ns, 'text')
		vel.textContent = ".421V, 40ºC"
		vel.setAttribute('font-size',14)
		b = svg[0].getBBox()
		vel.setAttribute('x',b.x + em/3)
		vel.setAttribute('y',b.y + b.height/2 + 5 )
		svg.parent().append(vel)
drawPlots = ->
	maxH = $('#nonfooter').height()-$("header").height()-10 -$('#tabBar').height()-1 -(2 + 0.2 + 8)*em
	height = $('#plotHolder').width() / 1.618
	$('#plotHolder').height(if height < maxH then height else maxH)
	dataName = $("#dataset")[0].options[$("#dataset")[0].selectedIndex].value.toLowerCase()
	data = []
	switch dataName
		when "example"
			data.push( {data:([x, Math.sin(x)] for x in [0..13] by 0.5), lines:{show:true, fill:true}} )
			data.push( [[0, 3], [4, 8], [8, 5], [9, 13]] )
			data.push( [[0, 12], [7, 12], null, [7, 2.5], [12, 2.5]] )
		when "velocity"
			data.push( ([x, Math.atan(x)] for x in [0..13] by 0.1) )
	$.plot('#plotHolder', data)
drawStrategy = ->
	maxH = $('#nonfooter').height()-$("header").height()-10 -$('#tabBar').height()-1 -(2 + 0.2 + 8)*em
	height = $('#strategyHolder').width() / 1.618
	$('#strategyHolder').height(if height < maxH then height else maxH)
	$.plot('#strategyHolder', [[0,1],[1,1.2]])
drawSend = ->
	$('#pktAddr').val($('#pktName').val())























