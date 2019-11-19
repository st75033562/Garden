using System;

public enum PathInfoMember
{
    CreateTime,
    UpdateTime,
    Name
}

public class PathInfoComparer : BaseComparer<PathInfo>
{
    private readonly PathInfoMember m_member;
    private readonly bool m_asc;

    public PathInfoComparer(PathInfoMember member, bool asc)
    {
        m_member = member;
        m_asc = asc;
    }

    protected override int DoCompare(PathInfo x, PathInfo y)
    {
        var res = x.path.isFile.CompareTo(y.path.isFile);
        if (res != 0)
        {
            return res;
        }

        switch (m_member)
        {
        case PathInfoMember.CreateTime:
            res = x.creationTime.CompareTo(y.creationTime);
            break;

        case PathInfoMember.UpdateTime:
            res = x.updateTime.CompareTo(y.updateTime);
            break;

        case PathInfoMember.Name:
            res = string.Compare(x.path.name, y.path.name, StringComparison.OrdinalIgnoreCase);
            break;

        default:
            throw new ArgumentOutOfRangeException();
        }

        if (res == 0 && m_member != PathInfoMember.Name)
        {
            res = string.Compare(x.path.name, y.path.name, StringComparison.OrdinalIgnoreCase);
        }

        return m_asc ? res : -res;
    }
}
