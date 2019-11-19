using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

// use of ParticleEmitter
#pragma warning disable 618

public class ObjectThumbnailGeneratorEditor : EditorWindow
{
    [Serializable]
    public class Config
    {
        public Vector2 viewportSize;
        public Vector2 objectScreenPos;
        public Vector2 objectScreenSize;
        public Vector3 cameraPosBias;
        public Vector2 uiObjectPadding;
        public int thumbnailTextureWidth;

        public static Config Load(string file)
        {
            return JsonUtility.FromJson<Config>(File.ReadAllText(file));
        }
    }

    private const string DefaultConfigFile = "object_thumbnail_generator.json";
    private const string UserConfigFile = "." + DefaultConfigFile;

    private float m_particleSimulateTime;
    private bool m_hasParticleSystem;
    private int m_generateDelay;

    private ObjectThumbnailPreviewer m_previewer;

    private SerializedObject m_serializedObject;
    private SerializedProperty m_viewportSize;
    private SerializedProperty m_objectScreenPos;
    private SerializedProperty m_objectScreenSize;
    private SerializedProperty m_cameraPosBias;
    private SerializedProperty m_uiObjectPadding;
    private SerializedProperty m_adjustUIObjectViewport;

    private bool m_showMaterials;
    private GameObject m_lastSelection;
    private int m_thumbnailTextureWidth;

    [MenuItem("Tools/Object Thumbnail Generator")]
    public static void Init()
    {
        GetWindow<ObjectThumbnailGeneratorEditor>();
    }

    void OnEnable()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
        EditorApplication.update += OnUpdate;

