// https://stackoverflow.com/questions/2368784/draw-on-html5-canvas-using-a-mouse

var canvas;
var canvasCtx;
var paintSwatchTable;

var prevX = 0;
var prevY = 0;
var currX = 0;
var currY = 0;

var mouseHeld = false;
var dot_flag = false;

var selectorDiv;

var presentImage;
var presenterDiv;
var presenterNameText;
var presenterImage;

var color = "black";
var lineWidth = 1;

// Gamepad API code from: https://github.com/luser/gamepadtest

const XboxControllerID = "Xbox 360 Controller (XInput STANDARD GAMEPAD)";
const AButtonID = 0;
const XButtonID = 2;
const LeftTriggerID = 6;
const RightTriggerID = 7;
const LeftStickXAxis = 0;
const LeftStickYAxis = 1;

var xGamepadCoordinate;
var yGamepadCoordinate;

var haveGamepadEvents = 'GamepadEvent' in window;
var haveWebkitEvents = 'WebKitGamepadEvent' in window;
var controllers = {};
var previousButtons = {};

var rAF = window.mozRequestAnimationFrame ||
    window.webkitRequestAnimationFrame ||
    window.requestAnimationFrame;

function handleControlUpdate(update) {
    if (update.controls.length > 0) {
        var control = update.controls[0];
        if (control.controlID === 'present' && control.meta.image != null) {
			
			canvas.style.visibility = 'hidden';
			paintSwatchTable.style.visibility = 'hidden';
			presentImage.style.visibility = 'visible';
			presenterDiv.style.visibility = 'visible';
			
			presenterImage.src = control.meta.useravatar;
			presenterNameText.innerHTML = control.meta.username;
			
			presentImage.src = control.meta.image;
			
			setTimeout(function() {
				canvas.style.visibility = 'visible';
				paintSwatchTable.style.visibility = 'visible';
				presentImage.style.visibility = 'hidden';
				presenterDiv.style.visibility = 'hidden';
				
				presenterImage.src = '';
				presenterNameText.innerHTML = '';
			}, 8000);
        }
    }
}

function handleMouseDrawing(res, e) {
	handleDrawing(res, e.clientX, e.clientY);
}

function handleDrawing(res, x, y) {
    var canvasRect = canvas.getBoundingClientRect();
    var canvasScaleX = canvas.width / canvasRect.width;
    var canvasScaleY = canvas.height / canvasRect.height;

    if (res == 'down') {
        prevX = currX;
        prevY = currY;
        currX = (x - canvasRect.left) * canvasScaleX;
        currY = (y - canvasRect.top) * canvasScaleY;

        mouseHeld = true;
        dot_flag = true;
        if (dot_flag) {
            canvasCtx.beginPath();
            canvasCtx.fillStyle = color;
            canvasCtx.fillRect(currX, currY, 2, 2);
            canvasCtx.closePath();
            dot_flag = false;
        }
    }

    if (res == 'up' || res == "out") {
        mouseHeld = false;
    }

    if (res == 'move' && mouseHeld) {
        prevX = currX;
        prevY = currY;
        currX = (x - canvasRect.left) * canvasScaleX;
        currY = (y - canvasRect.top) * canvasScaleY;
    
        canvasCtx.beginPath();
        canvasCtx.moveTo(prevX, prevY);
        canvasCtx.lineTo(currX, currY);
        canvasCtx.strokeStyle = color;
        canvasCtx.lineWidth = lineWidth;
        canvasCtx.stroke();
        canvasCtx.closePath();
    }
}

function clearCanvas() {
	var canvasRect = canvas.getBoundingClientRect();
	canvasCtx.clearRect(0, 0, canvasRect.width, canvasRect.height); 
}

function savePicture() {
	var link = document.getElementById('savelink');
	link.setAttribute('download', 'MixerPaint.png');
	link.setAttribute('href', canvas.toDataURL("image/png").replace("image/png", "image/octet-stream"));
	link.click();
}

