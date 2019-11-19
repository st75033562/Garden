using System.Collections;
using System.Collections.Generic;

public class Project : IEnumerable<FileData>
{
    private string m_name;

    public byte[] code;
    public byte[] leaveMessageData;

    public Project() { this.name = ""; }

    public Project(string name)
    {
        this.name = name;
    }

    public Project(Project rhs)
    {
        name = rhs.name;
        code = rhs.code;
        leaveMessageData = rhs.leaveMessageData;
    }

    public virtual string name
    {
        get { return m_name; }
        set
        {
            m_name = value ?? string.Empty;
        }
    }

    public virtual IEnumerator<FileData> GetEnumerator()
    {
        if (code != null)
        {
            yield return new FileData(CodeProjectRepository.ProjectFileName, code);
        }
        if (leaveMessageData != null)
        {
            yield return new FileData(CodeProjectRepository.LeaveMessageFileName, leaveMessageData);
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}