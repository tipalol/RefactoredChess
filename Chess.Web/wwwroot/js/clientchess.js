//-----------Configuration----------------------
var board = null;
var game = new Chess();
var $status = $('#status');
var $fen = $('#fen');
var $pgn = $('#pgn');
var currentSide = "no";
var gameOn = false;

var config = {
    orientation: "white",
    draggable: false,
    position: 'start'
};

$pgn[0].hidden = true;
$fen.hidden = true;

board = Chessboard('board', config);

document.getElementById("searchButton").addEventListener("click", function (event) {
    document.getElementById("searchButton").disabled = true;

    connection.invoke("Play").catch(function (err) {
        return console.error(err.toString());
    });
    event.preventDefault();
});

var connection = new signalR.HubConnectionBuilder().withUrl("/aiGame").build();
//---------------Configuration - End----------------

//---------------First Login------------------------
connection.start().then(function () {
    connection.invoke("Login", "Fyodor Likhachev");
}).catch(function (err) {
    return console.error(err.toString());
});

connection.on("LoggedIn", function (user) {
    console.log("User logged in as " + user);
});
//---------------First Login - End-------------------

//---------------Start Game--------------------------
connection.on("GameStarted", function (side){
    $("#searchButton")[0].hidden = true
    $pgn[0].hidden = false;
    gameOn = true;

    var config = {
        orientation: side,
        draggable: true,
        position: 'start',
        onDragStart: onDragStart,
        onDrop: onDrop,
        onSnapEnd: onSnapEnd
    };

    currentSide = config.orientation;
    board = Chessboard('board', config);

    newGame();

    updateStatus();


});
//---------------Start Game - End--------------------

//---------------Game Controls-----------------------
function onDragStart (source, piece, position, orientation) {
    if (game.game_over()) return false;
    if ((game.turn() !== currentSide.charAt(0))) return false;
    if (gameOn === false) return false;

    if ((game.turn() === 'w' && piece.search(/^b/) !== -1) ||
        (game.turn() === 'b' && piece.search(/^w/) !== -1))
        return false;
}

function onDrop (source, target) {
    //console.log("tried to move from " + source + " to " + target);
    // see if the move is legal
    var move = game.move({
        from: source,
        to: target,
        promotion: 'q' // NOTE: always promote to a queen for example simplicity
    });

    // illegal move
    if (move === null) return 'snapback';

    connection.invoke("MakeMove", source, target);

    // to move to response on server validation updateStatus();
    updateStatus();
}
//---------------Game Controls - End-----------------

//---------------Server Messages---------------------
connection.on("ReceiveMove", function(from, to) {
    // see if the move is legal
    var move = game.move({
        from: from,
        to: to,
        promotion: 'q' // NOTE: always promote to a queen for example simplicity
    });
    
    //console.log(moveReceived);
    
    //var move = game.move(moveReceived);

    console.log(move);

    // illegal move
    if (move === null) return 'snapback';

    board.move(from, to);

    if (board.fen() !== game.fen())
        board.position(game.fen(), true);

    updateStatus();
});
//---------------Server Messages - End---------------

//---------------Utils-------------------------------
// update the board position after the piece snap
// for castling, en passant, pawn promotion
function onSnapEnd () {
    board.position(game.fen());
}

function updateStatus () {
    var status = '';

    var moveColor = 'White';
    if (game.turn() === 'b') {
        moveColor = 'Black';
    }

    // checkmate?
    if (game.in_checkmate()) {
        status = 'Game over, ' + moveColor + ' is in checkmate.';
        connection.invoke("Checkmate", moveColor);
        connection.invoke("EndGame", game.fen());
        endGame();
    }

    // draw?
    else if (game.in_draw()) {
        status = 'Game over, drawn position';
        connection.invoke("InDraw");
        connection.invoke("EndGame", game.fen());
        endGame();
    }

    // game still on
    else {
        status = moveColor + ' to move';

        // check?
        if (game.in_check()) {
            status += ', ' + moveColor + ' is in check';
        }
    }

    $status.html(status);
    $pgn.html(game.pgn({ max_width: 5, newline_char: '<br />' }));
    $pgn.scrollTop = $pgn.scrollHeight;
    $("#pgn").scrollTop($("#pgn")[0].scrollHeight);
}

function newGame()
{
    $pgn.html("");
    gameOn = true;
    game = new Chess();
}
//---------------Utils - End-------------------------

//----------Tool Buttons------------
//----------Flip--------------------
document.getElementById("flipButton").addEventListener("click", function (event) {
    board.flip();
    event.preventDefault();
});
//----------Suicide-----------------
document.getElementById("suicideButton").addEventListener("click", function (event) {
    connection.invoke("Suicide");
    $status.html("Game over, you have done harakiri");
    gameOn = false;
    endGame();
    event.preventDefault();
});
//----------Tool Buttons - End-------