        InitializeFromScene();
    }

    void InitializeFromScene()
    {
        if (Camera.main)
        {
            var camEvent = Camera.main.GetComponent<CameraEvent>();
            if (camEvent)
            {
                camEvent.onPostRender += OnPostRender;
            }
        }

        m_previewer = FindObjectOfType<ObjectThumbnailPreviewer>();
        InitPreviewProperties();
        OnSelectionChange();
    }

    void InitPreviewProperties()
    {
        if (m_previewer && m_serializedObject == null)
        {
            m_serializedObject = new SerializedObject(m_previewer);
            m_viewportSize = m_serializedObject.FindProperty("m_viewportSize");
            m_objectScreenPos = m_serializedObject.FindProperty("m_objectScreenPos");
            m_objectScreenSize = m_serializedObject.FindProperty("m_objectScreenSize");
            m_cameraPosBias = m_serializedObject.FindProperty("m_cameraPosBias");
            m_uiObjectPadding = m_serializedObject.FindProperty("m_uiObjectPadding");
            m_adjustUIObjectViewport = m_serializedObject.FindProperty("m_adjustUIObjectViewport");

            LoadProperties();
        }
    }

    void LoadProperties()
    {
        Config config;
        if (File.Exists(UserConfigFile))
        {
            config = Config.Load(UserConfigFile);
        }
        else if (File.Exists(DefaultConfigFile))
        {
            config = Config.Load(DefaultConfigFile);
        }
        else
        {
            config = new Config();
        }

        m_viewportSize.vector2Value = config.viewportSize;
        m_objectScreenPos.vector2Value = config.objectScreenPos;
        m_objectScreenSize.vector2Value = config.objectScreenSize;
        m_cameraPosBias.vector3Value = config.cameraPosBias;
        m_uiObjectPadding.vector2Value = config.uiObjectPadding;
        m_thumbnailTextureWidth = config.thumbnailTextureWidth;
    }

    void OnDisable()
    {
        EditorSceneManager.sceneOpened -= OnSceneOpened;
        EditorApplication.playmodeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.update -= OnUpdate;

        if (Camera.main)
        {
            var camEvent = Camera.main.GetComponent<CameraEvent>();
            if (camEvent)
            {
                camEvent.onPostRender -= OnPostRender;
            }
        }

        SaveProperties();
    }

    void SaveProperties()
    {
        if (m_serializedObject != null)
        {
            var config = new Config();
            config.viewportSize = m_viewportSize.vector2Value;
            config.objectScreenPos = m_objectScreenPos.vector2Value;
            config.objectScreenSize = m_objectScreenSize.vector2Value;
            config.cameraPosBias = m_cameraPosBias.vector3Value;
            config.uiObjectPadding = m_uiObjectPadding.vector2Value;
            config.thumbnailTextureWidth = m_thumbnailTextureWidth;

            File.WriteAllText(UserConfigFile, JsonUtility.ToJson(config));
        }
    }

    private void OnPlayModeStateChanged()
    {
        InitializeFromScene();
        Repaint();
    }

    private void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        InitializeFromScene();
        Repaint();
    }

    void OnSelectionChange()
    {
        Repaint();

        if (Selection.activeGameObject && PrefabUtility.GetPrefabParent(Selection.activeGameObject))
        {
            m_lastSelection = Selection.activeGameObject;
        }
    }

    private void OnPostRender()
    {
        if (--m_generateDelay == 0)
        {
            Generate();
            m_previewer.objectScreenRectVisible = true;
        }
    }

    private void OnUpdate()
    {
        if (!EditorApplication.isPlaying && m_lastSelection && m_lastSelection.activeInHierarchy)
        {
            // animate the character so that we don't need to enter the play mode to take the capture
            var animator = m_lastSelection.GetComponentInChildren<Animator>();
            if (animator)
            {
                animator.Update(0.033f);
            }
        }
    }

    void OnGUI()
    {
        if (!m_previewer)
        {
            GUILayout.Label("ObjectThumbnail scene is not opened");
            if (GUILayout.Button("Open"))
            {
                EditorSceneManager.OpenScene("Assets/Scenes/ObjectThumbnail.unity");
            }
            return;
        }

        m_thumbnailTextureWidth = EditorGUILayout.IntField("Thumbnail Texture Width", m_thumbnailTextureWidth);

        m_serializedObject.Update();
        EditorGUILayout.PropertyField(m_viewportSize);
        EditorGUILayout.PropertyField(m_objectScreenPos);
        EditorGUILayout.PropertyField(m_objectScreenSize);
        EditorGUILayout.PropertyField(m_cameraPosBias);
        EditorGUILayout.PropertyField(m_uiObjectPadding);
        EditorGUILayout.PropertyField(m_adjustUIObjectViewport);
        m_serializedObject.ApplyModifiedProperties();

        DrawParticleSystemGUI();

        m_showMaterials = EditorGUILayout.Toggle("Show Materials", m_showMaterials);
        if (m_showMaterials)
        {
            DrawMaterials();
        }

        EditorGUILayout.BeginHorizontal();
        GUI.enabled = m_lastSelection;
        if (GUILayout.Button("Focus"))
        {
            m_previewer.Focus(m_lastSelection);
        }

        GUI.enabled = m_lastSelection;
        if (GUILayout.Button("Generate"))
        {
            if (EditorApplication.isPlaying)
            {
                // wait 2 frames to avoid capturing the object rect
                m_generateDelay = 2;
                m_previewer.objectScreenRectVisible = false;
            }
            else
            {
                Generate();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
        DrawHelpMsg();
    }

    private void DrawParticleSystemGUI()
    {
        if (!m_lastSelection)
        {
            m_hasParticleSystem = false;
            return;
        }

        var particleSystems = m_lastSelection.GetComponentsInChildren<ParticleSystem>();
        m_hasParticleSystem = particleSystems.Length > 0;
        
        if (m_hasParticleSystem)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginChangeCheck();
            m_particleSimulateTime = EditorGUILayout.Slider("Particle Simulation Time", m_particleSimulateTime, 0, 5.0f);
            bool changed = EditorGUI.EndChangeCheck();
            EditorGUILayout.EndHorizontal();

            foreach (var ps in particleSystems)
            {
                ps.Simulate(m_particleSimulateTime);
            }

            if (changed && !EditorApplication.isPlaying)
            {
                EditorUtils.RepaintGameView();
            }
        }
    }
    
    void DrawMaterials()
    {
        if (m_lastSelection)
        {
            var particleMats = m_lastSelection
                     .GetComponentsInChildren<Renderer>()
                     .SelectMany(x => x.sharedMaterials)
                     .Where(x => x != null)
                     .Distinct();

            foreach (var mat in particleMats)
            {
                EditorGUILayout.ObjectField(mat, typeof(Material));
            }
        }
    }

    void DrawHelpMsg()
    {
        string helpMsg = "";
        if (!m_lastSelection)
        {
            helpMsg += "Select a scene object of prefab to focus";
        }
        if (m_hasParticleSystem && !EditorApplication.isPlaying)
        {
            if (helpMsg != "")
            {
                helpMsg += "\n";
            }
            helpMsg += "run the game before capturing particle systems";
        }
        if (helpMsg != "")
        {
            EditorGUILayout.HelpBox(helpMsg, MessageType.Info);
        }
    }

    void Generate()
    {
        var camera = Camera.main;
        int x = (camera.pixelWidth - (int)m_previewer.viewportSize.x) / 2;
        int y = (camera.pixelHeight - (int)m_previewer.viewportSize.y) / 2;

        var texture = new Texture2D((int)m_previewer.viewportSize.x, (int)m_previewer.viewportSize.y, TextureFormat.RGBA32, false);

        if (!Application.isPlaying)
        {
            var rt = RenderTexture.GetTemporary(camera.pixelWidth, camera.pixelHeight, 16);
            camera.targetTexture = rt;
            RenderTexture.active = rt;
            camera.Render();
        }
        else
        {
            RenderTexture.active = null;
        }

        texture.ReadPixels(new Rect(x, y, (int)m_previewer.viewportSize.x, (int)m_previewer.viewportSize.y), 0, 0);
        SaveThumbnail(texture, PrefabUtility.GetPrefabParent(m_lastSelection));

        if (!Application.isPlaying)
        {
            var rt = camera.targetTexture;
            camera.targetTexture = null;
            RenderTexture.ReleaseTemporary(rt);
            RenderTexture.active = null;
        }

        UnityEngine.Object.DestroyImmediate(texture);
    }

    private void SaveThumbnail(Texture2D texture, UnityEngine.Object prefab)
    {
        var assetPath = AssetDatabase.GetAssetPath(prefab);
        var folder = Path.GetDirectoryName(assetPath);
        var thumbnailPath = folder + "/" + Path.GetFileNameWithoutExtension(assetPath) + "-thumbnail.png";
        File.WriteAllBytes(thumbnailPath, texture.EncodeToPNG());

        var outputHeight = (int)(m_thumbnailTextureWidth / (m_previewer.viewportSize.x / m_previewer.viewportSize.y));
        var resizeCmd = string.Format("Tools/resize.py {0} {1} {2}", thumbnailPath, m_thumbnailTextureWidth, outputHeight);

        var startInfo = new ProcessStartInfo();
        startInfo.FileName = "python";
        startInfo.Arguments = resizeCmd;
        startInfo.CreateNoWindow = true;
        startInfo.UseShellExecute = false;
        using (Process.Start(startInfo)) { }

        AssetDatabase.ImportAsset(thumbnailPath);
        var importer = (TextureImporter)AssetImporter.GetAtPath(thumbnailPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.mipmapEnabled = false;
        importer.spritePackingTag = AssetDatabase.GetImplicitAssetBundleName(assetPath);
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }

}
