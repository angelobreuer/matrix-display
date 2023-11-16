using MatrixSdk;

var buffer = ImageBuffer.Create();

var positionX = 0D;
var positionY = 0D;
var length = 4;
var speed = 1D;

var movementX = 1D;
var movementY = 0D;

var queue = new Queue<Point>();
var fruits = new List<Fruit>();

void Control()
{
    while (true)
    {
        var key = Console.ReadKey(true);

        if (movementX is 0)
        {
            if (key.Key is ConsoleKey.D or ConsoleKey.RightArrow)
            {
                if (movementX is -1)
                {
                    continue; // Don't allow 180 degree turns
                }

                movementX = 1;
                movementY = 0;
            }
            else if (key.Key is ConsoleKey.A or ConsoleKey.LeftArrow)
            {
                if (movementX is 1)
                {
                    continue; // Don't allow 180 degree turns
                }

                movementX = -1;
                movementY = 0;
            }
        }
        else if (movementY is 0)
        {
            if (key.Key is ConsoleKey.S or ConsoleKey.DownArrow)
            {
                if (movementY is 1)
                {
                    continue; // Don't allow 180 degree turns
                }

                movementX = 0;
                movementY = 1;
            }
            else if (key.Key is ConsoleKey.W or ConsoleKey.UpArrow)
            {
                if (movementY is 1)
                {
                    continue; // Don't allow 180 degree turns
                }

                movementX = 0;
                movementY = -1;
            }
        }
    }
}

void SpawnFruit()
{
    var color = Random.Shared.Next(6) switch
    {
        0 or 1 => Color.Red,
        2 => Color.Green,
        3 => Color.Blue,
        4 => new Color(0xFF, 0xFF, 0x00),
        5 => new Color(0xFF, 0x00, 0xFF),
    };

    fruits.Add(new Fruit(
        Random.Shared.Next(0, buffer.Width),
        Random.Shared.Next(0, buffer.Height),
        color));
}

bool Render(int tick)
{
    buffer.Clear();

    var index = 0.0D;
    foreach (var point in queue)
    {
        buffer[point.X, point.Y] = Color.FromHsv((queue.Count - index++) * 8D, 1D, 1D);
    }

    foreach (var fruit in fruits)
    {
        buffer[fruit.X, fruit.Y] = fruit.Color;
    }

    while (queue.Count > length)
    {
        queue.Dequeue();
    }

    if (tick % 4 is 0)
    {
        if (positionX >= buffer.Width || positionX < 0 || positionY >= buffer.Height || positionY < 0)
        {
            return false; // death, hit wall
        }

        positionX += movementX * speed;
        positionY += movementY * speed;

        foreach (var point in queue)
        {
            if ((int)positionX == point.X && (int)positionY == point.Y)
            {
                return false; // death, hit snake
            }
        }

        // Normalize speed
        speed = Math.Max(1, speed - 0.1D);

        queue.Enqueue(new Point((int)positionX, (int)positionY));

        var fruitsToSpawn = 0;

        fruits.RemoveAll(fruit =>
        {
            if (fruit.X == (int)positionX && fruit.Y == (int)positionY)
            {
                if (fruit.Color == Color.Green)
                {
                    speed += 0.7D;
                }
                else if (fruit.Color == new Color(0xFF, 0xFF, 0x00))
                {
                    fruitsToSpawn++;
                }
                else if (fruit.Color == new Color(0xFF, 0x00, 0xFF))
                {
                    (movementY, movementX) = (movementX, movementY);
                }
                else
                {
                    var points = fruit.Color == Color.Blue ? 2 : 1;
                    length += points;
                }

                fruitsToSpawn++;
                return true;
            }

            return false;
        });

        while (fruitsToSpawn-- > 0)
        {
            SpawnFruit();
        }
    }

    buffer.Commit();

    return true;
}

new Thread(Control).Start();

while (true)
{
    var tick = 0;

    positionX = 0;
    positionY = 0;
    length = 3;

    movementX = 1;
    movementY = 0;

    speed = 1.0D;

    queue.Clear();
    fruits.Clear();

    SpawnFruit();

    while (Render(tick++))
    {
        Thread.Sleep(100);
    }

    // Draw snake white to indicate death
    foreach (var point in queue)
    {
        buffer[point.X, point.Y] = new Color(0xFF, 0xFF, 0xFF);
    }

    buffer.Commit();
    Thread.Sleep(3000);
    buffer.Clear();
}


readonly record struct Fruit(int X, int Y, Color Color);

readonly record struct Point(int X, int Y);