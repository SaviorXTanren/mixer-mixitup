//InteractiveConnectedButtonControlModel button = this.buttons.FirstOrDefault(b => b.controlID.Equals(e.input.controlID));
//button.meta["x"] = e.input.meta["x"];
//button.meta["y"] = e.input.meta["y"];
//button.meta["userID"] = e.participantID;


var mapDiv;
var mapImage;

var xCoordinate = 0;
var yCoordinate = 0;
var mypoint = null;

var pointsMap = new Map();

function handleVideoResized(position) {
  const overlay = document.getElementById('overlay');
  const player = position.connectedPlayer;
  //overlay.style.top = `${player.top}px`;
  //overlay.style.left = `${player.left}px`;
  //overlay.style.height = `${player.height}px`;
  //overlay.style.width = `${player.width}px`;
}

function handleControlUpdate(update) {
  const filteredControls = update.controls.filter(c => c.controlID === 'position');
  if (filteredControls.length !== 1) {
      return;
  }

  const positionControl = filteredControls[0];
  if (positionControl.meta.x > 0 && positionControl.meta.y > 0 && positionControl.meta.userID !== null)
  {
    if (pointsMap.has(positionControl.meta.userID))
    {
      mapDiv.removeChild(pointsMap.get(positionControl.meta.userID));
    }

    var otherPoint = document.createElement('img');
    otherPoint.className += " otherPointImage";
    otherPoint.src = "otherpoint.png";
    otherPoint.style.position = 'absolute';
    otherPoint.style.left = (positionControl.meta.x + mapImage.offsetLeft - 5) + 'px';
    otherPoint.style.top = (positionControl.meta.y + mapImage.offsetTop - 5) + 'px';

    pointsMap.set(positionControl.meta.userID, otherPoint);

    mapDiv.appendChild(otherPoint);
  }
}

window.addEventListener('load', function initMixer() {
  mixer.display.position().subscribe(handleVideoResized);

  // Move the video by a static offset amount
  const offset = 50;
  mixer.display.moveVideo({
    top: offset,
    bottom: offset,
    left: offset,
    right: offset,
  });

  mixer.isLoaded();

  mapDiv = document.getElementById('mapDiv');
  mapImage = document.getElementById('mapImage');

  $(mapImage).mousemove(function(event) {
    xCoordinate = event.pageX;
    yCoordinate = event.pageY;
  }).mouseleave(function() {
    xCoordinate = 0;
    yCoordinate = 0;
  }).click(function() {
    if (mypoint !== null) {
      mapDiv.removeChild(mypoint);
    }
    mypoint = document.createElement('img');
    mypoint.className += " myPointImage";
    mypoint.src = "mypoint.png";
    mypoint.style.position = 'absolute';
    mypoint.style.left = (xCoordinate - 5) + 'px';
    mypoint.style.top = (yCoordinate - 5) + 'px';
    mapDiv.appendChild(mypoint);

    mixer.socket.call('giveInput', {
      controlID: 'position',
      event: 'click',
      meta: {
        x: xCoordinate - mapImage.offsetLeft,
        y: yCoordinate - mapImage.offsetTop,
      }
    });
  });
});

mixer.socket.on('onControlUpdate', handleControlUpdate);