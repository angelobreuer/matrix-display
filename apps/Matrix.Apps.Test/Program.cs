using MatrixSdk;

var buffer = ImageBuffer.Create();

void TestColor(Color color)
{
    buffer.Colors.Fill(color);
    buffer.Commit();

    Console.WriteLine($"Testing color {color}");
    Console.WriteLine("Press any key to continue...");
    Console.ReadKey();
}

TestColor(new Color(255, 0, 0));
TestColor(new Color(0, 255, 0));
TestColor(new Color(0, 0, 255));
TestColor(new Color(255, 255, 255));
