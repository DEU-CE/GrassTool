using UnityEditor;
using UnityEngine;

public partial class GrassEditor
{
    void DrawBounds(SceneView sceneView, int horizChunkCnt, int vertChunkCnt)
    {
        Vector3 lowLeftCorner = new Vector3(0, 0, 0);
        lowLeftCorner.x = _genBoundLeftDownCorner.x;
        lowLeftCorner.z = _genBoundLeftDownCorner.z;

        Vector3 lowRightCorner = new Vector3(0, 0, 0);
        lowRightCorner.x = _genBoundRightUpCorner.x;
        lowRightCorner.z = _genBoundLeftDownCorner.z;

        Vector3 upLeftCorner = new Vector3(0, 0, 0);
        upLeftCorner.x = _genBoundLeftDownCorner.x;
        upLeftCorner.z = _genBoundRightUpCorner.z;

        Vector3 upRightCorner = new Vector3(0, 0, 0);
        upRightCorner.x = _genBoundRightUpCorner.x;
        upRightCorner.z = _genBoundRightUpCorner.z;

        Handles.DrawLine(lowLeftCorner, lowRightCorner);
        Handles.DrawLine(lowLeftCorner, upLeftCorner);
        Handles.DrawLine(upLeftCorner, upRightCorner);
        Handles.DrawLine(upRightCorner, lowRightCorner);

        Handles.color = Color.black;
        Handles.DrawWireCube(lowLeftCorner, Vector3.one);
        Handles.color = Color.cyan;
        Handles.DrawWireCube(upRightCorner, Vector3.one);
        Handles.color = Color.white;

        if (vertChunkCnt > 0 && horizChunkCnt > 0)
        {
            float verticalStep = Mathf.Abs(_genBoundRightUpCorner.x - _genBoundLeftDownCorner.x) / (float)vertChunkCnt;
            float horizontalStep = Mathf.Abs(_genBoundRightUpCorner.z - _genBoundLeftDownCorner.z) / (float)horizChunkCnt;
            Handles.color = Color.yellow;
            for (int i = 1;i<vertChunkCnt; i++)
            {
                Handles.DrawLine( 
                    new Vector3(_genBoundLeftDownCorner.x + verticalStep*i, 0f, _genBoundLeftDownCorner.z),
                    new Vector3(_genBoundLeftDownCorner.x + verticalStep*i, 0f, _genBoundRightUpCorner.z)
                    ); 
            }

            for (int i = 1; i < horizChunkCnt; i++)
            {
                Handles.DrawLine(
                    new Vector3(_genBoundLeftDownCorner.x, 0f, _genBoundLeftDownCorner.z + horizontalStep * i),
                    new Vector3(_genBoundRightUpCorner.x, 0f, _genBoundLeftDownCorner.z + horizontalStep * i)
                    );
            }
        }
        Handles.color = Color.white;
    }
    
    void DrawHandlesRect(Vector3 centerPos, Vector3 extents, float upVal)
    {
        Handles.DrawLine(
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z-extents.z),
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z-extents.z)
        );
            
        Handles.DrawLine(
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z-extents.z),
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z+extents.z)
        );
            
        Handles.DrawLine(
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z+extents.z),
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z+extents.z)
        );
            
        Handles.DrawLine(
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z-extents.z),
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z+extents.z)
        );
    }
    
    void DrawHandlesVertLines(Vector3 centerPos, Vector3 extents, float downVal, float upVal)
    {
        Handles.DrawLine(
            new Vector3(centerPos.x-extents.x, downVal, centerPos.z-extents.z),
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z-extents.z)
        );
        
        Handles.DrawLine(
            new Vector3(centerPos.x-extents.x, downVal, centerPos.z+extents.z),
            new Vector3(centerPos.x-extents.x, upVal, centerPos.z+extents.z)
        );
        
        Handles.DrawLine(
            new Vector3(centerPos.x+extents.x, downVal, centerPos.z+extents.z),
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z+extents.z)
        );
        
        Handles.DrawLine(
            new Vector3(centerPos.x+extents.x, downVal, centerPos.z-extents.z),
            new Vector3(centerPos.x+extents.x, upVal, centerPos.z-extents.z)
        );
    }
    
    void DrawChunksData(SceneView sceneView)
    {
        if(ChunksContainer.GrassChunks.Count==0 || ChunksContainer.GrassChunks == null)
        {
            return;
        }
        foreach (GrassChunk chunk in ChunksContainer.GrassChunks)
        {
            //Handles.color = Color.green;
            //Handles.DrawWireCube(chunk.ChunkCenterPos, Vector3.one * _chunkCenterPointSize);        
            //Handles.Label(chunk.ChunkCenterPos + Vector3.up * _chunkSamplesCountLabelOffset, chunk.GrassSamplesCount.ToString());
            
            Handles.color = Color.blue;
            
            DrawHandlesRect(chunk.ChunkCenterPos, chunk.ChunkSize, chunk.MinY);
            DrawHandlesRect(chunk.ChunkCenterPos, chunk.ChunkSize, chunk.MaxY);
            DrawHandlesVertLines(chunk.ChunkCenterPos, chunk.ChunkSize, chunk.MinY, chunk.MaxY);
        }
        Handles.color = Color.white;
    }
}