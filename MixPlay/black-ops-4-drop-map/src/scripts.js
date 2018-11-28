// Gamepad API code from: https://github.com/luser/gamepadtest

const XboxControllerID = "Xbox 360 Controller (XInput STANDARD GAMEPAD)";
const AButtonID = 0;
const XButtonID = 2;
const LeftTriggerID = 6;
const RightTriggerID = 7;
const LeftStickXAxis = 0;
const LeftStickYAxis = 1;

var pcExplanationDiv;
var xboxExplanationDiv;

var mapDiv;
var mapImage;
var mapPointsDiv;

var selectorDiv;

var timerDiv;
var timerText;

var locationDiv;
var locationText;

var timerComplete = false;

var xMouseCoordinate = 0;
var yMouseCoordinate = 0;

var pointsMap = new Map();

var haveGamepadEvents = 'GamepadEvent' in window;
var haveWebkitEvents = 'WebKitGamepadEvent' in window;
var controllers = {};
var previousButtons = {};

var rAF = window.mozRequestAnimationFrame ||
    window.webkitRequestAnimationFrame ||
    window.requestAnimationFrame;

function handleVideoResized(position) {
    const overlay = document.getElementById('overlay');
    const player = position.connectedPlayer;
    overlay.style.top = `${player.top}px`;
    overlay.style.left = `${player.left}px`;
    overlay.style.height = `${player.height}px`;
    overlay.style.width = `${player.width}px`;
}

function pointPlaced(x, y) {
    if (!timerComplete) {
        var xPos = ((x - mapImage.offsetLeft) / mapImage.width) * 100;
        var yPos = ((y - mapImage.offsetTop) / mapImage.height) * 100;

        mixer.socket.call('giveInput', {
            controlID: 'position',
            event: 'mousedown',
            meta: {
                x: xPos,
                y: yPos,
            }
        });
    }
}

