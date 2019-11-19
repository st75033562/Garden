using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeaViewAttachCell : BaseAttachmentCell {
    public Text attachName;
    // Use this for initialization
    public override AttachData attachData {
        get {
            return ((AddAttachmentCellData)DataObject).data;
        }
    }

    public override void configureCellData() {
        attachName.text = attachData.programNickName;
        initIcon();
    }

    protected override void OpenWorkspack(Project project) {
        PopupManager.Workspace(CodeSceneArgs.FromTempCode(project));
    }

    protected override void GameboardPlayer(ProjectPath path, bool isWebGb) {
        AttachInfo attachInfo = new AttachInfo();
        attachInfo.resType = AttachInfo.ResType.GameBoard;
        AttachGb attachData = new AttachGb();

        if(Preference.scriptLanguage == ScriptLanguage.Python) {
            attachData.relations = new List<string>();
            foreach(var data in GetAttachments()) {
                if(data != null && data.type == AttachData.Type.Project && !string.IsNullOrEmpty(data.webProgramPath)) {
                    attachData.relations.Add(data.webProgramPath);
                }
            }
        }
        attachData.from = AttachGb.From.Server;
        attachData.programPath = path.path;
        attachData.webRelationPath = path.path.Substring(0, path.path.LastIndexOf("/")) + "/";
        attachInfo.gbData = attachData;
        AttachmentPreview.Preview(attachInfo);
    }

    protected override IEnumerable<AttachData> GetAttachments() {
        return ((AddAttachmentCellData)DataObject).attachments;
    }
}
