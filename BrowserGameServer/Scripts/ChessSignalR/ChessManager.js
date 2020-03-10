class Vector {
    constructor(x, y) {
        this.x = x;
        this.y = y;
    }

    Sum(addition) {
        if (typeof (addition) === "number") {
            let x = this.x + addition;
            let y = this.y + addition;
            return new Vector(x, y);
        }
        else if (typeof (addition) === "object") {
            let x = this.x + addition.x;
            let y = this.y + addition.y;
            return new Vector(x, y);
        }
        else return undefined;
    }

    Multiply(multiplier) {
        if (typeof (multiplier) === "number") {//произведение вектора на число
            let x = this.x * multiplier;
            let y = this.y * multiplier;
            return new Vector(x, y);
        }
        else if (typeof (multiplier) === "object"/*!!!*/) {//скалярное произведение вектора на вектор через координаты
            return this.x * multiplier.x + this.y * multiplier.y;
        }
        else return undefined;
    }
}

class Board {//шахматная доска
    constructor() {
        //выставляем размеры игровой области относительно меньшей из длин экрана
        if (innerHeight < innerWidth)
            this.size = { width: innerHeight, height: innerHeight };
        else
            this.size = { width: innerWidth, height: innerWidth };

        //координаты игровой области(ш/доски) в холсте
        this.position = new Vector(canvasManager.canvas.width / 2 - this.size.width / 2, canvasManager.canvas.height / 2 - this.size.height / 2);

        //словарь пар [id_шахматы:объект_шахматы]
        this.dictOfChess = new Map();

        this.img = new Image();
        this.imgIsLoaded = false;
        this.img.onload = () => {
            this.imgIsLoaded = true;
        }
        this.img.src = "/ChessWeb/Chessboard.png";
    }

    draw() {
        if (this.imgIsLoaded)//без этого может произойти попытка отрисовать еще не подгруженную картинку
            canvasManager.context.drawImage(this.img, this.position.x, this.position.y, this.size.width, this.size.height);
    }

    addChess(chess) {
        for (let e of this.dictOfChess) {
            if (e[1].id === chess.id) return;
        }
        this.dictOfChess.set(chess.id, chess);
    }
}

class Chess {
    constructor(board, tPosition, side, id) {
        this.id = id;

        this.board = board;
        this.size = { width: board.size.width / 8, height: board.size.height / 8 };

        //позиция фигуры внутри доски в клетках
        //(x,y) <==> (номер_клетки_по_горизонтали(слево направо),номер_клетки_по_вертикали(сверху вниз))
        this.boardPos = new Vector(tPosition.x, tPosition.y);

        //координаты в пикселях относительно левого верхнего угла холста
        //старые координаты, нужные для возврата передвигаемой текущим игроком фигуры
        //в исходную позицию при неправильном ходе(см. обработчик события movedown)
        this.oldCanvasPos = chessManager.getGlobalCanvasPositionOfCell(this, this.boardPos);
        //текущие координаты, нужные для вычисления позиции объектов фигур на холсте в данный момент времени
        this.canvasPosition = chessManager.getGlobalCanvasPositionOfCell(this, this.boardPos);

        //пропорция координат фигуры относительно координат стола,
        //нужная для обмена координатами между хостами игроков с разными размерами мониторов
        this.relativeFromBoardPosition = chessManager.getPositionRelativeToBoard(this.canvasPosition);

        this.side = side;//White/Black

        this.board.addChess(this);

        this.img = new Image();
        this.imgIsLoaded = false;
        this.img.onload = () => {
            this.imgIsLoaded = true;
        }
    }

    draw() {
        if (this.imgIsLoaded)//без этого может произойти попытка отрисовать еще не подгруженную картинку
            canvasManager.context.drawImage(this.img, this.canvasPosition.x, this.canvasPosition.y, this.size.width, this.size.height);
    }
}

