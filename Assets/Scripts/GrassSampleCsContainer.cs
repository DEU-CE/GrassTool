using UnityEngine;

public struct GrassSampleCsContainer
{
    public Matrix4x4 Matrix;
    public Color LowColor;
    public Color HighColor;
    public int ChunkID;
    public int AtlasID;

    public static int Size()
    {
        return sizeof(float) * 4 * 4 + // Matrix
               sizeof(float) * 4 * 2 + // colors
               sizeof(int) + // ChunkID
               sizeof(int); // AtlasID
    }
}