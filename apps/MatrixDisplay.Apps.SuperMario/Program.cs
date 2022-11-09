using MatrixDisplay;

var buffer = PixelBuffer.Instance;

var positionX = 2.0D;
var positionY = 6.0D;

var playerHeadColor = new Color(0x9E, 0x1C, 0x1F);
var playerBodyColor = new Color(0x2B, 0x5C, 0xA3);

var worldCache = new Dictionary<World, WorldCacheEntry>();

void ChangeWorld(World world)
{
    if (GameLogic.CurrentWorld is not null && worldCache.TryGetValue(GameLogic.CurrentWorld, out var entry))
    {
        entry.PositionX = positionX;
        entry.PositionY = positionY;
    }

    GameLogic.CurrentWorld = world;

    if (!worldCache.TryGetValue(world, out entry))
    {
        buffer.Picture(world.Name);
        worldCache[world] = entry = new WorldCacheEntry(buffer.Save(), null, null);
    }
    else
    {
        buffer.Restore(entry.Buffer);
    }

    positionX = entry.PositionX ?? world.StartPositionX;
    positionY = entry.PositionY ?? world.StartPositionY;

    Render();
}

GameLogic.ChangeWorldAction = ChangeWorld;

ChangeWorld(Worlds.MainWorld);

var airMaterial = new Air();

var velocityX = 0.0D;
var velocityY = 0.0D;

var materials = new Dictionary<Color, Material>
{
    [Tube.Color] = new Tube(),
    [TubeHead.Color] = new TubeHead(),
    [Air.Color] = new Air(),
    [Block.Color] = new Block(),
    [LuckyBlock.Color] = new LuckyBlock(),
};

var canJump = false;

void Render()
{
    void DrawPlayer()
    {
        buffer[(int)positionX, (int)positionY - 1] = playerHeadColor;
        buffer[(int)positionX, (int)positionY] = playerBodyColor;
    }

    buffer.Restore(worldCache[GameLogic.CurrentWorld].Buffer);
    DrawPlayer();

    buffer.Commit();
}

buffer.AutoCommit = false;

void Control()
{
    while (true)
    {
        SpinWait.SpinUntil(() => Console.KeyAvailable);

        var key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.A)
        {
            velocityX = -1.0D;
        }
        else if (key.Key == ConsoleKey.D)
        {
            velocityX = 1.0D;
        }
        else if (key.Key == ConsoleKey.Spacebar)
        {
            if (canJump)
            {
                velocityY = 1.5D;
            }
        }
    }
}

new Thread(Control).Start();

MaterialPhysicsContext GetPhysics(double positionX, double positionY)
{
    var material = materials.GetValueOrDefault(buffer[(int)positionX, (int)positionY], airMaterial);
    return new MaterialPhysicsContext((int)positionX, (int)positionY, material, worldCache[GameLogic.CurrentWorld].Buffer);
}

while (true)
{
    Render();

    var originalPositionX = positionX;
    var originalPositionY = positionY;

    var deltaVelocityX = Math.Sign(velocityX) * Math.Min(1.0D, Math.Abs(velocityX));
    var deltaVelocityY = Math.Sign(velocityY) * Math.Min(1.0D, Math.Abs(velocityY));

    var targetX = positionX + deltaVelocityX;
    var targetY = positionY - deltaVelocityY;

    velocityX -= deltaVelocityX;
    velocityY -= deltaVelocityY;

    if (targetX < 0.0D)
    {
        if (GameLogic.CurrentWorld.LeftWorld is not null)
        {
            GameLogic.ChangeWorld(GameLogic.CurrentWorld.LeftWorld());
        }
        else
        {
            targetX = 0.0D;
        }
    }
    else if (targetX > buffer.Width - 1)
    {
        if (GameLogic.CurrentWorld.RightWorld is not null)
        {
            GameLogic.ChangeWorld(GameLogic.CurrentWorld.RightWorld());
        }
        else
        {
            targetX = buffer.Width - 1;
        }
    }

    if (targetY < 0.0D)
    {
        targetY = 0.0D;
    }
    else if (targetY > buffer.Height - 1)
    {
        targetY = buffer.Height - 1;
    }

    var belowMaterial = GetPhysics(positionX, positionY + 1.0D);
    var aboveMaterial = GetPhysics(positionX, positionY - 2.0D);
    var targetMaterial = GetPhysics(targetX, targetY);

    if (!belowMaterial.Material.IsSolid)
    {
        targetY += 0.3D;
        canJump = false;
    }
    else
    {
        canJump = true;
    }

    if (aboveMaterial.Material.IsSolid)
    {
        aboveMaterial.Material.HandleJump(aboveMaterial);
    }

    // Only allow movement if the target destination is air
    if (!targetMaterial.Material.IsSolid)
    {
        // Move the player, if the position was not mutated by the material
        if (positionX == originalPositionX && positionY == originalPositionY)
        {
            positionX = targetX;
            positionY = targetY;
        }
    }
    else
    {
        targetMaterial.Material.HandleJump(targetMaterial);
    }

    Thread.Sleep(10);
}
