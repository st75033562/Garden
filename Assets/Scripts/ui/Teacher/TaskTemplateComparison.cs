using System;

public enum TaskTemplateSortType
{
    Name,
    CreateTime,
    UpdateTime
}

public static class TaskTemplateComparison
{
    public static Comparison<TaskTemplate> Get(TaskTemplateSortType type, bool asc)
    {
        Comparison<TaskTemplate> comp = null;
        switch (type)
        {
            case TaskTemplateSortType.CreateTime:
                comp = (x, y) => x.createTime.CompareTo(y.createTime);;
                break;

            case TaskTemplateSortType.Name:
                comp = (x, y) => string.Compare(x.name, y.name, StringComparison.CurrentCultureIgnoreCase);
                break;

            case TaskTemplateSortType.UpdateTime:
                comp = (x, y) => x.updateTime.CompareTo(y.updateTime);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
}
