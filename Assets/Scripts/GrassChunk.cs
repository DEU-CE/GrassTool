using UnityEngine;

[System.Serializable]
public class GrassChunk 
{
    public Vector3 ChunkCenterPos;
    public Vector3 ChunkSize;

    public int ChunkID;
    public int GrassSamplesCount;

    public Vector3[] GrassPoses;
    public Vector3[] GrassScales;
    public Quaternion[] GrassRotations;

    public Color[] LowColors;
    public Color[] HighColors;

    public int[] IDs;

    public float MinY;
    public float MaxY;
}