var connection;

function openWebsocketConnection(port)
{
    try
    {
        connection = new WebSocket("ws://localhost:" + port + "/ws/");

        // When the connection is open, send some data to the server
        connection.onopen = function ()
        {
            connectionOpened();
            sendPacket('ping', '');
        };

        // Log messages from the server
        connection.onmessage = function (e)
        {
            var packet = getDataFromJSON(e.data);
            packetReceived(packet);
        };

        // Log errors
        connection.onerror = function (error) { console.log('WebSocket Error ' + error); };

        connection.onclose = function (e)
        {
            connectionClosed();
            setTimeout(function () { openWebsocketConnection(port); }, 2000);
        };
    }
    catch (err) { console.log(err); }
}

function sendPacket(type, data)
{
    if (connection != null && connection.readyState == WebSocket.OPEN)
    {
        var packet = { type: type, data: data };
        connection.send(JSON.stringify(packet));
    }
}

function getDataFromJSON(packet)
{
    try
    {
        if (packet != null)
        {
            return JSON.parse(packet);
        }
    }
    catch (err) { console.log(err); }
    return null;
}