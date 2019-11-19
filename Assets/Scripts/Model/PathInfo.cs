using System;

public class PathInfo
{
    public PathInfo(IRepositoryPath path, DateTime creationTime, DateTime updateTime)
    {
        if (path == null)
        {
            throw new ArgumentNullException("path");
        }

        this.path = path;
        this.creationTime = creationTime;
        this.updateTime = updateTime;
    }

    public IRepositoryPath path { get; private set; }

    public DateTime creationTime { get; private set; }

    public DateTime updateTime { get; private set; }
}