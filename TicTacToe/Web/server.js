var express  = require('express');
var app      = express();
var http     = require('http').Server(app);
var io       = require('socket.io')(http);

app.use(express.static(__dirname + '/webgl-chat'));

io.on('connection', function(socket) {
	var id = socket.id;
	console.log(id);

	socket.on('open', function(message) {
		socket.broadcast.emit('open', id, message);
	});

	socket.on('error', function(message) {
		socket.broadcast.emit('error', id, message);
	});

	socket.on('req', function(message) {
		socket.broadcast.emit('req', id, message);
	});

	socket.on('disconnect', function(message) {
		console.log(id);
		socket.broadcast.emit('disconnect', id);
	});
});

http.listen(3000, function(){
	console.log('listening on *:3000');
});
