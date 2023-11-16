using MatrixSdk;

var buffer = ImageBuffer.Create();

var colors = new Color[]
{
    new Color(0x01, 0xF0, 0x00),
    new Color(0x9F, 0x01, 0xF0),
    new Color(0xF0, 0x01, 0x00),
    new Color(0xF0, 0xF0, 0x01),
    new Color(0xEF, 0xA0, 0x00),
    new Color(0x00, 0x01, 0xF0),
    new Color(0x01, 0xF0, 0xF1),
};

Block NewBlock()
{
    return new Block(Shape.All[Random.Shared.Next(Shape.All.Length)], colors[Random.Shared.Next(colors.Length)]) { X = 3, Y = 0, };
}

var blocks = new List<Block>
{
    NewBlock() with { Y = 10, X = 6 }
};

var currentBlock = NewBlock();

bool CheckForCompleted()
{
    var list = new uint[buffer.Height];

    for (var y = 0; y < buffer.Height; y++)
    {
        list[y] = 0b01111111110;
    }

    foreach (var block in blocks)
    {
        foreach (var (pointX, pointY) in block.Points)
        {
            if (pointY + block.Y < buffer.Height)
            {
                list[pointY + block.Y] &= ~(1U << (pointX + block.X));
            }
        }
    }

    var destroyed = false;
    for (var y = 0; y < buffer.Height; y++)
    {
        if (list[y] is 0)
        {
            destroyed = true;

            foreach (var block in blocks)
            {
                block.DestroyLine(y);
            }
        }
    }

    return destroyed;
}

bool IsValidPosition(Block block)
{
    foreach (var point in block.Points)
    {
        var x = point.X + block.X;
        var y = point.Y + block.Y;

        // y < 0 is valid, as it is moving from the top
        if (y >= buffer.Height || x < 1 || x >= buffer.Width - 1)
        {
            return false;
        }
    }

    foreach (var otherBlock in blocks)
    {
        if (otherBlock.IsColliding(block))
        {
            return false;
        }
    }

    return true;
}

void Render()
{

    buffer.Clear();

    buffer.Columns[0] = new Color(0xFF, 0xFF, 0xFF);
    buffer.Columns[12] = new Color(0xFF, 0xFF, 0xFF);

    foreach (var block in blocks)
    {
        block.Render(buffer);
    }

    currentBlock.Render(buffer);

    buffer.Commit();
}

void Control()
{
    while (true)
    {
        SpinWait.SpinUntil(() => Console.KeyAvailable);

        var key = Console.ReadKey(true);

        var previousX = currentBlock.X;
        var previousY = currentBlock.Y;
        var rotation = 0;

        if (key.Key == ConsoleKey.A)
        {
            currentBlock.X -= 1;
        }
        else if (key.Key == ConsoleKey.D)
        {
            currentBlock.X += 1;
        }
        else if (key.Key == ConsoleKey.Spacebar)
        {
            currentBlock.Y += 1;
        }
        else if (key.Key == ConsoleKey.W)
        {
            currentBlock.Rotate(90);
            rotation = 90;
        }
        else if (key.Key == ConsoleKey.S)
        {
            currentBlock.Rotate(-90);
            rotation = -90;
        }

        if (!IsValidPosition(currentBlock))
        {
            currentBlock.X = previousX;
            currentBlock.Y = previousY;
            currentBlock.Rotate(-rotation);
        }

        Render();
    }
}

new Thread(Control).Start();

buffer.Clear();

while (true)
{

    var previousX = currentBlock.X;
    var previousY = currentBlock.Y;

    currentBlock.Y++; // gravity

    if (!IsValidPosition(currentBlock))
    {
        currentBlock.X = previousX;
        currentBlock.Y = previousY;
        blocks.Add(currentBlock);
        currentBlock.Render(buffer);
        currentBlock = NewBlock();
    }
    else
    {
        currentBlock.Render(buffer);
    }

    SpinWait.SpinUntil(() => !CheckForCompleted());

    Render();

    Thread.Sleep(500);
}
