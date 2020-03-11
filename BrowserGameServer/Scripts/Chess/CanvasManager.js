class CanvasManager {
    constructor() {
        this.canvas = document.querySelector("canvas");
        this.context = this.canvas.getContext("2d");
        this.context.font = "22px Verdana";
        this.canvas.width = innerWidth;
        this.canvas.height = innerHeight;
    }

    //обработчик позволяющий двигать объекты шахмат
    mouseDown(event) {
        if (gameState === "ActiveLeading" && !chessManager.isMoved) {
            let targetObj;//захваченный(целевой) объект
            let mousePosInObj;//координаты курсора внутри объекта фигуры относительно его верхнего левого угла

            //перебираем фигуры в коллекции объектов-фигур в объекте доски и
            //выполняем ивент для шахматы на которую указывает курсор
            for (let pairIdChess of chessManager.board.dictOfChess) {
                //определяем в области какой фигуры был произведен клик
                if (event.clientX > pairIdChess[1].canvasPosition.x &&
                    event.clientX < pairIdChess[1].canvasPosition.x + pairIdChess[1].size.width &&
                    event.clientY > pairIdChess[1].canvasPosition.y &&
                    event.clientY < pairIdChess[1].canvasPosition.y + pairIdChess[1].size.height) {

                    targetObj = pairIdChess[1];
                    if (playerSide !== targetObj.side) break;//можно перетаскивать только фигуры своего цвета

                    //запоминаем где стояла фигура, которую сейчас будут перемещать
                    targetObj.oldCanvasPos.x = targetObj.canvasPosition.x;
                    targetObj.oldCanvasPos.y = targetObj.canvasPosition.y;

                    mousePosInObj = {
                        dx: event.clientX - targetObj.canvasPosition.x,
                        dy: event.clientY - targetObj.canvasPosition.y
                    };

                    canvasManager.canvas.addEventListener("mousemove", mouseMove);
                    canvasManager.canvas.addEventListener("mouseup", mouseUp);
                }
            }

            function mouseMove(event) {
                //получаем относительные координаты для передачи их ожидающему игроку
                targetObj.relativeFromBoardPosition.x = (event.clientX - chessManager.board.position.x - mousePosInObj.dx) / chessManager.board.size.width;
                targetObj.relativeFromBoardPosition.y = (event.clientY - chessManager.board.position.y - mousePosInObj.dy) / chessManager.board.size.height;

                //по относительным получаем абсолютные текущие координаты
                targetObj.canvasPosition.x = chessManager.board.position.x + targetObj.relativeFromBoardPosition.x * chessManager.board.size.width;
                targetObj.canvasPosition.y = chessManager.board.position.y + targetObj.relativeFromBoardPosition.y * chessManager.board.size.height;
            }

            function mouseUp(event) {
                canvasManager.canvas.removeEventListener("mousemove", mouseMove);
                canvasManager.canvas.removeEventListener("mouseup", mouseUp);

                //по текущим координатам захваченной фигуры определяем над какой ячейкой она находится
                let bPos = chessManager.getLocalCellNumberUnderCursor(targetObj, new Vector(event.clientX, event.clientY));
                let cPos = chessManager.getGlobalCanvasPositionOfCell(targetObj, bPos);//вычисляем координаты этой ячейки
                let shift = {//смещение относительно изначального положения(в клетках)
                    dx: bPos.x - targetObj.boardPos.x,
                    dy: bPos.y - targetObj.boardPos.y
                };

                //если объект отпускают в исходной клетке, то его возвращает в исходную позицию
                if (event.clientX > targetObj.oldCanvasPos.x && event.clientX < targetObj.oldCanvasPos.x + targetObj.size.width &&
                    event.clientY > targetObj.oldCanvasPos.y && event.clientY < targetObj.oldCanvasPos.y + targetObj.size.height) {
                    targetObj.relativeFromBoardPosition.x = (targetObj.oldCanvasPos.x - chessManager.board.position.x) / chessManager.board.size.width;
                    targetObj.relativeFromBoardPosition.y = (targetObj.oldCanvasPos.y - chessManager.board.position.y) / chessManager.board.size.height;

                    targetObj.canvasPosition.x = chessManager.board.position.x + targetObj.relativeFromBoardPosition.x * chessManager.board.size.width;
                    targetObj.canvasPosition.y = chessManager.board.position.y + targetObj.relativeFromBoardPosition.y * chessManager.board.size.height;
                }
                //если объект отпускают за пределами доски, то его возвращает в исходную позицию
                else if (!(
                    event.clientX > chessManager.board.position.x &&
                    event.clientY > chessManager.board.position.y &&
                    event.clientX < chessManager.board.position.x + chessManager.board.size.width &&
                    event.clientY < chessManager.board.position.y + chessManager.board.size.height)) {
                    targetObj.relativeFromBoardPosition.x = (targetObj.oldCanvasPos.x - chessManager.board.position.x) / chessManager.board.size.width;
                    targetObj.relativeFromBoardPosition.y = (targetObj.oldCanvasPos.y - chessManager.board.position.y) / chessManager.board.size.height;

                    targetObj.canvasPosition.x = chessManager.board.position.x + targetObj.relativeFromBoardPosition.x * chessManager.board.size.width;
                    targetObj.canvasPosition.y = chessManager.board.position.y + targetObj.relativeFromBoardPosition.y * chessManager.board.size.height;
                }
                //проверка на возможность сходить в данную клетку данной фигурой
                else if (!targetObj.allowedShift(shift.dx, shift.dy)) {
                    targetObj.relativeFromBoardPosition.x = (targetObj.oldCanvasPos.x - chessManager.board.position.x) / chessManager.board.size.width;
                    targetObj.relativeFromBoardPosition.y = (targetObj.oldCanvasPos.y - chessManager.board.position.y) / chessManager.board.size.height;

                    targetObj.canvasPosition.x = chessManager.board.position.x + targetObj.relativeFromBoardPosition.x * chessManager.board.size.width;
                    targetObj.canvasPosition.y = chessManager.board.position.y + targetObj.relativeFromBoardPosition.y * chessManager.board.size.height;
                }
                //если объект можно вставить в данную ячейку, то удаляем из этой ячейки(через словарь в объекте доски) объект фигуры и
                //меняем координаты текущей фигуры на координаты этой ячейки
                else {
                    let idOfChessToDelete;
                    for (let pairIdChess of chessManager.board.dictOfChess) {
                        if (pairIdChess[1].boardPos.x === bPos.x && pairIdChess[1].boardPos.y === bPos.y) {
                            idOfChessToDelete = pairIdChess[0];
                            chessManager.board.dictOfChess.delete(idOfChessToDelete);
                            break;
                        }
                    }

                    targetObj.boardPos.x = bPos.x;
                    targetObj.boardPos.y = bPos.y;
                    targetObj.canvasPosition.x = cPos.x;
                    targetObj.canvasPosition.y = cPos.y;
                    targetObj.oldCanvasPos.x = targetObj.canvasPosition.x;
                    targetObj.oldCanvasPos.y = targetObj.canvasPosition.y;

                    targetObj.relativeFromBoardPosition = chessManager.getPositionRelativeToBoard(targetObj.canvasPosition);

                    chessManager.isMoved = true;
                }
            }
        }
    }

    //парсит ответ от сервера(WebSocket)
    parseResponse(response) {
        //Header1:blabla1<delimiter>Header2:blabla2<delimiter>...<delimiter> -> 
        //-> [Header1: blabla1], [Header2: blabla2]...
        let tempReq = response.split("<delimiter>");
        tempReq = tempReq.slice(0, tempReq.length - 1);
        let parsedResult = new Map();
        let temp1 = null;
        for (let i = 0; i < tempReq.length; i++) {
            temp1 = tempReq[i].split(":");
            parsedResult.set(temp1[0], temp1[1]);
        }
        return parsedResult;
    }

    //формирует строку сообщения серверу(WebSocket)
    buildRequest(subRequests) {
        let result = "";
        for (let key of subRequests.keys()) {
            result += key + ":" + subRequests.get(key) + "<delimiter>";
        }
        return result;
    }
}

function startAnimationCycle() {
    canvasManager.context.clearRect(0, 0, canvasManager.canvas.width, canvasManager.canvas.height)

    chessManager.board.draw();

    for (let pairIdChess of chessManager.board.dictOfChess)
        pairIdChess[1].draw();


    //TESTS
    canvasManager.context.fillText("Player state: " + gameState, 20, 140);
    canvasManager.context.fillText("Side: " + playerSide, 20, 150);

    requestAnimationFrame(startAnimationCycle);
}