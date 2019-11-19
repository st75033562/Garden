using System;
using System.Collections.Generic;

public enum TaskCategory
{
    BL1,
    BL2,
    BL3,
    BL4,
    BL5,
    OTHERS,
    ALL,
}

public enum TaskTemplateType
{
    User,
    System,
}

public class TaskTemplate
{
    public string id;
    public string name;
    public string description;
    public TaskTemplateType type;
    public DateTime createTime;
    public DateTime updateTime;
    public string projectName;

    private TaskCategory m_level;
    private K8_Attach_Info m_attachs;

    public TaskCategory level
    {
        get { return m_level; }
        set
        {
            if (value == TaskCategory.ALL)
            {
                throw new ArgumentOutOfRangeException("value");
            }
            m_level = value;
        }
    }

    public K8_Attach_Info attachs {
        get { return m_attachs; }
        set {
            m_attachs = value;
            foreach(var at in m_attachs.AttachList.Values) {
                if(at != null && at.AttachFiles != null && at.AttachFiles.FileList_ != null) {
                    foreach(var fileNode in at.AttachFiles.FileList_) {
                        if(fileNode.PathName.Contains("/")) {
                            fileNode.PathName = fileNode.PathName.Substring(fileNode.PathName.LastIndexOf('/') + 1);
                        }
                    }
                }
            }
        }
    }

    public Project codeProject = new Project();

    public TaskTemplate() { }

    public TaskTemplate(TaskTemplate rhs)
    {
        id = rhs.id;
        name = rhs.name;
        description = rhs.description;
        projectName = rhs.projectName;
        type = rhs.type;
        m_level = rhs.m_level;
        createTime = rhs.createTime;
        updateTime = rhs.updateTime;
        codeProject = new Project(rhs.codeProject);
        attachs = rhs.attachs;
    }
}
