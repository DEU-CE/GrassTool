using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct MeshProperties
{
    public Matrix4x4 mat;
    public Vector4 color;

    public static int Size()
    {
        return
            sizeof(float) * 4 * 4 + // matrix;
            sizeof(float) * 4;      // color;
    }
}