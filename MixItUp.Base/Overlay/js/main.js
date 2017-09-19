function processResultAndRepeat(status, result)
{
    if (status == 200)
    {
        try
        {
            data = JSON.parse(result);

            var imageElement = document.createElement('img');
            imageElement.id = 'imageOverlay';

            var imageType = "data:image/png";
            if (data.filePath.endsWith("gif")) {
                imageType = "data:image/gif";
            }

            imageElement.src = imageType + ";base64," + data.fileData;
            imageElement.style.cssText = 'position: absolute; left: ' + data.horizontal.toString() + '%; top: ' +
                data.vertical.toString() + '%; transform: translate(-50%, -50%);'

            var mainOverlayDiv = document.getElementById('mainOverlay');
            mainOverlayDiv.appendChild(imageElement);

            setTimeout(function ()
            {
                mainOverlayDiv.removeChild(imageElement);
                sendGETRequest();
            }, data.duration * 1000);

            return;
        }
        catch (err) { }
    }
    sendGETRequest();
}

function sendGETRequest() {
    $.ajax({
        url: 'http://localhost:8201/',
        type: 'GET',
        timeout: 2000,
    })
	.success(function (data, textStatus, jqXHR) {
	    processResultAndRepeat(jqXHR.status, data);
	})
	.error(function (jqXHR, textStatus) {
	    processResultAndRepeat(-1, '');
	});
}

function sleep(milliseconds) {
    return new Promise(resolve => setTimeout(resolve, milliseconds));
}

sendGETRequest();