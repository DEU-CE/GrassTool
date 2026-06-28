using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GrassDrawer))]
public class GrassDrawerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GrassDrawer grassDrawer = (GrassDrawer)target;
        if (GUILayout.Button("Update idle wind params"))
        {
            grassDrawer.UpdateWindParams();
        }
    }
}