
# chrome.app.window.onClosed.addListener ->
	# for k, v of Model.usb
		# v.destroy()

chrome.app.runtime.onLaunched.addListener ->
	width = 800
	height = 600

	chrome.app.window.create 'index.html',
		bounds:
			width: width
			height: height
			left: Math.round((screen.availWidth - width)/2)
			top: Math.round((screen.availHeight - height)/2)
		transparentBackground: true
		, (created_window) ->

	# chrome.tts.speak "Welcome to the N U Solar, Zelda."

