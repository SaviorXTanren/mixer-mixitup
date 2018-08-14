// https://stackoverflow.com/questions/2368784/draw-on-html5-canvas-using-a-mouse

var canvas;
var canvasCtx;

var prevX = 0;
var prevY = 0;
var currX = 0;
var currY = 0;

var mouseHeld = false;
var dot_flag = false;

var color = "black";
var lineWidth = 1;

window.addEventListener('load', function initMixer() {
    mixer.isLoaded();

    canvas = document.getElementById('drawingCanvas');
    canvasCtx = canvas.getContext("2d");

    canvas.addEventListener("mousemove", function (e) {
        findxy('move', e)
    }, false);
    canvas.addEventListener("mousedown", function (e) {
        findxy('down', e)
    }, false);
    canvas.addEventListener("mouseup", function (e) {
        findxy('up', e)
    }, false);
    canvas.addEventListener("mouseout", function (e) {
        findxy('out', e)
    }, false);

    $('#blue').click(function() { color = 'blue'; });
    $('#red').click(function() { color = 'red'; });
    $('#green').click(function() { color = 'green'; });
    $('#yellow').click(function() { color = 'yellow'; });
    $('#orange').click(function() { color = 'orange'; });
    $('#purple').click(function() { color = 'purple'; });
    $('#white').click(function() { color = 'white'; });
    $('#black').click(function() { color = 'black'; });

    $('#clearButton').click(function() {
        var canvasRect = canvas.getBoundingClientRect();
        canvasCtx.clearRect(0, 0, canvasRect.width, canvasRect.height); 
    });
});

//mixer.socket.call('giveInput', {
//  controlID: 'draw',
//  event: 'click',
//  button: event.button,
//});

function findxy(res, e) {
    var canvasRect = canvas.getBoundingClientRect();
    var canvasScaleX = canvas.width / canvasRect.width;
    var canvasScaleY = canvas.height / canvasRect.height;

    if (res == 'down') {
        prevX = currX;
        prevY = currY;
        currX = (e.clientX - canvasRect.left) * canvasScaleX;
        currY = (e.clientY - canvasRect.top) * canvasScaleY;

        console.log(currX + "," + currY);

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
        currX = (e.clientX - canvasRect.left) * canvasScaleX;
        currY = (e.clientY - canvasRect.top) * canvasScaleY;
    
        canvasCtx.beginPath();
        canvasCtx.moveTo(prevX, prevY);
        canvasCtx.lineTo(currX, currY);
        canvasCtx.strokeStyle = color;
        canvasCtx.lineWidth = lineWidth;
        canvasCtx.stroke();
        canvasCtx.closePath();
    }
}

function save() {
    document.getElementById("canvasimg").style.border = "2px solid";
    var dataURL = canvas.toDataURL();
    document.getElementById("canvasimg").src = dataURL;
    document.getElementById("canvasimg").style.display = "inline";
}