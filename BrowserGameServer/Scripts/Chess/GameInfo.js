let GameStates = {
    WaitBegining: "WaitBegining",
    ActiveLeading: "ActiveLeading",
    ActiveWaiting: "ActiveWaiting",
    Winner: "Winner",
    Loser: "Loser",
    Disconnected: "Disconnected"
};
let PlayerSides = {
    Black: "Black",
    White: "White"
};

function numberToEnum(gameInfo) {
    switch (gameInfo.Side) {
        case 0:
            gameInfo.Side = "Black";
            break;
        case 1:
            gameInfo.Side = "White";
            break;
    }
    switch (gameInfo.PlayerState) {
        case 0:
            gameInfo.PlayerState = "WaitBegining";
            break;
        case 1:
            gameInfo.PlayerState = "ActiveLeading";
            break;
        case 2:
            gameInfo.PlayerState = "ActiveWaiting";
            break;
        case 3:
            gameInfo.PlayerState = "Winner";
            break;
        case 4:
            gameInfo.PlayerState = "Loser";
            break;
    }
}