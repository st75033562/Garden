using UnityEngine;

public class SystemPoolCell : BaseTaskPoolCell
{
    [SerializeField]
    private GameObject editorGo;
    public GameObject markGo;

    private UISystemTaskPool systemTaskPool;

    public override void ConfigureCellData()
    {
        base.ConfigureCellData();

        systemTaskPool = (UISystemTaskPool)controller.context;
        editorGo.SetActive(false);
        markGo.SetActive(systemTaskPool.operationType != SysPoolOpertionType.NONE);
    }
  
    public void OnClickShowMark()
    {
        editorGo.SetActive(!editorGo.activeSelf);
    }

    public void OnClickCell() {
        switch (systemTaskPool.operationType)
        {
        case SysPoolOpertionType.NONE:
            OnClickEditor();
            break;
        case SysPoolOpertionType.DELETE:
            OnClickDelete();
            break;
        case SysPoolOpertionType.PUBLISH:
            OnClickDownload();
            break;
        }
    }

    void OnClickEditor()
    {
        systemTaskPool.ClickEditor(template);
    }

    public void OnClickDelete()
    {
        PopupManager.YesNo("ui_confirm_delete".Localize(), ConfimDelete);
    }

    void ConfimDelete()
    {
        var request = new DeleteRequest();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            request.type = GetCatalogType.SYSTEM_TASK;
        } else {
            request.type = GetCatalogType.SYSTEM_TASK_PY;
        }
        
        request.basePath = template.id;

        request.Success((t) => {
            systemTaskPool.ChangeList(TaskPoolOperation.Remove, template);
        })
            .Execute();
    }

    public void OnClickDownload()
    {
        bool duplicate = NetManager.instance.SameNameInTaskPool(template.name);
        if (duplicate)
        {
            PopupManager.Notice("repat_name_need_change".Localize(), null);
            return;
        }
        else
        {
            AddSysTemplateToSelf();
        }
    }

    void AddSysTemplateToSelf()
    {
        var copyRequest = new CopyRequest();
        if(Preference.scriptLanguage == ScriptLanguage.Visual) {
            copyRequest.rootType = FileList_Root_Type.TaskTemplateGraphy;
            copyRequest.projectSrc = HttpCommon.c_taskSystemV3 + template.id;
        } else {
            copyRequest.rootType = FileList_Root_Type.TaskTemplatePython;
            copyRequest.projectSrc = HttpCommon.c_taskSystemPyV3 + template.id;
        }
        copyRequest.desTag = ((int)template.level).ToString();
        copyRequest.desName = template.name;

        copyRequest.Success((t) => {
            var userTemplate = new TaskTemplate(template);
            userTemplate.type = TaskTemplateType.User;

            NetManager.instance.CoverOrAddToTaskPool(userTemplate);
            PopupManager.Notice("ui_download_sucess".Localize());
        })
            .Execute();
    }
}
