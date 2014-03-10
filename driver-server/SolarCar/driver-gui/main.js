// Generated by CoffeeScript 1.7.1
(function() {
  var namespace,
    __slice = [].slice;

  namespace = function(target, name, block) {
    var item, top, _i, _len, _ref, _ref1;
    if (arguments.length < 3) {
      _ref = [(typeof exports !== 'undefined' ? exports : window)].concat(__slice.call(arguments)), target = _ref[0], name = _ref[1], block = _ref[2];
    }
    top = target;
    _ref1 = name.split('.');
    for (_i = 0, _len = _ref1.length; _i < _len; _i++) {
      item = _ref1[_i];
      target = target[item] || (target[item] = {});
    }
    return block(target, top);
  };

  window.MainTable = function($scope, $timeout, $interval) {
    var timer_id;
    $scope.TurnSignals = {
      Off: 0,
      Left: 1,
      Right: 2,
      Hazards: 3
    };
    $scope.signals_btn = $scope.TurnSignals.Off;
    $scope.headlights_btn = false;
    $scope.horn_btn = false;
    $scope.motor_btn = false;
    $scope.reverse_btn = false;
    $scope.commands = {
      turn_signals: 0,
      headlights: 0,
      horn: 0,
      drive: 0,
      reverse: 0
    };
    $scope.set_panel_button = function(panel) {
      $scope.map_btn = false;
      $scope.sensors_btn = false;
      $scope.drive_btn = false;
      $scope.camera_btn = false;
      if (panel) {
        return $timeout(((function(_this) {
          return function() {
            $scope.current_panel = panel;
            return $scope[$scope.current_panel] = true;
          };
        })(this)), 1);
      }
    };
    $scope.set_panel_button('sensors_btn');
    $scope.Left = function() {
      if ($scope.signals_btn === $scope.TurnSignals.Left) {
        $scope.signals_btn = $scope.TurnSignals.Off;
      } else {
        $scope.signals_btn = $scope.TurnSignals.Left;
      }
      return $scope.commands.turn_signals = $scope.signals_btn;
    };
    $scope.Right = function() {
      if ($scope.signals_btn === $scope.TurnSignals.Right) {
        $scope.signals_btn = $scope.TurnSignals.Off;
      } else {
        $scope.signals_btn = $scope.TurnSignals.Right;
      }
      return $scope.commands.turn_signals = $scope.signals_btn;
    };
    $scope.Hazards = function() {
      if ($scope.signals_btn === $scope.TurnSignals.Hazards) {
        $scope.signals_btn = $scope.TurnSignals.Off;
      } else {
        $scope.signals_btn = $scope.TurnSignals.Hazards;
      }
      return $scope.commands.turn_signals = $scope.signals_btn;
    };
    $scope.Headlights = function() {
      $scope.headlights_btn = !$scope.headlights_btn;
      return $scope.commands.headlights = $scope.headlights_btn ? 1 : 0;
    };
    $scope.SetHorn = function(horn_bool) {
      $scope.horn_btn = horn_bool;
      return $scope.commands.horn = horn_bool ? 1 : 0;
    };
    $scope.Map = function() {
      return $scope.set_panel_button('map_btn');
    };
    $scope.Sensors = function() {
      return $scope.set_panel_button('sensors_btn');
    };
    $scope.Drive = function() {
      return $scope.set_panel_button('drive_btn');
    };
    $scope.Camera = function() {
      return $scope.set_panel_button('camera_btn');
    };
    $scope.Motor = function() {
      $scope.motor_btn = !$scope.motor_btn;
      $scope.commands.drive = $scope.motor_btn ? 1 : 0;
      if ($scope.reverse_btn) {
        return $scope.Reverse();
      }
    };
    $scope.Reverse = function() {
      if ($scope.commands.drive) {
        $scope.reverse_btn = !$scope.reverse_btn;
        return $scope.commands.reverse = $scope.reverse_btn ? 1 : 0;
      } else {
        $scope.reverse_btn = false;
        return $scope.commands.reverse = 0;
      }
    };
    $scope.serialize_commands = function() {
      var commands;
      commands = {
        signals: $scope.commands.turn_signals | $scope.commands.headlights << 2 | $scope.commands.horn << 3,
        gear: $scope.commands.drive | $scope.commands.reverse << 1
      };
      return commands;
    };
    timer_id = $interval(((function(_this) {
      return function() {
        $scope.query_string = $.param($scope.serialize_commands());
        return $.ajax({
          url: window.location.origin + '/data.json',
          dataType: 'text',
          data: $scope.serialize_commands(),
          success: function(json_text) {
            var e, json;
            try {
              return json = JSON.parse(json_text);
            } catch (_error) {
              e = _error;
              return window.console.log(e);
            }
          }
        });
      };
    })(this)), 1000);
    return $scope.$on('$destroy', function() {
      return $interval.cancel(timer_id);
    });
  };

}).call(this);
