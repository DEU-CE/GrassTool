using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEditor.AnimatedValues;
using Random = UnityEngine.Random;

public partial class GrassEditor : EditorWindow
{
    private readonly int _lowColorShaderId = Shader.PropertyToID("_LowColor");
    private readonly int _highColorShaderId = Shader.PropertyToID("_HighColor");
    private readonly int _atlasIDColorShaderId = Shader.PropertyToID("_AtlasID");
    public GrassDataContainer GrassContainer;
    public GrassChunkContainer ChunksContainer;
    private List<GrassChunk> _grassChunks;
    private const string EDITOR_GRASS_SHADER_NAME = "Grass/InstanceGrassEditor";
    
    private Material _grassEditorMaterial;
    private Texture2D _grassEditorTexture;

    private float _radius;
    private float _scale;
    private int _grassCount;

    private float _paintRadius;
    private float _eraseRadius;
    private bool isPlacing = false;
    private bool isErasing = false;
    private bool isPainting = false;

    private bool _updateScene;

    private Vector3 _genBoundLeftDownCorner;
    private Vector3 _genBoundRightUpCorner;
    private bool _isShowBounds;
    private bool _isShowChunksData;
    
    private int _horizontalChunksCount;
    private int _verticalChunksCount;

    private string _grassDataSoPath;
    private string _grassDataSoName;
    private string _grassChunkPath;
    private string _grassChunkName;
    private string _textureAtlasName;
    private Texture2D _texture1;
    private Texture2D _texture2;
    private bool _isTwoTextures;
    private List<Matrix4x4> _grassRawCsSamples;
    
    private Mesh _mesh;
    private Camera _sceneCam;
    
    private AnimBool _scaleSettingsAnimBool;
    private float _minScaleValue;
    private float _maxScaleValue;
    
    private Vector2 _scrollPosition;
    
    private AnimBool _vertexColorSettingsAnimBool;
    private Color _lowColor;
    private Color _highColor;
    
    private AnimationCurve _vertexColorCurve;
    private AnimBool _vertexColorCurveAnimBool;
    private bool _isCurveColorBlend;
    private string TextureStatus => GrassContainer?._isDoubleTexture == true ? "Double Texture" : "Single Texture";
    
    private Mesh _editorMesh;

    private bool _showDrawingSettings;
    private bool _showEditorGrassDataSettings;
    private bool _showAtlassingSettings;
    private bool _showVertexColorSettings;
    private bool _showEraserSettings;
    private bool _showBakerSettings;
    
    
    [MenuItem("Tools/Grass Editor")]
    public static void ShowWindow()
    {
        GetWindow<GrassEditor>("Grass Editor");
    }

    private void OnEnable()
    {
        _grassEditorMaterial = new Material(Shader.Find(EDITOR_GRASS_SHADER_NAME));
        _grassEditorMaterial.mainTexture = _grassEditorTexture;
        _isCurveColorBlend = false;
        
        SceneView.duringSceneGui += DrawMesh;
        _scaleSettingsAnimBool = new AnimBool(false);
        _vertexColorCurveAnimBool = new AnimBool(false);
        _scaleSettingsAnimBool.valueChanged.AddListener(Repaint);
        _vertexColorCurveAnimBool.valueChanged.AddListener(Repaint);
        
        _vertexColorCurve = AnimationCurve.Linear(0, 0, 1, 1);
        
        _minScaleValue = 1f;
        _maxScaleValue = 1f;
    }
    
