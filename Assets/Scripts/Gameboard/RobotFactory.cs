using AssetBundles;
using RobotSimulation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Gameboard
{
    public class RobotFactory
    {
        private const string RobotBundleName = "gb-robot";
        private const string RobotPrefabName = "Robot";

        private readonly RobotColorSettings m_colorSettings;
        private GameObject m_robotPrefab;

        private AssetBundleLoadAssetOperation m_loadOp;
        private FloorGrid m_floorGrid;
        private readonly GameboardSceneManager m_sceneManager;

        public RobotFactory(RobotColorSettings colorSettings, GameboardSceneManager sceneManager)
        {
            m_colorSettings = colorSettings;
            m_sceneManager = sceneManager;
        }

        public void OnSceneLoaded()
        {
            m_floorGrid = m_sceneManager.sceneRoot.GetComponent<FloorGrid>();
        }

        public IEnumerator Initialize()
        {
            if (initialized) { yield break; }

            if (m_loadOp == null)
            {
                m_loadOp = AssetBundleManager.LoadAssetAsync(RobotBundleName, RobotPrefabName, typeof(GameObject));
            }
            yield return m_loadOp;

            m_robotPrefab = m_loadOp.GetAsset<GameObject>();
            m_loadOp = null;
            initialized = true;
        }

        public bool initialized { get; private set; }

        public void Uninitialize()
        {
            initialized = false;
            
            if (m_loadOp != null)
            {
                m_loadOp.Dispose();
                m_loadOp = null;
            }

            m_robotPrefab = null;
        }

        public Robot Create()
        {
            if (m_robotPrefab == null)
            {
                throw new InvalidOperationException();
            }

            var go = GameObject.Instantiate(m_robotPrefab, m_sceneManager.sceneRoot);

            var entity = go.GetComponent<Entity>();
            entity.Initialize();
            entity.sceneRoot = m_sceneManager.sceneRoot;

            var robot = go.GetComponent<Robot>();
            robot.floor = m_floorGrid;
            robot.proximityModel = ProximityLookupTable.defaultInstance;
            robot.floorSensorNoise = robot.proximitySensorNoise = new NormalNoise(0.0f, 1.0f);
            robot.lightSensor.lighting = m_sceneManager.lightManager;

            go.GetComponent<RobotColor>().Initialize(m_colorSettings);

            return robot;
        }
    }
}
