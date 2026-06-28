using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(menuName = "Grass objects/Grass  chunk SO data")]
public class GrassChunkContainer : ScriptableObject
{
    public bool _isDoubleTexture;
    public int VerticalChunkCnt;
    public int HorizontalChunkCnt;
    public List<GrassChunk> GrassChunks = new();
}