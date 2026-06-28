using UnityEngine;

public struct GrassChunkCs
{
    public Vector3 ChunkCenterPos;
    public Vector3 ChunkSize;
    
    public int GrassSamplesCount;

    public float MinY;
    public float MaxY;
    
    public static int Size()
    {
        return sizeof(float) * 6 // ChunkCenterPos + ChunkCenterPos
               + sizeof(int) // GrassSamplesCount
               + sizeof(float) * 2; // MinY + MaxY
    }
}