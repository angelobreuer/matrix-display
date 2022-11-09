sealed record class World(
    string Name,
    int StartPositionX,
    int StartPositionY,
    Func<World>? RightWorld = null,
    Func<World>? LeftWorld = null);
