'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = BarcodeScanner;
function BarcodeScanner() {
  var _this = this;

  this.handlers = [];
  var scanningBarcode = false;
  var lastKey = '';
  var chars = [];

  window.addEventListener('keydown', function (e) {
    switch (e.key) {
      case 'Shift':
        if (lastKey === 'F11') {
          scanningBarcode = true;
          e.preventDefault();
          chars = [];
        }
        break;
      case 'Enter':
        if (scanningBarcode) {
          e.preventDefault();
          _this.value = chars.join('');
          var event = { target: _this, data: _this.value };
          _this.dispatchEvent('scan', event);
          scanningBarcode = false;
        }
        break;
      default:
        if (scanningBarcode) {
          e.preventDefault();
          chars.push(e.key);
        }
        break;
    }
    lastKey = e.key;
  }, true);
}

BarcodeScanner.prototype.addEventListener = function addEventListener(eventName, eventHandler) {
  if (!this.handlers[eventName]) {
    this.handlers[eventName] = [];
  }
  this.handlers[eventName].push(eventHandler);
};
BarcodeScanner.prototype.removeEventListener = function removeEventListener(eventName, eventHandler) {
  this.handlers[eventName] = this.handlers[eventName].filter(function (handler) {
    return handler !== eventHandler;
  });
};
BarcodeScanner.prototype.dispatchEvent = function dispatchEvent(eventName, eventObject) {
  var _this2 = this;

  if (this.handlers && this.handlers[eventName]) {
    this.handlers[eventName].forEach(function (handler) {
      handler.call(_this2, eventObject);
    });
  }
};