using AssetBundles;
using DataAccess;
using RobotSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Gameboard
{
    public class GameboardSceneManager : MonoBehaviour
    {
        public UnityEvent onBeginLoading;
        public UnityEvent onEndLoading;

        // the root object for ar mode
        public Transform m_arRoot;
        public Transform m_uiRoot;
        public RobotColorSettings m_colorSettings;

        private Transform m_sceneRoot;
        private bool m_renderingOn;
        private Scene m_oldActiveScene;
        // keep track of the loaded scene with scene record instead of by name,
        // since we will have more than gameboard instances running simultaneously,
        // thus we can load a scene more than once, so name is not unique
        private Scene m_loadedScene;
        private GameboardConfig m_gameboardConfig;

        void Awake()
        {
            robotFactory = new RobotFactory(m_colorSettings, this);

            objectManager = new ObjectManager(this);
            robotManager = new RobotManager(robotFactory, objectManager);
            lightManager = new LightManager(objectManager);
            objectFactory = new ObjectFactory(this);

            m_oldActiveScene = SceneManager.GetActiveScene();
        }

        void OnDestroy()
        {
            RemoveObjects();
            robotFactory.Uninitialize();
            objectFactory.Uninitialize();

            if (currentTemplate != null)
            {
                AssetBundleManager.UnloadAssetBundle(currentTemplate.sceneBundleName);
            }
            AssetBundleManager.UnloadUnusedBundles();
        }

        void Update()
        {
            objectFactory.Update();
        }

        public RobotFactory robotFactory
        {
            get;
            private set;
        }

        public RobotManager robotManager
        {
            get;
            private set;
        }

        public ObjectManager objectManager
        {
            get;
            private set;
        }

        public ObjectFactory objectFactory
        {
            get;
            private set;
        }

        public Gameboard gameboard
        {
            get;
            private set;
        }

        public CameraManager cameraManager
        {
            get;
            private set;
        }

        public Camera currentCamera
        {
            get
            {
                return cameraManager ? cameraManager.currentCamera : null;
            }
        }

        public LightManager lightManager
        {
            get;
            private set;
        }

        public IEnumerator Initialize()
        {
            return robotFactory.Initialize();
        }

        public bool isLoading
        {
            get;
            private set;
        }

        public bool renderingOn
        {
            get { return m_renderingOn; }
            set
            {
                m_renderingOn = value;
                if (cameraManager)
                {
                    cameraManager.renderingOn = value;
                }
            }
        }

        public GameboardTemplateData currentTemplate
        {
            get
            {
                if (gameboard != null)
                {
                    return GameboardTemplateData.Get(gameboard.themeId);
                }
                return null;
            }
        }

        /// <summary>
        /// initialize the scene manager for the AR mode, existing scenes are unloaded
        /// </summary>
        public IEnumerator InitARMode()
        {
            if (isLoading)
            {
                Debug.LogError("cannot init ar mode while loading");
                yield break;
            }

            yield return Reset(false);
            m_sceneRoot = m_arRoot;
        }

        public IEnumerator Load(Gameboard project, bool createRobotIfNone)
        {
            if (isLoading)
            {
                Debug.LogError("already loading");
                yield break;
            }

            yield return LoadImpl(project, createRobotIfNone);
        }

        private IEnumerator LoadImpl(Gameboard newGameboard, bool createRobotIfNone)
        {
            var newTheme = GameboardTemplateData.Get(newGameboard.themeId);
            if (newTheme == null)
            {
                throw new ArgumentException("invalid theme id");
            }

            isLoading = true;

            if (onBeginLoading != null)
            {
                onBeginLoading.Invoke();
            }

            robotManager.Reset();
            RemoveObjects();

            if (gameboard == null || newGameboard.themeId != gameboard.themeId)
            {
                if (gameboard != null)
                {
                    ActivateOldActiveScene();
                    yield return SceneManager.UnloadSceneAsync(m_loadedScene);

                    AssetBundleManager.UnloadAssetBundle(currentTemplate.sceneBundleName);
                    AssetBundleManager.UnloadUnusedBundles();
                }

                var loadRequest = AssetBundleManager.LoadLevelAsync(newTheme.sceneBundleName, newTheme.sceneName, true);
                yield return StartCoroutine(loadRequest);

                m_loadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);
                SceneManager.SetActiveScene(m_loadedScene);

                var roots = m_loadedScene.GetRootGameObjects();

                Assert.IsTrue(roots.Length == 1, "scene should have only 1 root");

                cameraManager = roots[0].GetComponent<CameraManager>();
                Assert.IsNotNull(cameraManager, "camera manager null");
                cameraManager.renderingOn = renderingOn;

                m_sceneRoot = roots[0].transform;

                m_gameboardConfig = m_sceneRoot.GetComponent<GameboardConfig>();
                if (!m_gameboardConfig)
                {
                    Debug.LogError("GameboardConfig not found in scene " + newTheme.sceneName);
                }
            }

            gameboard = newGameboard;

            // reload all robots
            if (gameboard.robots.Count == 0 && createRobotIfNone)
            {
                if (m_gameboardConfig)
                {
                    if (m_gameboardConfig.defaultRobotSpawnPoint)
                    {
                        bool dirty = gameboard.isDirty;

                        Vector3 pos;
                        PhysicsUtils.GetPlacementPosition(m_gameboardConfig.defaultRobotSpawnPoint.position.xz(), out pos);
                        gameboard.AddRobot(new RobotInfo {
                            position = pos,
                            rotation = m_gameboardConfig.defaultRobotSpawnPoint.eulerAngles.y
                        });

                        gameboard.isDirty = dirty;
                    }
                    else
                    {
                        Debug.LogError("no default spawn point");
                    }
                }
            }

            lightManager.ambientIlluminance = m_gameboardConfig != null ? m_gameboardConfig.ambientIllumiance : 0.0f;
            robotFactory.OnSceneLoaded();

            robotManager.SetGameboard(gameboard);
            robotManager.SetDefaultSpawnPoint(m_gameboardConfig.defaultRobotSpawnPoint);

            yield return ResetObjects(true);

            isLoading = false;

            if (onEndLoading != null)
            {
                onEndLoading.Invoke();
            }
        }

        private IEnumerator CreateObjects(bool active)
        {
            if (gameboard == null) { yield break; }

            foreach (var objectInfo in gameboard.objects)
            {
                yield return objectFactory.Create(
                    new ObjectCreateInfo(objectInfo) {
                        active = false,
                    });
            }

            if (active)
            {
                objectManager.ActivateAll();
            }
        }

        public IEnumerator ResetObjects(bool active)
        {
            RemoveObjects();
            yield return CreateObjects(active);
            robotManager.ResetRobots();
        }

        /// <summary>
        /// reset the gameboard, remove all objects, unload the scene
        /// </summary>
        public IEnumerator Reset()
        {
            if (isLoading)
            {
                Debug.LogError("cannot reset while loading");
                yield break;
            }

            yield return Reset(true);
        }

        private IEnumerator Reset(bool unloadBundles)
        {
            robotManager.Reset();
            RemoveObjects();

            if (m_loadedScene.isLoaded)
            {
                ActivateOldActiveScene();

                yield return SceneManager.UnloadSceneAsync(m_loadedScene);
                if (unloadBundles)
                {
                    AssetBundleManager.UnloadAssetBundle(currentTemplate.sceneBundleName);
                }
            }

            m_loadedScene = new Scene();
            m_sceneRoot = null;
            gameboard = null;

            if (unloadBundles)
            {
                AssetBundleManager.UnloadUnusedBundles();
            }
        }

        private void ActivateOldActiveScene()
        {
            if (m_oldActiveScene.isLoaded)
            {
                SceneManager.SetActiveScene(m_oldActiveScene);

                if (m_loadedScene.isLoaded)
                {
                    // HACk: to avoid destroying objects not belong to the gameboard scene,
                    // we need to move them to the old active scene.
                    foreach (var go in m_loadedScene.GetRootGameObjects())
                    {
                        if (go.transform != m_sceneRoot)
                        {
                            SceneManager.MoveGameObjectToScene(go, m_oldActiveScene);
                        }
                    }
                }
            }
        }

        public void RemoveObjects()
        {
            robotManager.RemoveRobots();
            objectManager.Reset();
            objectFactory.RemoveAllRequests();
            lightManager.RemoveLights();
        }

        public void ActivateSceneObjects(bool active)
        {
            if (m_sceneRoot)
            {
                m_sceneRoot.gameObject.SetActive(active);
            }
        }

        public Transform sceneRoot
        {
            get { return m_sceneRoot; }
        }

        public Transform uiRoot
        {
            get { return m_uiRoot; }
        }

        public Bounds robotPlacementBounds
        {
            get
            {
                if (m_gameboardConfig == null)
                {
                    return new Bounds();
                }
                return m_gameboardConfig.robotPlacementArea.bounds;
            }
        }
    }
}
