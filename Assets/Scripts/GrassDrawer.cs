using System.Collections.Generic;
using UnityEngine;

public class GrassDrawer : MonoBehaviour
{
    [SerializeField] private Camera _cam;
    [SerializeField] private GrassChunkContainer _grassChunkContainer;
    [SerializeField] private ComputeShader _cullingComputeShader;
    [SerializeField] private Material _grassMat;
    [SerializeField] private Mesh _mesh;
    
    [Header("Lighting")] 
    [SerializeField] private Color _ambientColor = Color.white;
    [SerializeField] private Light _directLight;
    
    [Header("Idle wind settings")]
    [SerializeField] private float _windFrequency;
    [SerializeField] private float _windAmplitude;
    
    private ComputeBuffer _argsBuffer;
    private ComputeBuffer _grassSampleCsContainers;
    private ComputeBuffer _visibleGrassSamples;
    private ComputeBuffer _computeBufferCamPlanes;
    private ComputeBuffer _computeBufferChunks;
    private int _threadsCount;
    private Plane[] _cameraPlanes;
    private Vector4[] _camBufferPlanesArray;
    private bool _isBuffersInited;
    private List<GrassChunkCs> _grassChunksCsList;
    
    private int _windFreqShaderID = Shader.PropertyToID("_GrassWindFreq");
    private int _windAmplitudeShaderID = Shader.PropertyToID("_GrassWindAmp");
    private int _ambientColorShaderID = Shader.PropertyToID("_ambientColor");
    private int _dirLightDirectionShaderID = Shader.PropertyToID("_DirLightDirection");
    private int _dirLightColorShaderID = Shader.PropertyToID("_DirLightColor");

    private Bounds _bounds;

    public void UpdateWindParams()
    {
        _grassMat.SetFloat(_windFreqShaderID, _windFrequency);
        _grassMat.SetFloat(_windAmplitudeShaderID, _windAmplitude);
    }

    private void Awake()
    {
        _isBuffersInited = false;
    }
    private void OnEnable()
    {
        if (_cam == null)
        {
            _cam = Camera.main;
        }
        
        Setup();
        UpdateLighting();
        UpdateWindParams();
    }

    private void OnDestroy()
    {
        if (_argsBuffer != null) _argsBuffer.Release();       
        _argsBuffer = null;

        if (_grassSampleCsContainers != null) _grassSampleCsContainers.Release();       
        _grassSampleCsContainers = null;

        if (_visibleGrassSamples != null) _visibleGrassSamples.Release();        
        _visibleGrassSamples = null;
        
        if(_computeBufferCamPlanes != null) _computeBufferCamPlanes.Release();
        _computeBufferCamPlanes = null;
        
        if(_computeBufferChunks!= null) _computeBufferChunks.Release();
        _computeBufferChunks = null;
    }

    private void Update()
    {
        if(!_isBuffersInited)
        {
            return;
        }
        
        UpdateLighting();

        _cameraPlanes = GeometryUtility.CalculateFrustumPlanes(_cam);
        for(int i=0;i<6;i++)
        {
            _camBufferPlanesArray[i].x = _cameraPlanes[i].normal.x;
            _camBufferPlanesArray[i].y = _cameraPlanes[i].normal.y;
            _camBufferPlanesArray[i].z = _cameraPlanes[i].normal.z;
            _camBufferPlanesArray[i].w = _cameraPlanes[i].distance;
        }
        _computeBufferCamPlanes.SetData(_camBufferPlanesArray);
        _cullingComputeShader.SetBuffer(0, "_camPlanes", _computeBufferCamPlanes);
        _visibleGrassSamples.SetCounterValue(0);
        _cullingComputeShader.Dispatch(0, 64, 1, 1);
        ComputeBuffer.CopyCount(_visibleGrassSamples, _argsBuffer, 4);
        Graphics.DrawMeshInstancedIndirect(_mesh, 0, _grassMat, _bounds, _argsBuffer);
    }
    
    private void UpdateLighting()
    {
        _grassMat.SetColor(_ambientColorShaderID, _ambientColor);
        
        if (_directLight == null)
        {
            return;
        }
        
        _grassMat.SetVector(_dirLightDirectionShaderID, _directLight.transform.forward);
        _grassMat.SetColor(_dirLightColorShaderID, _directLight.color);
    }
    
