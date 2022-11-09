static class Worlds
{
    public static World MainWorld => new World("World1", 2, 6);
    public static World World2 => new World("World2", 2, 6, RightWorld: () => World3);
    public static World World3 => new World("World3", 2, 6, LeftWorld: () => World2, RightWorld: () => World4);
    public static World World4 => new World("World4", 2, 6, LeftWorld: () => World3, RightWorld: () => World5);
    public static World World5 => new World("World5", 2, 6, LeftWorld: () => World4, RightWorld: () => World6);
    public static World World6 => new World("World6", 2, 6, LeftWorld: () => World5, RightWorld: () => World7);
    public static World World7 => new World("World7", 2, 6, LeftWorld: () => World6);
    public static World World8 => new World("World8", 2, 6, RightWorld: () => World9);
    public static World World9 => new World("World9", 2, 6, LeftWorld: () => World8, RightWorld: () => World10);
    public static World World10 => new World("World10", 2, 6, LeftWorld: () => World9, RightWorld: () => World11);
    public static World World11 => new World("World11", 2, 6, LeftWorld: () => World10, RightWorld: () => World12);
    public static World World12 => new World("World12", 2, 6, LeftWorld: () => World11, RightWorld: () => World13);
    public static World World13 => new World("World13", 2, 6, LeftWorld: () => World12, RightWorld: () => World14);
    public static World World14 => new World("World14", 2, 6, LeftWorld: () => World13, RightWorld: () => World15);
    public static World World15 => new World("World15", 2, 6, LeftWorld: () => World14);
    public static World World16 => new World("World16", 2, 6);
}