function handleControlUpdate(update) {
    if (update.controls.length > 0) {
        var control = update.controls[0];

        if (control.controlID === 'position') {
            if (control.meta.x >= 0 && control.meta.y >= 0 && control.meta.userID != null) {
                var xPos = ((control.meta.x / 100) * mapImage.width) + mapImage.offsetLeft;
                var yPos = ((control.meta.y / 100) * mapImage.height) + mapImage.offsetTop;

                var otherPoint;
                if (pointsMap.has(control.meta.userID)) {
                    otherPoint = pointsMap.get(control.meta.userID);
                }
                else {
                    otherPoint = document.createElement('div');
                    otherPoint.className = "roundImage";

                    var pointImage = document.createElement('img');
                    pointImage.className = "userAvatarImage";
                    pointImage.src = "https://mixer.com/api/v1/users/" + control.meta.userID + "/avatar";
                    otherPoint.appendChild(pointImage);

                    pointsMap.set(control.meta.userID, otherPoint);

                    mapPointsDiv.appendChild(otherPoint);
                }

                otherPoint.style.left = xPos + 'px';
                otherPoint.style.top = yPos + 'px';
            }

            if (control.meta.map != null) {
                mapImage.addEventListener('load', function () {
                    var selectorImage = document.getElementById('selectorImage');
                    selectorImage.style.width = Math.round((mapImage.width / 15)) + "px";
                    selectorImage.style.height = Math.round((mapImage.height / 15)) + "px";
                });
                mapImage.src = control.meta.map;
            }
        }
        else if (control.controlID === 'winner') {
            if (control.meta.userID != null && control.meta.username != null && control.meta.location != null) {

                pcExplanationDiv.style.visibility = 'hidden';
                xboxExplanationDiv.style.visibility = 'hidden';
                timerDiv.style.visibility = 'hidden';

                while (mapPointsDiv.firstChild) {
                    mapPointsDiv.removeChild(mapPointsDiv.firstChild);
                }

                if (pointsMap.has(control.meta.userID)) {
                    mapPointsDiv.appendChild(pointsMap.get(control.meta.userID));
                }

                var winnerImage = document.getElementById('winnerImage');
                winnerImage.src = "https://mixer.com/api/v1/users/" + control.meta.userID + "/avatar";

                var winnerUsername = document.getElementById('winnerUsername');
                winnerUsername.innerHTML = control.meta.username;

                var winnerDiv = document.getElementById('winnerDiv');
                winnerDiv.style.visibility = 'visible';

                var locationText = document.getElementById('locationText');
                locationText.innerHTML = control.meta.location;

                var locationDiv = document.getElementById('locationDiv');
                locationDiv.style.visibility = 'visible';
            }
            else if (control.meta.timeleft != null) {
                if (control.meta.timeleft > 0) {
                    timerDiv.style.visibility = 'visible';
                    timerText.innerHTML = control.meta.timeleft;
                }
                else {
                    timerComplete = true;
                    timerDiv.style.visibility = 'hidden';
                }
            }
        }
    }
}

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
        selectorDiv.style.visibility = 'visible';

        var controller = controllers[j];
        if (controller.id == XboxControllerID) {

            if (controller.axes.length >= LeftStickYAxis) {
                xMovement = parseFloat(controller.axes[LeftStickXAxis].toFixed(2));
                yMovement = parseFloat(controller.axes[LeftStickYAxis].toFixed(2));

                if (xMovement != NaN && (xMovement > 0.05 || xMovement < -0.05) && yMovement != NaN && (yMovement > 0.05 || yMovement < -0.05)) {
                    xGamepadCoordinate += xMovement * 5.0;
                    yGamepadCoordinate += yMovement * 5.0;

                    xGamepadCoordinate = clamp(xGamepadCoordinate, mapImage.offsetLeft, mapImage.offsetLeft + mapImage.width);
                    yGamepadCoordinate = clamp(yGamepadCoordinate, mapImage.offsetTop, mapImage.offsetTop + mapImage.height);

                    if (selectorDiv != null) {
                        selectorDiv.style.left = xGamepadCoordinate + "px";
                        selectorDiv.style.top = yGamepadCoordinate + "px";
                    }
                }
            }

            if (isButtonPressed(controller, AButtonID) || isButtonPressed(controller, XButtonID) ||
                isButtonPressed(controller, LeftTriggerID) || isButtonPressed(controller, RightTriggerID)) {
                pointPlaced(xGamepadCoordinate, yGamepadCoordinate);
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

function clamp(number, min, max) {
    return Math.min(Math.max(number, min), max);
}

window.addEventListener('load', function initMixer() {
    mixer.display.position().subscribe(handleVideoResized);

    mixer.isLoaded();

    pcExplanationDiv = document.getElementById('pcExplanationDiv');
    xboxExplanationDiv = document.getElementById('xboxExplanationDiv');

    mapDiv = document.getElementById('mapDiv');
    mapImage = document.getElementById('mapImage');
    mapPointsDiv = document.getElementById('mapPointsDiv');

    xGamepadCoordinate = mapImage.offsetLeft + (mapImage.width / 2);
    yGamepadCoordinate = mapImage.offsetTop + (mapImage.height / 2);

    selectorDiv = document.getElementById('selectorDiv');
    selectorDiv.style.left = xGamepadCoordinate + "px";
    selectorDiv.style.top = yGamepadCoordinate + "px";

    var selectorImage = document.getElementById('selectorImage');
    selectorImage.style.width = Math.round((mapImage.width / 15)) + "px";
    selectorImage.style.height = Math.round((mapImage.height / 15)) + "px";

    timerDiv = document.getElementById('timerDiv');
    timerText = document.getElementById('timerText');

    $(mapImage).mousemove(function (event) {
        xMouseCoordinate = event.pageX;
        yMouseCoordinate = event.pageY;
    }).mouseleave(function () {
        xMouseCoordinate = 0;
        yMouseCoordinate = 0;
    }).click(function () {
        pointPlaced(xMouseCoordinate, yMouseCoordinate);
    });
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