using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Grass objects/Grass SO data")]
public class GrassDataContainer : ScriptableObject
{
    [SerializeField] public bool _isDoubleTexture;
    [SerializeField] public List<GrassSample> GrassTransforms = new();
}