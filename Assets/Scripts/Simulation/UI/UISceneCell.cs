using UnityEngine;
using UnityEngine.UI;

namespace RobotSimulation
{
    public class UISceneCell : MonoBehaviour
    {
        public Simulator simulator;
        public UISceneSelection controller;
        public Image sceneImage;

        public bool interactable { get; set; }

        void Awake()
        {
            interactable = true;
        }

        public void Init(SceneInfo data)
        {
            this.data = data;
            sceneImage.sprite = Resources.Load<Sprite>("Simulation/Scenes/" + data.name);
        }

        public SceneInfo data
        {
            get;
            private set;
        }

        public void OnClick()
        {
            if(!interactable)
                return;
            simulator.LoadScene(data.name);
            controller.CellClick(gameObject);
            controller.gameObject.SetActive(false);
        }
    }
}
