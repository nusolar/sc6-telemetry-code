// Generated by CoffeeScript 1.6.3
(function() {
  chrome.app.runtime.onLaunched.addListener(function() {
    var height, width;
    width = 800;
    height = 600;
    return chrome.app.window.create('index.html', {
      bounds: {
        width: width,
        height: height,
        left: Math.round((screen.availWidth - width) / 2),
        top: Math.round((screen.availHeight - height) / 2)
      },
      transparentBackground: true
    }, function(created_window) {});
  });

}).call(this);