function sendPicture() {
	mixer.socket.call('giveInput', {
		controlID: 'send',
		event: 'mousedown',
		meta: {
			image: canvas.toDataURL("image/png")
		}
	});
}

// Gamepad Methods

function connecthandler(e) {
    addgamepad(e.gamepad);
}
function addgamepad(gamepad) {
    controllers[gamepad.index] = gamepad;
    rAF(updateStatus);
}

function disconnecthandler(e) {
    removegamepad(e.gamepad);
}

function removegamepad(gamepad) {
    var d = document.getElementById("controller" + gamepad.index);
    document.body.removeChild(d);
    delete controllers[gamepad.index];
}

function updateStatus() {
    scangamepads();
    for (j in controllers) {
        var controller = controllers[j];
        if (controller.id == XboxControllerID) {
			selectorDiv.style.visibility = 'visible';
			
			var canvasRect = canvas.getBoundingClientRect();
		
            if (controller.axes.length >= LeftStickYAxis) {
                xMovement = parseFloat(controller.axes[LeftStickXAxis].toFixed(2));
                yMovement = parseFloat(controller.axes[LeftStickYAxis].toFixed(2));

                if (xMovement != NaN && (xMovement > 0.05 || xMovement < -0.05) && yMovement != NaN && (yMovement > 0.05 || yMovement < -0.05)) {
                    xGamepadCoordinate += xMovement * 5.0;
                    yGamepadCoordinate += yMovement * 5.0;
					
                    xGamepadCoordinate = clamp(xGamepadCoordinate, 0, canvasRect.width);
                    yGamepadCoordinate = clamp(yGamepadCoordinate, 0, canvasRect.height);

                    if (selectorDiv != null) {
                        selectorDiv.style.left = xGamepadCoordinate + "px";
                        selectorDiv.style.top = yGamepadCoordinate + "px";
                    }
                }
            }
			
			var xDrawCoordinate = xGamepadCoordinate + canvasRect.x - 4;
			var yDrawCoordinate = yGamepadCoordinate + canvasRect.y - 4;
			
			if (isButtonPressed(controller, AButtonID) || isButtonPressed(controller, XButtonID) ||
                isButtonPressed(controller, LeftTriggerID) || isButtonPressed(controller, RightTriggerID)) {
				
				if (isGamepadOverLocation('blue', xDrawCoordinate, yDrawCoordinate)) {
					color = 'blue';
				}
				else if (isGamepadOverLocation('red', xDrawCoordinate, yDrawCoordinate)) {
					color = 'red';
				}
				else if (isGamepadOverLocation('green', xDrawCoordinate, yDrawCoordinate)) {
					color = 'green';
				}
				else if (isGamepadOverLocation('yellow', xDrawCoordinate, yDrawCoordinate)) {
					color = 'yellow';
				}
				else if (isGamepadOverLocation('orange', xDrawCoordinate, yDrawCoordinate)) {
					color = 'orange';
				}
				else if (isGamepadOverLocation('purple', xDrawCoordinate, yDrawCoordinate)) {
					color = 'purple';
				}
				else if (isGamepadOverLocation('white', xDrawCoordinate, yDrawCoordinate)) {
					color = 'white';
				}
				else if (isGamepadOverLocation('black', xDrawCoordinate, yDrawCoordinate)) {
					color = 'black';
				}
				else if (isGamepadOverLocation('clearButton', xDrawCoordinate, yDrawCoordinate)) {
					clearCanvas();
				}
				else if (isGamepadOverLocation('sendButton', xDrawCoordinate, yDrawCoordinate)) {
					sendPicture();
				}
			}

            if (isButtonHeld(controller, AButtonID) || isButtonHeld(controller, XButtonID) ||
                isButtonHeld(controller, LeftTriggerID) || isButtonHeld(controller, RightTriggerID)) {
				handleDrawing('down', xDrawCoordinate, yDrawCoordinate);
            }
			else {
				handleDrawing('up', xDrawCoordinate, yDrawCoordinate);
			}

            break;
        }
    }

    rAF(updateStatus);
}

