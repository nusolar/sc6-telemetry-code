doctype 5
html ->
	head ->
		meta charset: 'utf-8'
		title "NU Solar Telemetry"
		link rel: 'stylesheet', href: 'css/style.css'
		link rel: 'stylesheet', href: 'css/bootstrap.min.css', media: 'screen'
		script src: 'js/jquery-1.9.0.js'
		script src: 'js/jquery.flot.js'
		script src: 'js/bootstrap.min.js'
		script src: 'js/script.js'
		# script data: {main: "js/script"}, src: 'js/require.js'

	body ->
		div '#nonfooter', ->
			header ->
				"NU Solar Telemetry"
			nav '#nav.navbar', ->
				div '.navbar-inner', ->
					ul '.nav.nav-tabs', ->
						li '.active', -> a data: {toggle: 'tab'}, href: '#telemetry', -> "Telemetry"
						li -> a data: {toggle: 'tab'}, href: '#plots', -> "Plots"
						li -> a data: {toggle: 'tab'}, href: '#strategy', -> "Strategy"
						li -> a data: {toggle: 'tab'}, href: '#alerts', -> "Alerts"
						li -> a data: {toggle: 'tab'}, href: '#send', -> "Send"

			div '.tab-content', ->
				article '#telemetry.tab-pane.active', ->
					div '#driver_svg.svgHolder', (->)
					div '#bbox_svg.svgHolder', (->)
				article '#plots.tab-pane', ->
					div '.graph', ->
						div '#plot_holder', (->)
					div '.hanger', ->
						form ->
							span '.datasetLabel', -> "Plots: "
							select '#dataset', (->)
							span '.datasetLabel', -> " from "
							select '#datatime', (->)
							span '.datasetLabel', -> " ago."
				article '#strategy.tab-pane', ->
					h2 -> "Inspirational Messages"
					div '.graph', ->
						div '#strategy_holder', (->)
				article '#alerts.tab-pane', ->
					h2 -> "(List of)"
					div '.alert', ->
						button type: 'button', class: 'close', data:{dismiss: 'alert'}, -> "&times;"
						strong "Warning!"
						"Check yo voltage"
				article '#send.tab-pane', ->
					h2 -> "Compose CAN packets hurr"
					form ->
						span -> "Name "
						select '#pkt_name', name: 'name'
						span -> "or Address "
						input '#pkt_addr', name: 'addr', type: 'text', size: 8
						br()
						"Data: "

		footer " © Alex Chandel 2013."
