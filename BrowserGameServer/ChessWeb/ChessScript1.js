//import {Pawn} from "./ChessClasses";

let canvas = document.querySelector("canvas");
let context = canvas.getContext("2d");
canvas.width = innerWidth;
canvas.height = innerHeight;

class Vector {
    constructor(x, y){
        this.x = x;
        this.y = y;
    }

    Sum(addition){
        if(typeof(addition) === "number"){
            let x = this.x + addition;
            let y = this.y + addition;
            return new Vector(x, y);
        }
        else if(typeof(addition) === "object"){
            let x = this.x + addition.x;
            let y = this.y + addition.y;
            return new Vector(x, y);
        }
        else return undefined;
    }

    Multiply(multiplier){
        if(typeof(multiplier) === "number"){
            let x = this.x * multiplier;
            let y = this.y * multiplier;
            return new Vector(x, y);
        }
        else if(typeof(multiplier) === "object"){
            return this.x * multiplier.x + this.y * multiplier.y;
        }
        else return undefined;
    }
}

class Table{
    constructor(){
        this.img = new Image();
        this.img.src = "Chessboard.png";
        this.size = {width: 600, height: 600};
        this.position = new Vector(canvas.width / 2 - this.size.width / 2, canvas.height / 2 - this.size.height / 2);

        this.arrayOfChess = [];
    }

    draw(){
        context.drawImage(this.img, this.position.x, this.position.y, this.size.width, this.size.height);
    }

    update(){}

    addChess(chess){//под позицией имеется ввиду двойной номер ячейки, а не координаты на холсте
        this.arrayOfChess.push(chess); 
    }
}

class Chess{
    constructor(table, tPosition, side){
        this.img = new Image();
        this.table = table;
        this.size = {width: table.size.width / 8, height: table.size.height / 8};

        this.tablePos = new Vector(tPosition.x, tPosition.y);///

        this.oldCanvasPos = getCanvasPosition(this, tPosition);
        this.canvasPosition = getCanvasPosition(this, tPosition);

        this.side = side;

        this.table.addChess(this);
    }

    draw(){
        context.drawImage(this.img, this.canvasPosition.x, this.canvasPosition.y, this.size.width, this.size.height);
    }

    update(){}
}

class Pawn extends Chess{
    constructor(table, tPosition, side) {
        super(table, tPosition, side);
        if(side === 0){
            this.img.src = "PeshkaB.png";
        }
        else{
            this.img.src = "PeshkaW.png";
        }
    }

    allowedShift(shift_x, shift_y){
        //если пешка никуда не ходит
        if (shift_x === 0 && shift_y === 0)
            return true;
        //ходы в бок, назад, либо вперед более чем на 1 клетку недопустимы
        if ((Math.abs(shift_x) > 0 && shift_y === 0) || shift_y > 1 || shift_y < 0)
            return false;
        //двинуться по диагонали если есть фигура и она враг
        else if (shift_x !== 0 && shift_y !== 0 &&
        this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x + shift_x) && (a.tablePos.y === this.tablePos.y + shift_y))){
            if(this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x + shift_x) && (a.tablePos.y === this.tablePos.y + shift_y)).side !== this.side)
                return true;
            else
                return false; 
        }
        //двинуться вперед если никого нет
        else if (shift_x === 0 && shift_y !== 0 &&
        !(this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y + shift_y)))){
            return true;
        }
        else return false;
    }
}

class Queen extends Chess{
    constructor(table, tPosition, side) {
        super(table, tPosition, side);
        if(side === 0){
            this.img.src = "QueenB.png";
        }
        else{
            this.img.src = "QueenW.png";
        }
    }

    allowedShift(shift_x, shift_y){
        //если ход не по диагонали 
        if ((Math.abs(shift_x) !== Math.abs(shift_y)) && (shift_x !== 0 && shift_y !== 0))
            return false;
        if (shift_x === 0 && shift_y === 0)
            return true;

            
        //анализ маршрута вниз от движемой фигуры
        let move_distance_down = 0;
        if(shift_y > 0 && shift_x === 0){
            for(let i = this.tablePos.y; i < 8 && move_distance_down < Math.abs(shift_y); i++){
                move_distance_down += 1;
                if(this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y + move_distance_down))){
                    //move_distance_down--;
                    break;
                }
            }
            if (shift_y <= move_distance_down && 
            !this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y + move_distance_down))){
                return true;
            }
            else if(shift_y <= move_distance_down &&
            this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y + move_distance_down)).side !== this.side)
                return true;
        }

        //анализ маршрута вверх от движемой фигуры
        let move_distance_up = 0;
        if(shift_y < 0 && shift_x === 0){
            for(let i = this.tablePos.y; i > 0 && move_distance_up < Math.abs(shift_y); i--){    
                move_distance_up += 1;
                if(this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y - move_distance_up))){
                    //move_distance_up--;
                    break;
                }
            }
            if (Math.abs(shift_y) <= move_distance_up &&
            !this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y - move_distance_up))){
                return true;
            }
            else if(Math.abs(shift_y) <= move_distance_up &&
            this.table.arrayOfChess.find(a => (a.tablePos.x === this.tablePos.x) && (a.tablePos.y === this.tablePos.y - move_distance_up)).side !== this.side)
                return true;
        }