function scangamepads() {
    var gamepads = navigator.getGamepads ? navigator.getGamepads() : (navigator.webkitGetGamepads ? navigator.webkitGetGamepads() : []);
    for (var i = 0; i < gamepads.length; i++) {
        if (gamepads[i]) {
            if (!(gamepads[i].index in controllers)) {
                addgamepad(gamepads[i]);
            } else {
                controllers[gamepads[i].index] = gamepads[i];
            }
        }
    }
}

function isButtonPressed(controller, index) {
    var pressed = false;
    if (controller.buttons.length >= index) {
        var value = controller.buttons[index];
        var active = value == 1.0;
        if (typeof (value) == "object") {
            active = value.pressed;
        }

        pressed = !previousButtons[index] && active;

        previousButtons[index] = active;
    }
    return pressed;
}

function isButtonHeld(controller, index) {
    var held = false;
    if (controller.buttons.length >= index) {
        var value = controller.buttons[index];
        var held = value == 1.0;
        if (typeof (value) == "object") {
            held = value.pressed;
        }
    }
    return held;
}

function isGamepadOverLocation(itemName, x, y) {
	var item = document.getElementById(itemName);
	if (item != null) {
		var rect = item.getBoundingClientRect();
		return (rect.left <= x && x <= (rect.left + rect.width) &&
			rect.top <= y && y <= (rect.top + rect.height));	
	}
	return false;
}

function clamp(number, min, max) {
    return Math.min(Math.max(number, min), max);
}

window.addEventListener('load', function initMixer() {
    mixer.isLoaded();
	
	selectorDiv = document.getElementById('selectorDiv');
    selectorDiv.style.left = xGamepadCoordinate + "px";
    selectorDiv.style.top = yGamepadCoordinate + "px";

    canvas = document.getElementById('drawingCanvas');
    canvasCtx = canvas.getContext("2d");
	
	var canvasRect = canvas.getBoundingClientRect();
	xGamepadCoordinate = canvasRect.left + (canvasRect.width / 2);
    yGamepadCoordinate = canvasRect.top + (canvasRect.height / 2);
	
    selectorDiv.style.left = xGamepadCoordinate + "px";
    selectorDiv.style.top = yGamepadCoordinate + "px";
	
	presentImage = document.getElementById('presentImage');
	presenterDiv = document.getElementById('presenterDiv');
	presenterNameText = document.getElementById('presenterName');
	presenterImage = document.getElementById('presenterImage');
	
	paintSwatchTable = document.getElementById('paintSwatchTable');

    canvas.addEventListener("mousemove", function (e) {
		handleDrawing('move', e.clientX, e.clientY);
    }, false);
    canvas.addEventListener("mousedown", function (e) {
		handleDrawing('down', e.clientX, e.clientY);
    }, false);
    canvas.addEventListener("mouseup", function (e) {
		handleDrawing('up', e.clientX, e.clientY);
    }, false);
    canvas.addEventListener("mouseout", function (e) {
		handleDrawing('out', e.clientX, e.clientY);
    }, false);

    $('#blue').click(function() { color = 'blue'; });
    $('#red').click(function() { color = 'red'; });
    $('#green').click(function() { color = 'green'; });
    $('#yellow').click(function() { color = 'yellow'; });
    $('#orange').click(function() { color = 'orange'; });
    $('#purple').click(function() { color = 'purple'; });
    $('#white').click(function() { color = 'white'; });
    $('#black').click(function() { color = 'black'; });

    $('#clearButton').click(function() { clearCanvas(); });
	$('#saveButton').click(function() { savePicture(); });
	$('#sendButton').click(function() { sendPicture(); });
});

if (haveGamepadEvents) {
    window.addEventListener("gamepadconnected", connecthandler);
    window.addEventListener("gamepaddisconnected", disconnecthandler);
} else if (haveWebkitEvents) {
    window.addEventListener("webkitgamepadconnected", connecthandler);
    window.addEventListener("webkitgamepaddisconnected", disconnecthandler);
} else {
    setInterval(scangamepads, 500);
}

mixer.socket.on('onControlUpdate', handleControlUpdate);