    private void OnDisable()
    {
        SceneView.duringSceneGui -= DrawMesh;
    }
    
    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        DestroyImmediate(_grassEditorMaterial);
    }
    
    private void DrawMesh(SceneView sceneView)
    {
        if (GrassContainer == null)
        {
            return;
        }
        
        if (_grassEditorMaterial == null)
        {
            _grassEditorMaterial = new Material(Shader.Find(EDITOR_GRASS_SHADER_NAME));
            _grassEditorMaterial.mainTexture = _grassEditorTexture;
        }
        
        if (Event.current.type == EventType.Repaint && _editorMesh != null && _grassEditorMaterial != null && !Application.isPlaying)
        {
            MaterialPropertyBlock properties = new MaterialPropertyBlock();
            RenderParams rp = new RenderParams(_grassEditorMaterial)
            {
                camera = sceneView.camera,
                matProps = properties
            };
            
            for (int i = 0; i < GrassContainer.GrassTransforms.Count; i++)
            {
                GrassSample grassTransform = GrassContainer.GrassTransforms[i];
                Matrix4x4 m = Matrix4x4.TRS(grassTransform.Position, grassTransform.Rotation, grassTransform.Scale);
                
                properties.SetColor(_lowColorShaderId, grassTransform.LowColor);
                properties.SetColor(_highColorShaderId, grassTransform.HighColor);
                properties.SetInt(_atlasIDColorShaderId, grassTransform.AtlasID);
                Graphics.RenderMesh(rp, _editorMesh, 0, m);
            }
        }
    }

    bool IsHasGrassDataContainer() => GrassContainer != null;
    
    void WarnNoGrassData()
    {
        Debug.LogWarning("No Grass Data SO!");
    }

    void AddGrassData(GrassSample grassTr)
    {
        if (!IsHasGrassDataContainer())
        {
            WarnNoGrassData();
            return;
        }
        GrassContainer.GrassTransforms.Add(grassTr);
    }

    void DeleteGrassData(GrassSample grassTr)
    {
        if (!IsHasGrassDataContainer())
        {
            WarnNoGrassData();
            return;
        }
        
        GrassContainer.GrassTransforms.Remove(grassTr);
    }

    private void RefreshEditorTexture()
    {
        _grassEditorMaterial.mainTexture = _grassEditorTexture;
    }
    private void RecalculateAtlasIDs()
    {
        if (GrassContainer == null)
        {
            WarnNoGrassData();
            return;
        }
        foreach (GrassSample grassSample in GrassContainer.GrassTransforms)
        {
            grassSample.AtlasID = GrassContainer._isDoubleTexture ? Random.Range(1, 3) : 1;
        }
    }
    private void OnGUI()
    {
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
        _showDrawingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showDrawingSettings, "Draw grass settings");
        if (_showDrawingSettings)
        {
            EditorGUILayout.Space(20);
            _radius = EditorGUILayout.FloatField("Draw radius:", _radius);
            _scale = EditorGUILayout.FloatField("Grass scale:", _scale);
        
            _scaleSettingsAnimBool.target = EditorGUILayout.Toggle("Randomize vertical scale", _scaleSettingsAnimBool.target);
            if (EditorGUILayout.BeginFadeGroup(_scaleSettingsAnimBool.faded))
            {
                _minScaleValue = EditorGUILayout.FloatField("Min scale value:", _minScaleValue);
                _maxScaleValue = EditorGUILayout.FloatField("Max scale value:", _maxScaleValue);
            }
        
            EditorGUILayout.EndFadeGroup();
        
            _grassCount = EditorGUILayout.IntField("Grass count: ", _grassCount);
            EditorGUILayout.Separator();
            _editorMesh = (Mesh)EditorGUILayout.ObjectField("Editor mesh", _editorMesh, typeof(Mesh), false);  
            
            if (GUILayout.Button(isPlacing ? "Stop Placing" : "Start Placing"))
            {
                if(IsHasGrassDataContainer())
                {
                    isPlacing = !isPlacing;
                }
                else
                {
                    WarnNoGrassData();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.Separator();
        _updateScene = isPlacing || isErasing || isPainting || _isShowChunksData || _isShowBounds;
        
        _showEditorGrassDataSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showEditorGrassDataSettings, "Grass data SO settings");
        if (_showEditorGrassDataSettings)
        {
            GrassContainer = (GrassDataContainer)EditorGUILayout.ObjectField("Grass data SO", GrassContainer, typeof(GrassDataContainer), false);
            if (GrassContainer == null)
            {
                _grassDataSoPath = EditorGUILayout.TextField("SO save path", _grassDataSoPath);
                _grassDataSoName = EditorGUILayout.TextField("SO save name", _grassDataSoName);
                if (GUILayout.Button("Create grass data SO"))
                {
                    if (!string.IsNullOrEmpty(_grassDataSoPath) && !string.IsNullOrEmpty(_grassDataSoName))
                    {
                        GrassDataContainer tmp = CreateInstance<GrassDataContainer>();
                        GrassContainer = tmp;
                        AssetDatabase.CreateAsset(tmp, _grassDataSoPath + "/" + _grassDataSoName + ".asset");
                    }
                    else
                    {
                        Debug.LogError("Grass data SO path or name is empty");
                    }
                }
            }
            if (GUILayout.Button("Save grass data"))
            {
                SaveFile(GrassContainer);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Separator();
        _showAtlassingSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showAtlassingSettings, "Texture atlassing settings");
        if (_showAtlassingSettings)
        {
            GUILayout.Label("Texture status: " + TextureStatus, EditorStyles.boldLabel);
            if (GrassContainer is not null)
            {
                if (GrassContainer._isDoubleTexture)
                {
                    if (GUILayout.Button("Switch to single texture"))
                    {
                        GrassContainer._isDoubleTexture = false;
                        RecalculateAtlasIDs();
                    }
                }
                else
                {
                    if (GUILayout.Button("Switch to double texture"))
                    {
                        GrassContainer._isDoubleTexture = true;
                        RecalculateAtlasIDs();
                    }
                }
            }
        
            EditorGUILayout.Separator();
            _grassEditorTexture = EditorGUILayout.ObjectField("Texture for editor", _grassEditorTexture, typeof(Texture2D), false) as Texture2D;
            if (GUILayout.Button("Refresh editor texture"))
            {
                RefreshEditorTexture();
            }
            EditorGUILayout.Separator();
            _texture1 = EditorGUILayout.ObjectField("Texture 1", _texture1, typeof(Texture2D), false) as Texture2D;
            _isTwoTextures = EditorGUILayout.Toggle("Use two textures", _isTwoTextures);
            
            if (_isTwoTextures)
            {
                _texture2 = EditorGUILayout.ObjectField("Texture 2", _texture2, typeof(Texture2D), false) as Texture2D;
            }
        
            _textureAtlasName = EditorGUILayout.TextField("Texture atlas name", _textureAtlasName);

            if (GUILayout.Button("Generate texture atlas"))
            {
                if (_texture1 == null || _isTwoTextures && (_texture1 == null || _texture2 == null))
                {
                    Debug.LogError("No textures for atlas generating");
                }
                else
                {
                    string path = EditorUtility.SaveFolderPanel("Choose path for saving atlas", "Assets/", "");
                    if (path.Length != 0)
                    {
                        path += "/";
                        path = FileUtil.GetProjectRelativePath(path);
                        GenerateTextureAtlas(path);
                    }
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Separator();
        _showVertexColorSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showVertexColorSettings, "Vertex color settings");
        if (_showVertexColorSettings)
        {
            _paintRadius = EditorGUILayout.FloatField("Paint radius", _paintRadius);
            _lowColor = EditorGUILayout.ColorField("Low Color", _lowColor);
            _highColor = EditorGUILayout.ColorField("High Color", _highColor);
        
            _vertexColorCurveAnimBool.target = EditorGUILayout.Toggle("Use non-linear blend curve", _vertexColorCurveAnimBool.target);
            if (EditorGUILayout.BeginFadeGroup(_vertexColorCurveAnimBool.faded))
            {
                _vertexColorCurve = EditorGUILayout.CurveField("Color blend curve", _vertexColorCurve);
                _isCurveColorBlend = true;
            }
            else
            {
                _isCurveColorBlend = false;
            }
            EditorGUILayout.EndFadeGroup();
            
            if (GUILayout.Button(isPainting ? "Stop Painting" : "Start Painting"))
            {
                if (IsHasGrassDataContainer())
                {
                    isPainting = !isPainting;
                }
                else
                {
                    WarnNoGrassData();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        
        EditorGUILayout.Separator();
        _showEraserSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showEraserSettings, "Eraser settings");
        if (_showEraserSettings)
        {
            _eraseRadius = EditorGUILayout.FloatField("Erase radius:", _eraseRadius);
            if (GUILayout.Button(isErasing ? "Stop Erasing" : "Start Erasing"))
            {
                if (IsHasGrassDataContainer())
                {
                    isErasing = !isErasing;
                }
                else
                {
                    WarnNoGrassData();
                }
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();

        if (_updateScene)
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }
        else
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        EditorGUILayout.Separator();
        _showBakerSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showBakerSettings, "Baker settings");
        if (_showBakerSettings)
        {
            ChunksContainer = (GrassChunkContainer)EditorGUILayout.ObjectField("Grass chunks SO", ChunksContainer, typeof(GrassChunkContainer), false);
            if (ChunksContainer == null)
            {
                _grassChunkPath = EditorGUILayout.TextField("Chunks save path", _grassChunkPath);
                _grassChunkName = EditorGUILayout.TextField("Chunks save name", _grassChunkName);
                if (GUILayout.Button("Create chunk data SO"))
                {
                    if (!string.IsNullOrEmpty(_grassChunkPath) && !string.IsNullOrEmpty(_grassChunkName))
                    {
                        GrassChunkContainer tmp = CreateInstance<GrassChunkContainer>();
                        ChunksContainer = tmp;
                        AssetDatabase.CreateAsset(tmp, _grassChunkPath + "/" + _grassChunkName + ".asset");
                    }
                    else
                    {
                        Debug.LogError("Grass chunk filepath or name is empty");
                    }
                }
            }
            _isShowBounds = EditorGUILayout.Toggle("Show bounds", _isShowBounds);
            _isShowChunksData = EditorGUILayout.Toggle("Show chunks data", _isShowChunksData);
            _horizontalChunksCount = EditorGUILayout.IntField("Horizontal chunks count: ", _horizontalChunksCount);
            _verticalChunksCount = EditorGUILayout.IntField("Vertical chunks count: ", _verticalChunksCount);

            if (GUILayout.Button("Bake chunks"))
            {
                GrassBaker grassBaker = new GrassBaker(GrassContainer);
                grassBaker.SetChunksDimensions(_verticalChunksCount, _horizontalChunksCount);
                List<Vector3> bounds = grassBaker.GetGeneralBounds();
                _genBoundLeftDownCorner = bounds[0];
                _genBoundRightUpCorner = bounds[1];
                _grassChunks = new();
                foreach(GrassSample sample in GrassContainer.GrassTransforms)
                {
                    sample.IsBaked = false;
                }
                _grassChunks = grassBaker.BakeGrassIntoChunks();
                ChunksContainer.GrassChunks = _grassChunks;
                ChunksContainer._isDoubleTexture = GrassContainer._isDoubleTexture;
                ChunksContainer.HorizontalChunkCnt = _horizontalChunksCount;
                ChunksContainer.VerticalChunkCnt = _verticalChunksCount;
                
                SaveFile(ChunksContainer);
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        EditorGUILayout.EndScrollView();
    }

    private void GenerateTextureAtlas(string savePath = "Assets/")
    {
        TextureAtlasGenerator.GenerateAndSave(_texture1, _texture2, _textureAtlasName, savePath);
    }
    
    private void GenerateIdleWindDir(GrassSample grass)
    {
        grass.LowColor.a = Random.Range(-1f, 1f);
        grass.HighColor.a = Random.Range(-1f, 1f);
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        if (!IsHasGrassDataContainer())
        {
            WarnNoGrassData();
            return;
        }

        if (_sceneCam == null)
        {
            _sceneCam = sceneView.camera;
        }

        if(_isShowBounds)
        {
            DrawBounds(sceneView, ChunksContainer.HorizontalChunkCnt, ChunksContainer.VerticalChunkCnt);
        }

        if(_isShowChunksData)
        {
            DrawChunksData(sceneView);
        }

        sceneView.Repaint();

        Event e = Event.current;
        
        Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
        RaycastHit hit;
        Vector3 endDirPos, startDirPos, rayDir;
        if (Physics.Raycast(ray, out hit))
        {           
            if (isPlacing)
            {
                Handles.color = Color.green;
                Handles.DrawWireDisc(hit.point, hit.normal, _radius);                
            }
            if(isErasing)
            {
                Handles.color = Color.red;
                Handles.DrawWireDisc(hit.point, hit.normal, _eraseRadius);               
            }
            if (isPainting)
            {
                Handles.color = Color.yellow;
                Handles.DrawWireDisc(hit.point, hit.normal, _paintRadius);
            }
            sceneView.Repaint();

            if (e.type == EventType.MouseDown && e.button == 0)
            {
                if (isPlacing)
                {
                    for(int i=0;i< _grassCount; i++)
                    {                
                        Vector2 randPos = Random.insideUnitCircle * _radius;

                        GrassSample tmp = new();
                        endDirPos = hit.point + new Vector3(randPos.x, 0, randPos.y);
                        startDirPos = endDirPos + hit.normal*10000f;
                        rayDir = Vector3.Normalize(endDirPos-startDirPos);
                        RaycastHit randHit;
                        if (Physics.Raycast(startDirPos, rayDir, out randHit))
                        {
                            tmp.Position = randHit.point;
                            tmp.Rotation = Quaternion.FromToRotation(Vector3.up, randHit.normal);
                            float heightScaleFactor = Random.Range(_minScaleValue, _maxScaleValue);
                            tmp.Scale = new Vector3(_scale,_scale*heightScaleFactor,_scale);
                            tmp.LowColor = Color.white;
                            tmp.HighColor = Color.white;
                            tmp.AtlasID = GrassContainer._isDoubleTexture ? Random.Range(1, 3) : 0;

                            GenerateIdleWindDir(tmp);
                            AddGrassData(tmp);
                        }
                    }
                }

                if (isErasing)
                {
                    Vector3 groundPos = new Vector3(hit.point.x, 0, hit.point.z);
                    bool isHasGrass = true;
                    while (isHasGrass)
                    {
                        if (GrassContainer.GrassTransforms.Count <= 0)
                        {
                            break;
                        }
                        foreach (GrassSample grassSample in GrassContainer.GrassTransforms)
                        {
                            isHasGrass = false;
                            if (Vector3.Distance(groundPos, new Vector3(grassSample.Position.x,0f,grassSample.Position.z)) <= _eraseRadius)
                            {
                                isHasGrass = true;
                                if (isErasing)
                                {
                                    DeleteGrassData(grassSample);
                                }
                                break;
                            }
                        }
                    }
                }

                if (isPainting)
                {
                    foreach (GrassSample grassSample in GrassContainer.GrassTransforms)
                    {
                        Vector3 groundPos = new Vector3(hit.point.x, 0, hit.point.z);
                        float distance = Vector3.Distance(groundPos, new Vector3(grassSample.Position.x, 0f, grassSample.Position.z));
                        if (distance <= _paintRadius)
                        {
                            if (!_isCurveColorBlend)
                            {
                                grassSample.LowColor = _lowColor;
                                grassSample.HighColor = _highColor;
                            }
                            else
                            {
                                float normalizedDistance = 1f-distance/_paintRadius;
                                grassSample.LowColor = Color.Lerp(grassSample.LowColor, _lowColor, 
                                    _vertexColorCurve.Evaluate(normalizedDistance));
                                
                                grassSample.HighColor = Color.Lerp(grassSample.HighColor, _highColor, 
                                    _vertexColorCurve.Evaluate(normalizedDistance));
                            }
                            GenerateIdleWindDir(grassSample);
                        }
                    }
                }
                e.Use();
            }
        }
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
    }    
    
    private void SaveFile(ScriptableObject so)
    {
        EditorUtility.SetDirty(so); 
        AssetDatabase.SaveAssets(); 
        AssetDatabase.Refresh(); 
    }
}