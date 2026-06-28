using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassBaker
{
   private GrassDataContainer _grassDataContainer;
    private int _verticalChunkCount;
    private int _horizontalChunkCount;

    private Vector3 _leftCorner, _rightCorner;

    private List<GrassChunk> _grassChunks;
    public GrassBaker(GrassDataContainer grassDataContainer)
    {
        _grassDataContainer = grassDataContainer;
        _grassChunks = new List<GrassChunk>();
    }

    public void SetChunksDimensions(int vert, int horiz)
    {
        _verticalChunkCount = vert;
        _horizontalChunkCount = horiz;
        _grassChunks.Clear();
        _grassChunks = new List<GrassChunk>();
    }
    
    public List<GrassChunk> BakeGrassIntoChunks()
    {
        float verticalSize = Mathf.Abs(_rightCorner.x - _leftCorner.x) / (float)_verticalChunkCount;
        float horizontalSize = Mathf.Abs(_rightCorner.z - _leftCorner.z) / (float)_horizontalChunkCount;
        Vector3 chunkSize = new Vector3(verticalSize, 1f, horizontalSize);
        List<Vector3> poses, scales;
        List<Quaternion> rotations;
        List<Color> lowColors, highColors;
        List<int> ids;
        GrassChunk tempChunk;
        int curChunkID = 0;
        for(int i = 0; i < _verticalChunkCount;i++)
        {
            for (int j=0; j< _horizontalChunkCount;j++)
            {
                poses = new();
                scales = new();
                rotations = new();
                lowColors = new();
                highColors = new();
                ids = new ();
                tempChunk = new GrassChunk();
                tempChunk.ChunkSize = new Vector3(verticalSize*0.5f, 0.5f, horizontalSize*0.5f);    
                // TODO: check Y-size of chunk size. it should be calculated, not hard-coded 0.5f
                
                Vector3 chunkCenterPos = new Vector3(_leftCorner.x + (i + 0.5f) * chunkSize.x,
                    0f,
                    _leftCorner.z + (j+0.5f)*chunkSize.z);

                float minHeight=100000f, maxHeight=-100000f;
                tempChunk.ChunkCenterPos = chunkCenterPos;
                foreach (GrassSample grass in _grassDataContainer.GrassTransforms)
                {                  
                    if(grass.Position.x > tempChunk.ChunkCenterPos.x-chunkSize.x*0.5f && 
                        grass.Position.x < tempChunk.ChunkCenterPos.x + chunkSize.x * 0.5f &&
                        grass.Position.z > tempChunk.ChunkCenterPos.z - chunkSize.z * 0.5f &&
                        grass.Position.z < tempChunk.ChunkCenterPos.z + chunkSize.z * 0.5f &&
                        grass.IsBaked != true)
                    {
                        if (grass.Position.y-grass.Scale.y*0.5f < minHeight)
                        {
                            minHeight =grass.Position.y-grass.Scale.y*0.5f;
                        }
                        if (grass.Position.y+grass.Scale.y*0.5f > maxHeight)
                        {
                            maxHeight =grass.Position.y+grass.Scale.y*0.5f;
                        }
                        
                        poses.Add(grass.Position);
                        scales.Add(grass.Scale);
                        rotations.Add(grass.Rotation);
                        lowColors.Add(grass.LowColor);
                        highColors.Add(grass.HighColor);
                        ids.Add(grass.AtlasID);
                        grass.IsBaked = true;
                    }
                }
                tempChunk.GrassSamplesCount = poses.Count;
                if(poses.Count > 0)
                {
                    //Debug.LogWarning("chunk [" + i + "][" + j + "] grass count is " + _poses.Count);
                    tempChunk.GrassPoses = poses.ToArray();
                    tempChunk.GrassScales = scales.ToArray();
                    tempChunk.GrassRotations = rotations.ToArray();
                    tempChunk.LowColors = lowColors.ToArray();
                    tempChunk.HighColors = highColors.ToArray();
                    tempChunk.ChunkID = curChunkID;
                    tempChunk.ChunkSize.y = Mathf.Abs(maxHeight-minHeight)*0.5f;
                    tempChunk.MinY = minHeight;
                    tempChunk.MaxY = maxHeight;
                    tempChunk.IDs = ids.ToArray();
                    _grassChunks.Add(tempChunk);
                    curChunkID++;
                }               
            }
        }
        return _grassChunks;
    }

    public List<Vector3> GetGeneralBounds()
    {
        List<Vector3> bounds = new List<Vector3>();
        _leftCorner = new Vector3(0, 0, 0);
        _rightCorner = new Vector3(1, 0, 1);

        foreach(GrassSample grassSample in _grassDataContainer.GrassTransforms)
        {
            if(grassSample.Position.x - grassSample.Scale.x*0.5f < _leftCorner.x)
            {
                _leftCorner.x = grassSample.Position.x - grassSample.Scale.x * 0.5f;
            }
            if (grassSample.Position.z - grassSample.Scale.z * 0.5f < _leftCorner.z)
            {
                _leftCorner.z = grassSample.Position.z - grassSample.Scale.z * 0.5f;
            }

            if (grassSample.Position.x + grassSample.Scale.x * 0.5f > _rightCorner.x)
            {
                _rightCorner.x = grassSample.Position.x + grassSample.Scale.x * 0.5f;
            }
            if (grassSample.Position.z + grassSample.Scale.z * 0.5f > _rightCorner.z)
            {
                _rightCorner.z = grassSample.Position.z + grassSample.Scale.z * 0.5f;
            }
        }

        bounds.Add(_leftCorner);
        bounds.Add(_rightCorner);
    
        return bounds;
    }
}
