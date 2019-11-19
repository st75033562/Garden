using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PeriodAnswer : MonoBehaviour {
    public ScrollLoopController scroll;
    public GameObject[] stars;
    public Text countText;

    private Period_Item_Info periodItem;
    public void SetData(int startCount, Period_Item_Info periodItem, List<GBAnswer> starAnswer) {
        countText.text = string.Format("ui_text_brackets".Localize(), starAnswer.Count);
        this.periodItem = periodItem;
        gameObject.SetActive(true);
        scroll.context = this;
        scroll.initWithData(starAnswer);

        
        for(int i = 0; i< stars.Length; i++) {
            if(i < startCount)
                stars[i].SetActive(true);
            else
                stars[i].SetActive(false);
        }
    }


    public void OnClickCell(PublishAnswerCell answerCell) {
        GBAnswer answer = (GBAnswer)answerCell.DataObject;
        int popupId = 0;
        popupId = PopupManager.GameboardPlayer(
            ProjectPath.Remote(periodItem.GbInfo.ProjPath),
            new[] { answer.ToRobotCodeInfo() },
            (mode, result) => {
                PopupManager.Close(popupId);
            }, gameboardModifier:(gameBoard)=> {
                var groups = gameBoard.GetCodeGroups(Preference.scriptLanguage);
                foreach (var g in groups)
                {
                    groups.ChangeGroupPath(g.projectPath, Gameboard.ProjectUrl.ToRemote(answer.ProjPath));
                }
            });
    }
    
    public void OnClickOpenCode(PublishAnswerCell answerCell) {
        GBAnswer answer = (GBAnswer)answerCell.DataObject;
        var request = new ProjectDownloadRequest();
        request.basePath = answer.ProjPath;
        request.blocking = true;
        request.Success(tRt => {
            if(Preference.scriptLanguage == ScriptLanguage.Python) {
                string mainScripath = null;
                for(int i = 0; i < tRt.FileList_.Count; ++i) {
                    FileNode tCurFile = tRt.FileList_[i];
                    string fullPath = FileUtils.appTempPath + "/" + tCurFile.PathName;
                    if((FN_TYPE)tCurFile.FnType == FN_TYPE.FnDir) {
                        FileUtils.createParentDirectory(fullPath);
                    } else {
                        FileUtils.createParentDirectory(fullPath);
                        mainScripath = fullPath;
                        File.WriteAllBytes(fullPath, tCurFile.FileContents.ToByteArray() ?? new byte[0]);
                    }
                }
                PythonEditorManager.instance.Open(mainScripath);
            } else {
                var project = tRt.ToProject();
                if(project != null) {
                    PopupManager.Workspace(CodeSceneArgs.FromCode(project));
                } else {
                    Debug.LogError("project didn't download data");
                }
            }
        })
            .Execute();
    }

    public void OnClickClose() {
        gameObject.SetActive(false);
    }
}
