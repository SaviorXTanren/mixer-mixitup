var mainOverlayDiv = document.getElementById('mainOverlay');

function processResultAndRepeat(status, result)
{
    if (status == 200)
    {
        try
        {
            data = JSON.parse(result);

            var addedElement = null;

            if (data.imagePath != null)
            {
                addedElement = document.createElement('img');
                addedElement.id = 'imageOverlay';

                var imageType = "data:image/png";
                if (data.imagePath.endsWith("gif")) {
                    imageType = "data:image/gif";
                }

                addedElement.src = imageType + ";base64," + data.imageData;
            }
            else if (data.text != null)
            {
                addedElement = document.createElement('p');
                addedElement.id = 'textOverlay';
                addedElement.innerHTML = data.text;
            }

            if (addedElement != null)
            {
                addedElement.style.cssText = 'position: absolute; left: ' + data.horizontal.toString() + '%; top: ' +
                    data.vertical.toString() + '%; transform: translate(-50%, -50%);'

                mainOverlayDiv.appendChild(addedElement);

                setTimeout(function () {
                    mainOverlayDiv.removeChild(addedElement);
                    sendGETRequest();
                }, data.duration * 1000);

                return;
            }        
        }
        catch (err) { }
    }
    sendGETRequest();
}

function sendGETRequest() {
    $.ajax({
        url: 'http://localhost:8001/',
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