/*
        //анализ маршрута вправо от движемой фигуры
        int move_distance_right = 0;
        if (shift_y > 0){
            while (this.y + move_distance_right < this.chess_Board.GetLength(1) - 1){
                move_distance_right++;
                if (this.chess_Board[this.x, this.y + move_distance_right] != null){
                    if (this.chess_Board[this.x, this.y + move_distance_right].side == this.side)
                        move_distance_right--;
                    break;
                }
            }
            if (shift_y <= move_distance_right) return true;
        }

        //анализ маршрута влево от движемой фигуры
        int move_distance_left = 0;
        if (shift_y < 0){
            while (this.y + move_distance_left > 0){
                move_distance_left--;
                if (this.chess_Board[this.x, this.y + move_distance_left] != null){
                    if (this.chess_Board[this.x, this.y + move_distance_left].side == this.side)
                        move_distance_left++;
                    break;
                }
            }
            if (shift_y >= move_distance_left) return true;
        }

            //анализ маршрута вниз вправо от движемой фигуры
        move_distance_down = 0;
        move_distance_right = 0;
        if (shift_x > 0 && shift_y > 0)
        {
            while (this.x + move_distance_down < this.chess_Board.GetLength(0) - 1 &&
                    this.y + move_distance_right < this.chess_Board.GetLength(1) - 1)
            {
                move_distance_down++;
                move_distance_right++;
                if (this.chess_Board[this.x + move_distance_down, this.y + move_distance_right] != null)
                {
                    if (this.chess_Board[this.x + move_distance_down, this.y + move_distance_right].side == this.side)
                    {
                        move_distance_down--;
                        move_distance_right--;
                    }
                    break;
                }
            }
            if (shift_x <= move_distance_down && shift_y <= move_distance_right) return true;
        }

        //анализ маршрута вниз влево от движемой фигуры
        move_distance_down = 0;
        move_distance_left = 0;
        if (shift_x > 0 && shift_y < 0)
        {
            while (this.x + move_distance_down < this.chess_Board.GetLength(0) - 1 &&
                    this.y + move_distance_left > 0)
            {
                move_distance_down++;
                move_distance_left--;
                if (this.chess_Board[this.x + move_distance_down, this.y + move_distance_left] != null)
                {
                    if (this.chess_Board[this.x + move_distance_down, this.y + move_distance_left].side == this.side)
                    {
                        move_distance_down--;
                        move_distance_left++;
                    }
                    break;
                }
            }
            if (shift_x <= move_distance_down && shift_y >= move_distance_left) return true;
        }

        //анализ маршрута вверх влево от движемой фигуры
        move_distance_up = 0;
        move_distance_left = 0;
        if (shift_x < 0 && shift_y < 0)
        {
            while (this.x + move_distance_up > 0 &&
                    this.y + move_distance_left > 0)
            {
                move_distance_up--;
                move_distance_left--;
                if (this.chess_Board[this.x + move_distance_up, this.y + move_distance_left] != null)
                {
                    if (this.chess_Board[this.x + move_distance_up, this.y + move_distance_left].side == this.side)
                    {
                        move_distance_up++;
                        move_distance_left++;
                    }
                    break;
                }
            }
            if (shift_x >= move_distance_up && shift_y >= move_distance_left) return true;
        }

        //анализ маршрута вверх вправо от движемой фигуры
        move_distance_up = 0;
        move_distance_right = 0;
        if (shift_x < 0 && shift_y > 0)
        {
            while (this.x + move_distance_up > 0 &&
                    this.y + move_distance_right < this.chess_Board.GetLength(1) - 1)
            {
                move_distance_up--;
                move_distance_right++;
                if (this.chess_Board[this.x + move_distance_up, this.y + move_distance_right] != null)
                {
                    if (this.chess_Board[this.x + move_distance_up, this.y + move_distance_right].side == this.side)
                    {
                        move_distance_up++;
                        move_distance_right--;
                    }
                    break;
                }
            }
            if (shift_x >= move_distance_up && shift_y <= move_distance_right) return true;
        } */

        return false;
    }
}

function getCanvasPosition(chess, tPosition){
    let x = tPosition.x * chess.size.width + chess.table.position.x;
    let y = tPosition.y * chess.size.height + chess.table.position.y;
    return new Vector(x, y);
}

function getCursorTablePosition(chess, cursorPos){
    let x = Math.floor((cursorPos.x - chess.table.position.x) / chess.size.width);
    let y = Math.floor((cursorPos.y - chess.table.position.y) / chess.size.height);
    return new Vector(x, y);
} 

