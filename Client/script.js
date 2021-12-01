var connectButton = document.getElementById("connectButton");
var closeButton = document.getElementById("closeButton");
var sendButton = document.getElementById("sendButton");

var stateLabel = document.getElementById("stateLabel");
var connIdLable = document.getElementById("connIdLable");
var connectionUrl = document.getElementById("connectionUrl");
var sendMessage = document.getElementById("sendMessage");
var commsLog = document.getElementById("commsLog");
var recipents = document.getElementById("recipents");

connectionUrl.value = "ws://localhost:5000";

connectButton.onclick = function () {
    stateLabel.innerHTML = "Attempting to connect...";

    socket = new WebSocket(connectionUrl.value);

    socket.onopen = function (event) {
        updateState();
        commsLog.innerHTML += '<tr>' + '<td colspan="3">Connection opened</td>' + '</tr>';
    }

    socket.onclose = function (event) {
        updateState();
        commsLog.innerHTML += '<tr>' + '<td colspan="3">Connection closed. Code: ' + htmlEscape(event.code) + ' Reason: ' + htmlEscape(event.reason) + '</td>' + '</tr>';
    }

    socket.onmessage = function (event) {
        commsLog.innerHTML += '<tr>' + '<td>Server</td>' + '<td>Client</td>' + '<td>' + htmlEscape(event.data) + '</td></tr>';
        isConnId(event.data);
    }

    socket.onerror = updateState();

    closeButton.onclick = function () {
        validateSocket(socket);

        socket.close(1000, "Closing from client!");
    }

    sendButton.onclick = function () {
        validateSocket(socket);

        let data = constructJSON();
        socket.send(data);

        commsLog.innerHTML += '<tr>' + '<td>Server</td>' + '<td>Client</td>' + '<td>' + htmlEscape(data) + '</td></tr>';
    }

    function validateSocket(socket) {
        if (!socket || socket.readyState !== WebSocket.OPEN) {
            alert("Socket not connected!");
        }
    }

    function isConnId(str) {
        if (str.substring(0, 12) == "ConnectionId") {
            connIdLable.innerHTML = `Connection Id: ${str.substring(13, 50)}`
        }
    }

    function constructJSON() {
        return JSON.stringify({
            "From" : connIdLable.innerHTML.substring(13, connIdLable.innerHTML.length),
            "To" : recipents.value,
            "Message" : sendMessage.value
        });
    }

    function htmlEscape(str) {
        return str.toString()
            .replace(/&/g, '&amp;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#39;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
    }

    function updateState() {

        function disable() {
            sendMessage.disabled = true;
            sendButton.disabled = true;
            closeButton.disabled = true;
            recipents.disabled = true;
        }

        function enable() {
            sendMessage.disabled = false;
            sendButton.disabled = false;
            closeButton.disabled = false;
            recipents.disabled = false;
        }

        connectionUrl.disabled = true;
        connectButton.disabled = true;

        if (!socket) {
            disable();
        } else {
            switch (socket.readyState) {
                case WebSocket.CLOSED:
                    stateLabel.innerHTML = "Closed";
                    connIdLable.innerHTML = "ConnId: N/a";
                    connectionUrl.disabled = false;
                    connectButton.disabled = false;
                    disable();
                    break;
                case WebSocket.CLOSING:
                    stateLabel.innerHTML = "Closing...";
                    disable();
                    break;
                case WebSocket.OPEN:
                    stateLabel.innerHTML = "Open";
                    enable();
                    break;
                default:
                    stateLabel.innerHTML = "Unknown WebSocket, State: " + htmlEscape(socket.readyState);
                    disable();
                    break;
            }
        }
    }
}


