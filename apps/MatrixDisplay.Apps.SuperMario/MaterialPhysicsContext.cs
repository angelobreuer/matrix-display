using MatrixDisplay;

sealed record class MaterialPhysicsContext(int PositionX, int PositionY, Material Material, Memory<Color> Buffer);
