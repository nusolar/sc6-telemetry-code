// Generated by CoffeeScript 1.4.0
(function() {
  var centerTabs, drawCar, drawPlots, drawSend, drawStrategy, grab, grabSet, populate, populateCar, populatePlots, populateSend, redraw, whoami;

  this.em = 16;

  this.toc = ["telemetry", "plots", "strategy", "alerts", "send"];

  whoami = function() {
    this.hash = window.location.hash.slice(1).toLowerCase();
    this.anchor = hash.slice(0, +hash.indexOf('?') + 1 || 9e9);
    this.arg = hash[hash.indexOf('?')];
    this.page = toc.indexOf(anchor);
    if (this.page < 0) {
      this.page = 0;
      return this.hash = toc[page];
    }
  };

  $(function() {
    var em;
    em = Number($('body').css('font-size').slice(0, +($('body').css('font-size').indexOf('p') - 1) + 1 || 9e9));
    whoami();
    $(window).resize(redraw);
    $('.tabs').tabs({
      activate: function(event, ui) {
        window.location.hash = ui.newPanel.attr('id');
        window.scrollTo(0, 0);
        whoami();
        return redraw();
      }
    });
    window.pops = {};
    window.pops.send = [['bms_rx_trip', 0x200], ['mc_rx_drive_cmd', 0x501], ['mc_rx_power_cmd', 0x502], ['mc_rx_reset_cmd', 0x503]];
    window.pops.plots = ["Velocity", "Example"];
    grabSet('populate', grab);
    populate();
    $('footer').prepend("Via CoffeeScript 1.4, jQuery 1.9, jQuery UI 1.10, Flot " + $.plot.version + " &ndash; ");
    return window.setInterval(redraw, 1000);
  });

  grabSet = function(key, dest) {
    return $.ajax({
      url: window.location.origin + ':8080/' + key,
      jsonpCallback: 'callback',
      dataType: 'jsonp',
      success: dest
    });
  };

  grab = function(json) {
    window.pops.send = json.send;
    return populate();
  };

  populate = function() {
    populateCar();
    populatePlots();
    populateSend();
    return redraw();
  };

  populateCar = function() {
    $('#driverSvg').load('css/driver.svg', null);
    return $('#bboxSvg').load('css/bbox.svg', drawCar);
  };

  populatePlots = function() {
    var g, i, _i, _len, _ref;
    _ref = window.pops.plots;
    for (i = _i = 0, _len = _ref.length; _i < _len; i = ++_i) {
      g = _ref[i];
      $('#dataset')[0].options[i] = new Option(g);
    }
    return $('#dataset')[0].onchange = drawPlots;
  };

  populateSend = function() {
    var g, i, _i, _len, _ref;
    _ref = window.pops.send;
    for (i = _i = 0, _len = _ref.length; _i < _len; i = ++_i) {
      g = _ref[i];
      $('#pktName')[0].options[i] = new Option(g[0], g[1]);
    }
    return $('#pktName')[0].onchange = drawSend;
  };

  redraw = function() {
    centerTabs();
    return [drawCar, drawPlots, drawStrategy, (function() {}), drawSend][page]();
  };

  centerTabs = function() {
    var margin;
    if (!$('.ui-corner-top').size()) {
      return true;
    }
    margin = $('.ui-corner-top').map(function() {
      return $(this).width();
    }).get().reduce(function(l, r) {
      return l + r;
    });
    return $('#t1').css('margin-left', ($('#tabBar').width() - margin) / 2);
  };

  drawCar = function() {
    var b, maxH, ns, svg, vel, x, _i, _results;
    if (!($('#driverSvg').children().size() && $('#bboxSvg').children().size())) {
      return true;
    }
    maxH = $("#nonfooter").height() - $("header").height() - 10 - $("#tabBar").height() - 1 - (2 + 0.2) * em;
    ns = "http://www.w3.org/2000/svg";
    $('#telemetry').find('text').remove();
    svg = $("#svg_vel");
    vel = document.createElementNS(ns, 'text');
    vel.textContent = "v = 30 m/s";
    vel.setAttribute('font-size', 14);
    b = svg[0].getBBox();
    vel.setAttribute('x', b.x + em);
    vel.setAttribute('y', b.y + b.height / 2 + 5);
    svg.parent().append(vel);
    svg = $("#svg_eff");
    vel = document.createElementNS(ns, 'text');
    vel.textContent = "eff = 30 m/C";
    vel.setAttribute('font-size', 14);
    b = svg[0].getBBox();
    vel.setAttribute('x', b.x + em);
    vel.setAttribute('y', b.y + b.height / 2 + 5);
    svg.parent().append(vel);
    _results = [];
    for (x = _i = 1; _i <= 32; x = ++_i) {
      svg = $("#bat" + x);
      vel = document.createElementNS(ns, 'text');
      vel.textContent = ".421V, 40ºC";
      vel.setAttribute('font-size', 14);
      b = svg[0].getBBox();
      vel.setAttribute('x', b.x + em / 3);
      vel.setAttribute('y', b.y + b.height / 2 + 5);
      _results.push(svg.parent().append(vel));
    }
    return _results;
  };

  drawPlots = function() {
    var data, dataName, height, maxH, x;
    maxH = $('#nonfooter').height() - $("header").height() - 10 - $('#tabBar').height() - 1 - (2 + 0.2 + 8) * em;
    height = $('#plotHolder').width() / 1.618;
    $('#plotHolder').height(height < maxH ? height : maxH);
    dataName = $("#dataset")[0].options[$("#dataset")[0].selectedIndex].value.toLowerCase();
    data = [];
    switch (dataName) {
      case "example":
        data.push({
          data: (function() {
            var _i, _results;
            _results = [];
            for (x = _i = 0; _i <= 13; x = _i += 0.5) {
              _results.push([x, Math.sin(x)]);
            }
            return _results;
          })(),
          lines: {
            show: true,
            fill: true
          }
        });
        data.push([[0, 3], [4, 8], [8, 5], [9, 13]]);
        data.push([[0, 12], [7, 12], null, [7, 2.5], [12, 2.5]]);
        break;
      case "velocity":
        data.push((function() {
          var _i, _results;
          _results = [];
          for (x = _i = 0; _i <= 13; x = _i += 0.1) {
            _results.push([x, Math.atan(x)]);
          }
          return _results;
        })());
    }
    return $.plot('#plotHolder', data);
  };

  drawStrategy = function() {
    var height, maxH;
    maxH = $('#nonfooter').height() - $("header").height() - 10 - $('#tabBar').height() - 1 - (2 + 0.2 + 8) * em;
    height = $('#strategyHolder').width() / 1.618;
    $('#strategyHolder').height(height < maxH ? height : maxH);
    return $.plot('#strategyHolder', [[0, 1], [1, 1.2]]);
  };

  drawSend = function() {
    return $('#pktAddr').val($('#pktName').val());
  };

}).call(this);
