using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShareSceneController : MonoBehaviour {
    [SerializeField]
    private Canvas rootCanvas;

	// Use this for initialization
	void Start () {
		
	}
	
    public void OnClickGameBoard() {
        //rootCanvas.enabled = false;
        PopupManager.GameBoardShare(()=> {
            rootCanvas.enabled = true;
        });
    }

    public void OnClickVideo() {
        PopupManager.VideoShare();
    }

    public void OnClickBack() {
        SceneDirector.Push("Lobby");
    }
}
