using MatrixSdk;

var buffer = ImageBuffer.Create();

for (int index = 0; index < buffer.Width; index++)
{
    for (int j = 0; j < index; j++)
    {
        buffer[j, 0] = Color.Red;
    }

    buffer.Commit();
    Thread.Sleep(100);
}

buffer.Commit();
