using MatrixSdk;

var buffer = ImageBuffer.Create();

var size = 11;

var turn = 'X';
var positionRow = 0;
var positionColumn = 0;

var white = new Color(0xFF, 0xFF, 0xFF);
var red = new Color(0xFF, 0x00, 0x00);
var blue = new Color(0x00, 0x00, 0xFF);

var blink = false;

(int X, int Y) GetCenter(int column, int row)
{
    var x = column switch
    {
        0 => 2,
        1 => 6,
        2 => 10,
    };

    var y = row switch
    {
        0 => 1,
        1 => 5,
        2 => 9,
    };

    return (x, y);
}

void DrawO(int column, int row)
{
    var (x, y) = GetCenter(column, row);
    buffer[x - 1, y] = blue;
    buffer[x, y - 1] = blue;
    buffer[x + 1, y] = blue;
    buffer[x, y + 1] = blue;
}

void DrawX(int column, int row)
{
    var (x, y) = GetCenter(column, row);
    buffer[x, y] = red;
    buffer[x - 1, y - 1] = red;
    buffer[x + 1, y + 1] = red;
    buffer[x - 1, y + 1] = red;
    buffer[x + 1, y - 1] = red;
}

var placements = new char[3, 3];

void Render(bool blink)
{
    buffer.Clear();

    buffer.Rows[3] = white;
    buffer.Rows[7] = white;

    buffer.Columns[4] = white;
    buffer.Columns[8] = white;

    buffer.Columns[0] = default;
    buffer.Columns[12] = default;

    for (var column = 0; column < 3; column++)
    {
        for (var row = 0; row < 3; row++)
        {
            var c = placements[column, row];

            if (c == 'X')
            {
                DrawX(column, row);
            }
            else if (c == 'O')
            {
                DrawO(column, row);
            }
        }
    }

    if (blink && placements[positionColumn, positionRow] == default)
    {
        if (turn == 'X')
        {
            DrawX(positionColumn, positionRow);
        }
        else
        {
            DrawO(positionColumn, positionRow);
        }
    }
}

void DrawLine(double angleInDegrees, int offsetX, int offsetY)
{
    var angleInRad = angleInDegrees * Math.PI / 180;
    var (sin, cos) = Math.SinCos(angleInRad);

    var length = 8;

    var x = (-cos * length) + offsetX;
    var y = (-sin * length) + offsetY + 1;

    for (var index = 0; index < length * 2; index++)
    {
        x += cos;
        y += sin;

        buffer[(int)Math.Round(x), (int)Math.Round(y)] = new Color(0xFF, 0xFF, 0);
    }
}

bool DetectWinner()
{
    // Check winner across column
    for (var column = 0; column < 3; column++)
    {
        if (placements[column, 0] is not (char)0 && placements[column, 0] == placements[column, 1] && placements[column, 1] == placements[column, 2])
        {
            var (offsetX, offsetY) = GetCenter(column, 0);
            DrawLine(90, offsetX, offsetY);
            return true;
        }
    }

    // Check winner across row
    for (var row = 0; row < 3; row++)
    {
        if (placements[0, row] is not (char)0 && placements[0, row] == placements[1, row] && placements[1, row] == placements[2, row])
        {
            var (offsetX, offsetY) = GetCenter(0, row);
            DrawLine(180, offsetX + 3, offsetY - 1);
            return true;
        }
    }

    // Check winner across diagonal
    if (placements[0, 0] is not (char)0 && placements[0, 0] == placements[1, 1] && placements[1, 1] == placements[2, 2])
    {
        DrawLine(45, size / 2, (size / 2) - 2);
        return true;
    }

    if (placements[0, 2] is not (char)0 && placements[0, 2] == placements[1, 1] && placements[1, 1] == placements[2, 0])
    {
        DrawLine(-45, size / 2, size / 2);
        return true;
    }

    // Check no winner
    for (var column = 0; column < 3; column++)
    {
        for (var row = 0; row < 3; row++)
        {
            if (placements[column, row] is (char)0)
            {
                return false;
            }
        }
    }

    // no player won the game
    return true;
}

void Control()
{
    while (true)
    {
        SpinWait.SpinUntil(() => Console.KeyAvailable);

        var key = Console.ReadKey(true);

        if (key.Key is ConsoleKey.W or ConsoleKey.UpArrow)
        {
            positionRow = Math.Max(0, positionRow - 1);
        }
        else if (key.Key is ConsoleKey.A or ConsoleKey.LeftArrow)
        {
            positionColumn = Math.Max(0, positionColumn - 1);
        }
        else if (key.Key is ConsoleKey.S or ConsoleKey.DownArrow)
        {
            positionRow = Math.Min(2, positionRow + 1);
        }
        else if (key.Key is ConsoleKey.D or ConsoleKey.RightArrow)
        {
            positionColumn = Math.Min(2, positionColumn + 1);
        }
        else if (key.Key is ConsoleKey.Enter)
        {
            if (placements[positionColumn, positionRow] is (char)0)
            {
                placements[positionColumn, positionRow] = turn;
                turn = turn == 'X' ? 'O' : 'X';
            }
        }

        Render(true);
        buffer.Commit();
    }
}

new Thread(Control).Start();

while (true)
{
    Array.Clear(placements);

    while (true)
    {
        Render(blink = !blink);

        if (DetectWinner())
        {
            buffer.Commit();
            Thread.Sleep(3000);
            break;
        }

        buffer.Commit();
        Thread.Sleep(500);
    }
}
