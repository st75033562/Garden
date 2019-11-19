using UnityEngine;
public class TaskPoolCell : BaseTaskPoolCell
{
    [SerializeField]
    private GameObject markGo;

    private UITeacherTaskPool teacherTaskPool;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        teacherTaskPool = (UITeacherTaskPool)controller.context;
        markGo.SetActive(teacherTaskPool.isEditing);
    }

    public void OnClickCreate()
    {
        ((UITeacherTaskPool)controller.context).ClickCreate();
    }

    public void OnClickCell()
    {
        switch (teacherTaskPool.curOperation)
        {
        case TaskOperationType.NONE:
            OnClickEditor();
            break;

        case TaskOperationType.DELETE:
            OnClickDelete();
            break;

        case TaskOperationType.PUBLISH:
                teacherTaskPool.OnClickRelease(template);
            break;

        case TaskOperationType.UPLOADSYS:
                teacherTaskPool.UpLoadToSystem(template);
            break;
        case TaskOperationType.SELECT:
                SelectCell();
                break;
        }
    }

    void OnClickEditor()
    {
        teacherTaskPool.ClickEditor(template);
    }

    void OnClickDelete()
    {
        PopupManager.YesNo("ui_confirm_delete".Localize(), ConfimDelete);
    }

    void ConfimDelete()
    {
        var request = new DeleteRequest();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            request.type = GetCatalogType.TEACHER_TASK;
        } else {
            request.type = GetCatalogType.TEACHER_TASK_PY;
        }
        request.userId = UserManager.Instance.UserId;
        request.basePath = template.id;

        request.Success((t) => {
            teacherTaskPool.ChangeList(TaskPoolOperation.Remove, template);
        })
            .Execute();
    }

    

    void SelectCell() {
        string path;
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            path = HttpCommon.c_tasktemplateV3 + UserManager.Instance.UserId + "/" + template.id ;
        } else {
            path = HttpCommon.c_tasktemplatePyV3 + UserManager.Instance.UserId + "/" + template.id;
        }
        path += "/";
        var request = new SingleFileDownload();
        request.fullPath = path + RequestUtils.Base64Encode(TaskCommon.TaskFileName);
        request.defaultErrorHandling = false;
        request.Success(data => {
            A8_Task_Info a8TaskInfo = A8_Task_Info.Parser.ParseFrom(data);
            TaskInfo taskInfo = new TaskInfo();
            taskInfo.SetValue(a8TaskInfo);
            teacherTaskPool.SelectCell(taskInfo, path);
        })
            .Execute();
    }
}
