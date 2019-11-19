using UnityEngine;

public class LessonCenterCotroller : MonoBehaviour {
    public Canvas m_canvas;

	// Use this for initialization
	void Start () {
        UserManager.Instance.appRunModel = AppRunModel.Normal;
	}
	
    public void OnClickOnlineCourse() {
        m_canvas.enabled = false;
        PopupManager.IntelligentLesson(() => {
            NodeTemplateCache.Instance.ShowBlockUI = false;
            m_canvas.enabled = true;
        });
    }

    public void OnClickClass() {
        m_canvas.enabled = false;
        PopupManager.MyClass(() => {
            m_canvas.enabled = true;
        });
    }

    public void OnClickback() {
        SceneDirector.Pop();
    }

    public void ClickGuide() {
        PopupManager.TwoBtnDialog(
            "ui_simulator_select".Localize(),
            "code_panel_title".Localize(),
            () => {
                UserManager.Instance.appRunModel = AppRunModel.Guide;
                UserManager.Instance.isSimulationModel = false;
                var containConnectRobot = false;
                foreach (var robot in Robomation.RobotManager.instance.robots)
                {
                    if(robot.getConnectionState() == Robomation.ConnectionState.Connected) {
                        containConnectRobot = true;
                        break;
                    }
                }
                if (!containConnectRobot)
                {
                    SceneDirector.Push("RobotManage");
                }
                else
                {
                    SceneDirector.Push("SmartClassScene");
                }
            },
            "ui_simulator".Localize(),
            () => {
                UserManager.Instance.appRunModel = AppRunModel.Guide;
                UserManager.Instance.isSimulationModel = true;
                SceneDirector.Push("SmartClassScene");
            });
    }
}
