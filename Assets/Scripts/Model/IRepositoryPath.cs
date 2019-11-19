using System;

public interface IRepositoryPath : IComparable<IRepositoryPath>, IEquatable<IRepositoryPath>
{
    bool isDir { get; }

    bool isFile { get; }

    bool isLogical { get; }

    IRepositoryPath logicalPath { get; }

    /// <summary>
    /// return the logical name
    /// </summary>
    string name { get; }

    /// <summary>
    /// return the raw, unencoded name
    /// </summary>
    string rawName { get; }

    /// <summary>
    /// return the directory depth, counting from 0
    /// </summary>
    int depth { get; }

    IRepositoryPath AppendFile(string name);

    IRepositoryPath AppendLogicalDir(string name);

    /// <summary>
    /// return the string representation
    /// </summary>
    string ToString();

    /// <summary>
    /// return the parent path
    /// </summary>
    IRepositoryPath parent { get; }

    /// <summary>
    /// return the corresponding repository
    /// </summary>
    ProjectRepository repository { get; }
}