    private int SizeOfVisibleGrassSample()
    {
        return sizeof(float) * 4 * 4 + // matrix
               sizeof(float) * 4 * 2 + // colors
               sizeof(int); // AtlasID
    }
    private void Setup()
    {
        _bounds = new Bounds(transform.position, Vector3.one * 10000f);
        InitializeBuffers();
    }

    private void InitializeBuffers()
    {
        if (_grassChunkContainer == null || _grassChunkContainer.GrassChunks.Count == 0)
        {
            return;
        }
        
        _grassChunksCsList = new List<GrassChunkCs>();
        _camBufferPlanesArray = new Vector4[6];
        _computeBufferCamPlanes = new ComputeBuffer(6, sizeof(float) * 4);
        List<GrassSampleCsContainer> grassContainers = new List<GrassSampleCsContainer>();
        int grassCount = 0;
        for (int i=0; i< _grassChunkContainer.GrassChunks.Count;i++) // iterate all chunks
        {
            for(int j=0; j < _grassChunkContainer.GrassChunks[i].GrassSamplesCount;j++)
            {
                GrassSampleCsContainer container = new GrassSampleCsContainer();
                Vector3 position = _grassChunkContainer.GrassChunks[i].GrassPoses[j];
                Quaternion rotation = _grassChunkContainer.GrassChunks[i].GrassRotations[j];
                Vector3 scale = _grassChunkContainer.GrassChunks[i].GrassScales[j];

                container.Matrix = Matrix4x4.TRS(position, rotation, scale);
                container.ChunkID = _grassChunkContainer.GrassChunks[i].ChunkID;
                container.AtlasID = _grassChunkContainer.GrassChunks[i].IDs[j];
                container.LowColor = _grassChunkContainer.GrassChunks[i].LowColors[j];
                container.HighColor = _grassChunkContainer.GrassChunks[i].HighColors[j];
                grassContainers.Add(container);
                
                grassCount++;
            }
            
            GrassChunkCs csChunk = new GrassChunkCs();
            csChunk.ChunkCenterPos = _grassChunkContainer.GrassChunks[i].ChunkCenterPos;
            csChunk.ChunkSize = _grassChunkContainer.GrassChunks[i].ChunkSize;
            csChunk.GrassSamplesCount = _grassChunkContainer.GrassChunks[i].GrassSamplesCount;
            csChunk.MinY = _grassChunkContainer.GrassChunks[i].MinY;
            csChunk.MaxY = _grassChunkContainer.GrassChunks[i].MaxY;
            _grassChunksCsList.Add(csChunk);
        }
        // need to set chunks cs buffer
        _computeBufferChunks = new ComputeBuffer(_grassChunkContainer.GrassChunks.Count, GrassChunkCs.Size());
        _computeBufferChunks.SetData(_grassChunksCsList.ToArray());
        _cullingComputeShader.SetBuffer(0, "_grassChunks", _computeBufferChunks);
        
        _grassSampleCsContainers = new ComputeBuffer(grassContainers.Count, GrassSampleCsContainer.Size());
        _grassSampleCsContainers.SetData(grassContainers.ToArray());
        // set this buffer to compute shader. let 0 as kernel ID
        _cullingComputeShader.SetBuffer(0, "_grassSampleContainers", _grassSampleCsContainers);
        _cullingComputeShader.SetInt("_grassSamplesCount", grassCount);
        _visibleGrassSamples = new ComputeBuffer(grassCount, SizeOfVisibleGrassSample(), ComputeBufferType.Append);
        _cullingComputeShader.SetBuffer(0, "_visibleGrassSamples", _visibleGrassSamples);
        
        _grassMat.SetBuffer("_visibleGrassSamples", _visibleGrassSamples);

        if (_argsBuffer != null)
        {
            _argsBuffer.Release();
        }
            
        uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
        args[0] = (uint)_mesh.GetIndexCount(0);
        args[1] = (uint)grassCount;
        args[2] = (uint)_mesh.GetIndexStart(0);
        args[3] = (uint)_mesh.GetBaseVertex(0);
        _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(args);

        _isBuffersInited = true;
    }
}