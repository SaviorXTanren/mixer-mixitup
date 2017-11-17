var app = require('express')();
var http = require('http').Server(app);
var io = require('socket.io')(http);

app.get('/', function(req, res){
	res.sendFile(__dirname + '/OverlayPage.html');
});

io.on('connection', function(socket) {
	console.log('client connected');
	
	socket.on('disconnect', function(){
		console.log('client disconnected');
	});
	
	socket.on('test', function(packet) {
		console.log('test receieved');
		io.emit('test', packet);
	});
	
	socket.on('image', function(packet) {
		console.log('image receieved');
		io.emit('image', packet);
	});
	
	socket.on('text', function(packet) {
		console.log('text receieved');
		io.emit('text', packet);
    });

    socket.on('htmlText', function (packet) {
        console.log('htmlText receieved');
        io.emit('htmlText', packet);
    });
});

http.listen(8111, function(){
    console.log('listening on *:8111');
});