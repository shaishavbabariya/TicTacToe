var socket = io() || {};
socket.isReady = false;

window.addEventListener('load', function() {
	var execInUnity = function(method) {
		if (!socket.isReady) return;
		var args = Array.prototype.slice.call(arguments, 1);
		SendMessage("SocketHandler", method, args.join(','));
	};
	socket.on('open', function(message) {
		execInUnity('OnOpen',message);
	});

	socket.on('req', function(message) {
		execInUnity('OnReceive',message);
	});

	socket.on('error', function(message) {
		execInUnity('OnError', message);
	});

	socket.on('close', function(message) {
		execInUnity('OnClose', message);
	});

	var name = 'Unknown';
	var message = 'Hello!';
	var nameButton = document.getElementById('name');
	var messageButton = document.getElementById('message');
	var update = function() {
		SendMessage("Player", "Talk", '@' + name + ': ' + message);
	};
	nameButton.addEventListener('click', function() {
		name = window.prompt('Your Name', 'Unknown');
		update();
	});
	messageButton.addEventListener('click', function() {
		message = window.prompt('Message (English only)', 'Hello!');
		update();
	});
});