class Pawn extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/PeshkaB.png";
        }
        else {
            this.img.src = "/ChessWeb/PeshkaW.png";
        }
    }

    //высчитывает можно ли переместить текущий объект фигуры в координаты указанные через смещение,
    //где смещение(shift_x, shift_y) - число клеток по оси X и Y относительно текущего положени объекта(this.boardPos)
    allowedShift(shift_x, shift_y) {
        if (shift_x === 0 && shift_y === 0)
            return true;

        //ходы в бок, либо вперед более чем на 1 клетку недопустимы
        if ((Math.abs(shift_x) > 0 && shift_y === 0) || Math.abs(shift_y) > 1)
            return false;
        if (this.side === "White") {
            if (shift_y < 0) return false;//белые расположены сверху и ходят только вниз
        }
        if (this.side === "Black") {
            if (shift_y > 0) return false;//черные расположены снизу и ходят только вверх
        }

        //двинуться по диагонали если есть фигура и она враг
        if (shift_x !== 0 && shift_y !== 0 &&
            Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x + shift_x) && (a.boardPos.y === this.boardPos.y + shift_y))) {
            if (Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x + shift_x) && (a.boardPos.y === this.boardPos.y + shift_y)).side !== this.side)
                return true;
            else
                return false;
        }
        //двинуться вперед если никого нет
        else if (shift_x === 0 && shift_y !== 0 &&
            !(Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x) && (a.boardPos.y === this.boardPos.y + shift_y)))) {
            return true;
        }
        return false;
    }
}

