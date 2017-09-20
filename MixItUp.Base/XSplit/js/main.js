var xjs = require('xjs');

var dataDiv = document.getElementById('data');

function processResultAndRepeat(status, result)
{
	if (status == 200)
	{
		try
		{
			data = JSON.parse(result);
		
			if (data.sceneName != null)
			{
				dataDiv.innerHTML += 'Scene Transition: ' + data.sceneName + '<BR>';
				
				xjs.ready().then(function()
				{
					return xjs.Scene.getByName(data.sceneName);
				}).then(function(scenes)
				{
					if (scenes != null && scenes.length > 0)
					{
						xjs.Scene.setActiveScene(scenes[0]);
					}
				});
			}
			else if (data.sourceName != null)
			{
				dataDiv.innerHTML += 'Source Visibility Changed: ' + data.sourceName + ' - ' + data.sourceVisible + '<BR>';
				
				xjs.ready().then(function()
				{
					return xjs.Scene.getActiveScene();
				}).then(function(scene)
				{
					return scene.getSources();
				}).then(function(sources)
				{
					if (sources != null && sources.length > 0)
					{
						for (let i = 0; i < sources.length; i++)
						{
							if (sources[i]._cname == data.sourceName)
							{
								sources[i].getItemList().then(function(items)
								{
									if (items != null && items.length > 0)
									{
										for (let j = 0; j < items.length; j++)
										{
											try
											{
												items[j].setVisible(data.sourceVisible);
											}
											catch (err) {}
										}
									}
								});
							}
						}
					}
				})		
			}	
		}
		catch (err)
		{
			dataDiv.innerHTML += 'Error Occurred: ' + err.message + '<BR>';
		}
	}

	sendGETRequest();
}

function sendGETRequest()
{
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

sendGETRequest();