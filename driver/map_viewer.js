// Generated by CoffeeScript 1.6.3
(function() {
  doctype(5);

  html(function() {
    head(function() {
      script({
        src: 'jquery-2.0.3.js'
      });
      script({
        src: 'https://maps.googleapis.com/maps/api/js?v=3.exp&sensor=false'
      });
      return script({
        src: 'map.js',
        type: 'text/javascript'
      });
    });
    return body(function() {
      return div('#map_canvas', function() {});
    });
  });

}).call(this);
