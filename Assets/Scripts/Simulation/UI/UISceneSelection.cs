using LitJson;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class SceneInfo
    {
        public string name;
    }

    public class UISceneSelection : MonoBehaviour
    {
        public RectTransform content;
        public UISceneCell template;
        public event Action<GameObject> onSelectScene;

        void Start()
        {
            var data = Resources.Load<TextAsset>("Data/sim_scenes").text;
            var scenes = JsonMapper.ToObject<List<SceneInfo>>(data);

            foreach (var scene in scenes)
            {
                var cell = Instantiate(template.gameObject).GetComponent<UISceneCell>();
                cell.gameObject.SetActive(true);
                cell.Init(scene);
                cell.transform.SetParent(content, false);
            }
        }

        public void Enable(string sceneName, bool disableOthers = true)
        {
            foreach (Transform child in content.transform)
            {
                UISceneCell cell = child.gameObject.GetComponent<UISceneCell>();
                if (cell.data != null && cell.data.name == sceneName)
                {
                    cell.interactable = true;
                }
                else if (disableOthers)
                {
                    cell.interactable = false;
                }
            }
        }

        public GameObject GetCellByName(string name)
        {
            GameObject go = null;
            foreach (Transform child in content.transform)
            {
                UISceneCell cell = child.gameObject.GetComponent<UISceneCell>();
                if (cell.data != null && cell.data.name == name)
                {
                    return cell.gameObject;
                }
            }
            return go;
        }

        internal void CellClick(GameObject go)
        {
            if (onSelectScene != null)
                onSelectScene(go);
        }
    }
}
