export class Chess{
    constructor(table, tPosition){
        this.img = new Image();
        this.img.src = "Peshka.png";
        this.table = table;
        this.size = {width: table.size.width / 8, height: table.size.height / 8};

        this.tablePos = new Vector(tPosition.x, tPosition.y);///

        this.oldCanvasPos = getCanvasPosition(this, tPosition);
        this.canvasPosition = getCanvasPosition(this, tPosition);

        this.table.addChess(this);
    }

    draw(){
        context.drawImage(this.img, this.canvasPosition.x, this.canvasPosition.y, this.size.width, this.size.height);
    }

    update(){}
}

export class Pawn extends Chess
    {
        constructor(table, tPosition) {
             super(table, tPosition);
        }

        allowedShift(shift_x, shift_y)
        {
            if (this.side == "Black")
            {
                //черные смещаются в положительную сторону(вниз) на 1 клетку
                if (shift_x > 1 || shift_y > 1 || shift_x < 0)
                    return false;
                if (shift_x == 0 && shift_y == 0)
                    return true;
                //двинуться вперед если впереди есть фигура 
                else if (chess_Board[this.x + shift_x, this.y] != null && shift_x != 0 && shift_y == 0)
                    return false;
                //двинуться по диагонали если есть фигура и она враг
                else if (shift_x != 0 && shift_y != 0 && chess_Board[this.x + shift_x, this.y + shift_y] != null)
                {
                    if (chess_Board[this.x + shift_x, this.y + shift_y].side != this.side)
                        return true;
                    else
                        return false;
                }
                //двинуться вперед если никого нет
                else if (chess_Board[this.x + shift_x, this.y] == null && shift_x != 0 && shift_y == 0)
                    return true;
                else return false;
            }
            else
            {
                //белые смещаются в отрицательную сторону(вверх) на 1 клетку
                if (shift_x < -1 || shift_y < -1 || shift_x > 0)
                    return false;
                if (shift_x == 0 && shift_y == 0)
                    return true;
                //двинуться вперед если впереди есть фигура 
                else if (chess_Board[this.x + shift_x, this.y] != null && shift_x != 0 && shift_y == 0)
                    return false;
                //двинуться по диагонали если есть фигура и она враг
                else if (shift_x != 0 && shift_y != 0 && chess_Board[this.x + shift_x, this.y + shift_y] != null)
                {
                    if (chess_Board[this.x + shift_x, this.y + shift_y].side != this.side)
                        return true;
                    else
                        return false;
                }
                //двинуться вперед если никого нет
                else if (chess_Board[this.x + shift_x, this.y] == null && shift_x != 0 && shift_y == 0)
                    return true;
                else return false;
            }
        }
    }