class Queen extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/QueenB.png";
        }
        else {
            this.img.src = "/ChessWeb/QueenW.png";
        }
    }

    allowedShift(shift_x, shift_y) {
        let tempChess;

        //можно ходить по диагонали и по прямой
        if ((Math.abs(shift_x) !== Math.abs(shift_y)) && (shift_x !== 0 && shift_y !== 0))
            return false;
        if (shift_x === 0 && shift_y === 0)
            return true;


        //анализ маршрута вниз от движемой фигуры
        let move_distance_down = 0;
        if (shift_y > 0 && shift_x === 0) {
            for (let i = this.boardPos.y; i < 8 && move_distance_down < Math.abs(shift_y); i++) {
                move_distance_down++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            //1)если на пути встретилась другая фигура и запрашиваемое смещение(shift_y) больше допустимого смещения(move_distance_down)(прыгнуть за другую фигуру) => false
            //2)если на пути встретилась другая фигура и з/смещение равно д/смещению(прыгнуть на фигуру или перед ней)
            // 2.1)на другую фигуру другого цвета => true
            // 2.2)на другую фигуру своего цвета => false
            // 2.3)на пустую клетку => true;
            //3)если на пути нет других фигур(з/смещение всегда равно д/смещению) => true;
            if (shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх от движемой фигуры
        let move_distance_up = 0;
        if (shift_y < 0 && shift_x === 0) {
            for (let i = this.boardPos.y; i > 0 && move_distance_up < Math.abs(shift_y); i--) {
                move_distance_up++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вправо от движемой фигуры
        let move_distance_right = 0;
        if (shift_x > 0 && shift_y == 0) {
            for (let i = this.boardPos.x; i < 8 && move_distance_right < Math.abs(shift_x); i++) {
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута влево от движемой фигуры
        let move_distance_left = 0;
        if (shift_x < 0 && shift_y == 0) {
            for (let i = this.boardPos.x; i > 0 && move_distance_left < Math.abs(shift_x); i--) {
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вниз вправо от движемой фигуры
        move_distance_right = 0;
        move_distance_down = 0;
        if (shift_x > 0 && shift_y > 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i < 8 && move_distance_right < Math.abs(shift_x) && j < 8 && move_distance_down < Math.abs(shift_y)) {
                i++; j++;
                move_distance_down++;
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right && shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вниз влево от движемой фигуры
        move_distance_left = 0;
        move_distance_down = 0;
        if (shift_x < 0 && shift_y > 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i > 0 && move_distance_left < Math.abs(shift_x) && j < 8 && move_distance_down < Math.abs(shift_y)) {
                i--; j++;
                move_distance_down++;
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left && shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх влево от движемой фигуры
        move_distance_left = 0;
        move_distance_up = 0;
        if (shift_x < 0 && shift_y < 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i > 0 && move_distance_left < Math.abs(shift_x) && j > 0 && move_distance_up < Math.abs(shift_y)) {
                i--; j--;
                move_distance_up++;
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left && Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх вправо от движемой фигуры
        move_distance_right = 0;
        move_distance_up = 0;
        if (shift_x > 0 && shift_y < 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i < 8 && move_distance_right < Math.abs(shift_x) && j > 0 && move_distance_up < Math.abs(shift_y)) {
                i++; j--;
                move_distance_up++;
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right && Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        return false;
    }
}

class King extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/KingB.png";
        }
        else {
            this.img.src = "/ChessWeb/KingW.png";
        }
    }

    allowedShift(shift_x, shift_y) {
        let tempChess;

        if (shift_x === 0 && shift_y === 0)
            return true;

        if (((shift_x <= 1 && shift_x >= -1) && (shift_y <= 1 && shift_y >= -1))) {
            tempChess = Array.from(this.board.dictOfChess.values())
                .find(a => (a.boardPos.x === this.boardPos.x + shift_x) && (a.boardPos.y === this.boardPos.y + shift_y));

            if (!tempChess)
                return true;
            else if (tempChess.side != this.side)
                return true;
        }

        return false;
    }
}

class Knight extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/KnightB.png";
        }
        else {
            this.img.src = "/ChessWeb/KnightW.png";
        }
    }

    allowedShift(shift_x, shift_y) {
        let tempChess;

        if (shift_x == 0 && shift_y == 0)
            return true;

        //если ход (2 вверх или вниз и 1 налево или направо) или (2 налево или направло и 1 вверх или вниз)
        if (((shift_x === 2 || shift_x === -2) && (shift_y === 1 || shift_y === -1)) || ((shift_x === 1 || shift_x === -1) && (shift_y === 2 || shift_y === -2))) {
            tempChess = Array.from(this.board.dictOfChess.values())
                .find(a => (a.boardPos.x === this.boardPos.x + shift_x) && (a.boardPos.y === this.boardPos.y + shift_y));

            if (!tempChess)
                return true;
            else if (tempChess.side !== this.side)
                return true;
        }

        return false;
    }
}

class Castle extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/CastleB.png";
        }
        else {
            this.img.src = "/ChessWeb/CastleW.png";
        }
    }

    allowedShift(shift_x, shift_y) {
        let tempChess;

        //можно ходить по прямой
        if (shift_x != 0 && shift_y != 0)
            return false;
        if (shift_x == 0 && shift_y == 0)
            return true;

        //анализ маршрута вниз от движемой фигуры
        let move_distance_down = 0;
        if (shift_y > 0 && shift_x === 0) {
            for (let i = this.boardPos.y; i < 8 && move_distance_down < Math.abs(shift_y); i++) {
                move_distance_down++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            if (shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх от движемой фигуры
        let move_distance_up = 0;
        if (shift_y < 0 && shift_x === 0) {
            for (let i = this.boardPos.y; i > 0 && move_distance_up < Math.abs(shift_y); i--) {
                move_distance_up++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вправо от движемой фигуры
        let move_distance_right = 0;
        if (shift_x > 0 && shift_y == 0) {
            for (let i = this.boardPos.x; i < 8 && move_distance_right < Math.abs(shift_x); i++) {
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута влево от движемой фигуры
        let move_distance_left = 0;
        if (shift_x < 0 && shift_y == 0) {
            for (let i = this.boardPos.x; i > 0 && move_distance_left < Math.abs(shift_x); i--) {
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values()).find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        return false;
    }
}

class Bishop extends Chess {
    constructor(board, tPosition, side, id) {
        super(board, tPosition, side, id);
        if (side === "Black") {
            this.img.src = "/ChessWeb/BishopB.png";
        }
        else {
            this.img.src = "/ChessWeb/BishopW.png";
        }
    }

    allowedShift(shift_x, shift_y) {
        let tempChess;

        //можно ходить только по диагонали
        if (Math.abs(shift_x) != Math.abs(shift_y))
            return false;
        if (shift_x == 0 && shift_y == 0)
            return true;

        //анализ маршрута вниз вправо от движемой фигуры
        let move_distance_right = 0;
        let move_distance_down = 0;
        if (shift_x > 0 && shift_y > 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i < 8 && move_distance_right < Math.abs(shift_x) && j < 8 && move_distance_down < Math.abs(shift_y)) {
                i++; j++;
                move_distance_down++;
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right && shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вниз влево от движемой фигуры
        let move_distance_left = 0;
        move_distance_down = 0;
        if (shift_x < 0 && shift_y > 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i > 0 && move_distance_left < Math.abs(shift_x) && j < 8 && move_distance_down < Math.abs(shift_y)) {
                i--; j++;
                move_distance_down++;
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y + move_distance_down));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left && shift_y <= move_distance_down) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх влево от движемой фигуры
        move_distance_left = 0;
        let move_distance_up = 0;
        if (shift_x < 0 && shift_y < 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i > 0 && move_distance_left < Math.abs(shift_x) && j > 0 && move_distance_up < Math.abs(shift_y)) {
                i--; j--;
                move_distance_up++;
                move_distance_left++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x - move_distance_left) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (Math.abs(shift_x) <= move_distance_left && Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        //анализ маршрута вверх вправо от движемой фигуры
        move_distance_right = 0;
        move_distance_up = 0;
        if (shift_x > 0 && shift_y < 0) {
            let i = this.boardPos.x;
            let j = this.boardPos.y;
            while (i < 8 && move_distance_right < Math.abs(shift_x) && j > 0 && move_distance_up < Math.abs(shift_y)) {
                i++; j--;
                move_distance_up++;
                move_distance_right++;
                tempChess = Array.from(this.board.dictOfChess.values())
                    .find(a => (a.boardPos.x === this.boardPos.x + move_distance_right) && (a.boardPos.y === this.boardPos.y - move_distance_up));
                if (tempChess) {
                    break;
                }
            }
            if (shift_x <= move_distance_right && Math.abs(shift_y) <= move_distance_up) {
                if (!tempChess) return true;
                if (tempChess.side !== this.side) return true;
            }
        }

        return false;
    }
}

class ChessManager {
    constructor() {
        this.isMoved = false;//ход

        //объект шахмотной доски
        this.board = new Board();
    }

    addChessToBoard() {
        //объекты шахмат
        //короли должны занимать 0 и 1 id, по которым будет вычисляться живы ли они
        new King(this.board, new Vector(3, 0), "White", 0);
        new King(this.board, new Vector(3, 7), "Black", 1);

        new Pawn(this.board, new Vector(0, 1), "White", 2);
        new Pawn(this.board, new Vector(1, 1), "White", 3);
        new Pawn(this.board, new Vector(2, 1), "White", 4);
        new Pawn(this.board, new Vector(3, 1), "White", 5);
        new Pawn(this.board, new Vector(4, 1), "White", 6);
        new Pawn(this.board, new Vector(5, 1), "White", 7);
        new Pawn(this.board, new Vector(6, 1), "White", 8);
        new Pawn(this.board, new Vector(7, 1), "White", 9);

        new Castle(this.board, new Vector(0, 0), "White", 10);
        new Knight(this.board, new Vector(1, 0), "White", 11);
        new Bishop(this.board, new Vector(2, 0), "White", 12);
        new Queen(this.board, new Vector(4, 0), "White", 13);
        new Bishop(this.board, new Vector(5, 0), "White", 14);
        new Knight(this.board, new Vector(6, 0), "White", 15);
        new Castle(this.board, new Vector(7, 0), "White", 16);

        new Pawn(this.board, new Vector(0, 6), "Black", 17);
        new Pawn(this.board, new Vector(1, 6), "Black", 18);
        new Pawn(this.board, new Vector(2, 6), "Black", 19);
        new Pawn(this.board, new Vector(3, 6), "Black", 20);
        new Pawn(this.board, new Vector(4, 6), "Black", 21);
        new Pawn(this.board, new Vector(5, 6), "Black", 22);
        new Pawn(this.board, new Vector(6, 6), "Black", 23);
        new Pawn(this.board, new Vector(7, 6), "Black", 24);

        new Castle(this.board, new Vector(0, 7), "Black", 25);
        new Knight(this.board, new Vector(1, 7), "Black", 26);
        new Bishop(this.board, new Vector(2, 7), "Black", 27);
        new Queen(this.board, new Vector(4, 7), "Black", 28);
        new Bishop(this.board, new Vector(5, 7), "Black", 29);
        new Knight(this.board, new Vector(6, 7), "Black", 30);
        new Castle(this.board, new Vector(7, 7), "Black", 31);
    }

    getGlobalCanvasPositionOfCell(chess, boardPosition) {
        let x = boardPosition.x * chess.size.width + this.board.position.x;
        let y = boardPosition.y * chess.size.height + this.board.position.y;
        return new Vector(x, y);
    }

    getLocalCellNumberUnderCursor(chess, cursorGlobalPos) {
        let x = Math.floor((cursorGlobalPos.x - this.board.position.x) / chess.size.width);
        let y = Math.floor((cursorGlobalPos.y - this.board.position.y) / chess.size.height);
        return new Vector(x, y);
    }

    //возвращает координаты шахматы относительно пространства внутри стола
    getPositionRelativeToBoard(canvasPosition) {
        let x = (canvasPosition.x - this.board.position.x) / this.board.size.width;
        let y = (canvasPosition.y - this.board.position.y) / this.board.size.height;
        return new Vector(x, y);
    }

    updateChess(boardState) {
        if (boardState === "") return;
        let chessStrings = boardState.split(";");
        chessStrings.splice(chessStrings.length - 1, 1);

        let dictIdChessRelCoord = new Map();//(id: {relX, relY})
        for (let e of chessStrings) {
            let IdXY = e.match(/[0-9]+\.*[0-9]*/gi);
            dictIdChessRelCoord.set(Number(IdXY[0]), {
                relX: Number(IdXY[1]),
                relY: Number(IdXY[2])
            });
        }

        if (dictIdChessRelCoord.size === 0) return;
        for (let pairIdChess of this.board.dictOfChess) {
            if (!dictIdChessRelCoord.has(pairIdChess[0])) {
                this.board.dictOfChess.delete(pairIdChess[0]);
            }
            else {
                pairIdChess[1].relativeFromBoardPosition.x = dictIdChessRelCoord.get(pairIdChess[0]).relX;
                pairIdChess[1].relativeFromBoardPosition.y = dictIdChessRelCoord.get(pairIdChess[0]).relY;

                pairIdChess[1].canvasPosition.x = this.board.position.x + pairIdChess[1].relativeFromBoardPosition.x * this.board.size.width;
                pairIdChess[1].canvasPosition.y = this.board.position.y + pairIdChess[1].relativeFromBoardPosition.y * this.board.size.height;

                pairIdChess[1].oldCanvasPos.x = pairIdChess[1].canvasPosition.x;
                pairIdChess[1].oldCanvasPos.y = pairIdChess[1].canvasPosition.y;

                pairIdChess[1].boardPos.x = Math.floor((pairIdChess[1].canvasPosition.x - this.board.position.x) / pairIdChess[1].size.width);
                pairIdChess[1].boardPos.y = Math.floor((pairIdChess[1].canvasPosition.y - this.board.position.y) / pairIdChess[1].size.height);
            }
        }
    }

    buildChessPositionsString() {
        let result = "";
        for (let pairIdChess of this.board.dictOfChess) {
            let relCoordinates = pairIdChess[1].relativeFromBoardPosition;
            result += "(" + pairIdChess[0] + "," + relCoordinates.x.toString() + "," + relCoordinates.y.toString() + ");";
        }
        return result;
    }
}