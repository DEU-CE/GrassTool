using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoMeshDrawer : MonoBehaviour
{
    private Mesh _gizmoMesh;
    private Color _gizmoColor;
    private Vector3 _gizmoScale;

    public void SetGizmoMesh(Mesh msh) => _gizmoMesh = msh;
    public void SetGizmoScale(Vector3 scale) => _gizmoScale = scale;
    public void SetGizmoColor(Color clr) => _gizmoColor = clr;

    public void DrawGizmoMesh(Transform trns)
    {
        Gizmos.matrix = Matrix4x4.TRS(trns.position, trns.rotation, _gizmoScale);

        Gizmos.color = _gizmoColor;
        if (_gizmoMesh != null)
        {
            Gizmos.DrawMesh(_gizmoMesh);
        }
    }      
}