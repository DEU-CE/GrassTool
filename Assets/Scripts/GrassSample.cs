using UnityEngine;
using System;

[Serializable]
public class GrassSample
{
    [SerializeField]public Vector3 Position; // position of pivot
    [SerializeField]public Quaternion Rotation;
    [SerializeField]public Vector3 Scale;

    [SerializeField] public Color LowColor;
    [SerializeField] public Color HighColor;

    [SerializeField]public bool IsBaked;
    [SerializeField]public int ChunkID;
    [SerializeField] public int AtlasID;
}