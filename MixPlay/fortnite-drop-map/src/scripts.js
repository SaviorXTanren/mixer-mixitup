//InteractiveConnectedButtonControlModel button = this.buttons.FirstOrDefault(b => b.controlID.Equals(e.input.controlID));
//button.meta["x"] = e.input.meta["x"];
//button.meta["y"] = e.input.meta["y"];
//button.meta["userID"] = e.participantID;


var mapDiv;
var mapImage;

var timerDiv;
var timerText;

var locationDiv;
var locationText;

var timerComplete = false;

var xCoordinate = 0;
var yCoordinate = 0;

var pointsMap = new Map();

function handleVideoResized(position) {
    const overlay = document.getElementById('overlay');
    const player = position.connectedPlayer;
    overlay.style.top = `${player.top}px`;
    overlay.style.left = `${player.left}px`;
    overlay.style.height = `${player.height}px`;
    overlay.style.width = `${player.width}px`;
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

                    mapDiv.appendChild(otherPoint);
                }

                otherPoint.style.left = xPos + 'px';
                otherPoint.style.top = yPos + 'px';
            }
        }
        else if (control.controlID === 'winner') {
            if (control.meta.userID != null && control.meta.username != null && control.meta.location != null) {
                timerDiv.style.visibility = 'hidden';

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

window.addEventListener('load', function initMixer() {
    mixer.display.position().subscribe(handleVideoResized);

    mixer.isLoaded();

    mapDiv = document.getElementById('mapDiv');
    mapImage = document.getElementById('mapImage');

    timerDiv = document.getElementById('timerDiv');
    timerText = document.getElementById('timerText');

    $(mapImage).mousemove(function(event) {
        xCoordinate = event.pageX;
        yCoordinate = event.pageY;
    }).mouseleave(function() {
        xCoordinate = 0;
        yCoordinate = 0;
    }).click(function () {
        if (!timerComplete) {
            var xPos = ((xCoordinate - mapImage.offsetLeft) / mapImage.width) * 100;
            var yPos = ((yCoordinate - mapImage.offsetTop) / mapImage.height) * 100;

            mixer.socket.call('giveInput', {
                controlID: 'position',
                event: 'click',
                meta: {
                    x: xPos,
                    y: yPos,
                }
            });
        }
    });
});

mixer.socket.on('onControlUpdate', handleControlUpdate);