//обновление состояния холста(анимация холста)
function updateCanvas(){

    context.clearRect(0, 0, canvas.width, canvas.height)

    table.draw();
    if(table.update)
        table.update();

    for(let i = 0; i < table.arrayOfChess.length; i++){
        //отрисовывает объект
        table.arrayOfChess[i].draw();
        //обновляет состояние объекта если внутри есть соответствующий метод
        if(table.arrayOfChess[i].update)
            table.arrayOfChess[i].update();
    }

    requestAnimationFrame(updateCanvas);
}

let table = new Table();

new Pawn(table, new Vector(3, 1), 1);
new Pawn(table, new Vector(3, 2), 1);
new Pawn(table, new Vector(3, 6), 0);
new Queen(table, new Vector(3, 4), 0);


canvas.addEventListener("mousedown", mouseDown);
function mouseDown(event){  

    let targetObj;//захваченный объект
    let mousePosInObj;//координаты курсора внутри объекта относительно его верхнего левого угла

    //перебираем объекты из массива объектов-шахмат в объекте доски и выполняем ивент для шахматы на которую указывает курсор
    for(let i = 0; i < table.arrayOfChess.length; i++){
        if(event.clientX > table.arrayOfChess[i].canvasPosition.x &&
            event.clientX < table.arrayOfChess[i].canvasPosition.x + table.arrayOfChess[i].size.width &&
            event.clientY > table.arrayOfChess[i].canvasPosition.y && 
            event.clientY < table.arrayOfChess[i].canvasPosition.y + table.arrayOfChess[i].size.height){
                
                targetObj = table.arrayOfChess[i];

                targetObj.oldCanvasPos.x = targetObj.canvasPosition.x;
                targetObj.oldCanvasPos.y = targetObj.canvasPosition.y;

                mousePosInObj = {
                    dx: event.clientX - targetObj.canvasPosition.x,
                    dy: event.clientY - targetObj.canvasPosition.y
                };
                canvas.addEventListener("mousemove", mouseMove);
                canvas.addEventListener("mouseup", mouseUp);
            }
    }

    function mouseMove(event){
        targetObj.canvasPosition.x = event.clientX - mousePosInObj.dx;
        targetObj.canvasPosition.y = event.clientY - mousePosInObj.dy;
    }

    function mouseUp(event){
        canvas.removeEventListener("mousemove", mouseMove);
        canvas.removeEventListener("mouseup", mouseUp);

        //по текущим координатам захваченной фигуры определяем над какой ячейкой она находится
        let tPos = getCursorTablePosition(targetObj, new Vector(event.clientX, event.clientY));
        let cPos = getCanvasPosition(targetObj, tPos);//вычисляем координаты этой ячейки
        let shift = {//смещение относительно изначального положения
            dx: tPos.x - targetObj.tablePos.x,
            dy: tPos.y - targetObj.tablePos.y
        };

        if(event.clientX > targetObj.oldCanvasPos.x && event.clientX < targetObj.oldCanvasPos.x + targetObj.size.width &&
        event.clientY > targetObj.oldCanvasPos.y && event.clientY < targetObj.oldCanvasPos.y + targetObj.size.height){
            targetObj.canvasPosition.x = targetObj.oldCanvasPos.x;
            targetObj.canvasPosition.y = targetObj.oldCanvasPos.y;
        }
        //если объект отпускают за пределами доски, то его возвращает в исходную позицию
        else if(!(event.clientX > table.position.x && event.clientY > table.position.y &&
            event.clientX < table.position.x + table.size.width && event.clientY < table.position.y + table.size.height)){
                targetObj.canvasPosition.x = targetObj.oldCanvasPos.x;
                targetObj.canvasPosition.y = targetObj.oldCanvasPos.y;
            }
        //проверка на возможность сходить в данную клетку данной фигурой
        else if(!targetObj.allowedShift(shift.dx, shift.dy)){
            targetObj.canvasPosition.x = targetObj.oldCanvasPos.x;
            targetObj.canvasPosition.y = targetObj.oldCanvasPos.y;
        }
        //если объект можно вставить в данную ячейку, то удаляем из этой ячейки(через массив в объекте доски) объект фигуры и 
        //меняем координаты текущей фигуры на координаты этой ячейки
        else{
            let index = targetObj.table.arrayOfChess.findIndex(a => a.tablePos.x === tPos.x && a.tablePos.y === tPos.y);
            targetObj.table.arrayOfChess[index] = null;
            targetObj.table.arrayOfChess = targetObj.table.arrayOfChess.filter(a => a !== null);

            targetObj.tablePos.x = tPos.x;
            targetObj.tablePos.y = tPos.y;
            targetObj.canvasPosition.x = cPos.x;
            targetObj.canvasPosition.y = cPos.y;
            targetObj.oldCanvasPos.x = targetObj.canvasPosition.x;
            targetObj.oldCanvasPos.y = targetObj.canvasPosition.y;
        }
    }
}

updateCanvas();

