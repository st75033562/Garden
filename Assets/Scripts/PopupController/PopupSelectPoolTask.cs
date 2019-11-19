using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectPoolTaskInfo {
    public string webPath;
    public TaskInfo taskInfo;

    public SelectPoolTaskInfo(TaskInfo taskInfo, string webPath) {
        this.taskInfo = taskInfo;
        this.webPath = webPath;
    }
}
public class PopupSelectPoolTask : PopupController {
    public UITeacherTaskPool teacherTaskPool;
    protected override void Start () {
        base.Start();

        teacherTaskPool.SelectPoolCell((info)=> {
            Close();
            ((Action<SelectPoolTaskInfo>)payload)(info);
        });
    }
}
