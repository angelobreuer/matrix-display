static class GameLogic
{
    public static Action<World> ChangeWorldAction;

    public static World CurrentWorld { get; set; }
    public static void ChangeWorld(World world) => ChangeWorldAction(world);
}
