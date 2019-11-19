using System.Collections.Generic;
using System.Collections;

public class FileCollection : IEnumerable<FileData>
{
    private readonly List<FileData> m_files = new List<FileData>();
    private string m_name = "";

    public string name
    {
        get { return m_name; }
        set { m_name = value ?? ""; }
    }

    public FileCollection() { }

    public FileCollection(string name)
    {
        this.name = name;
    }

    public FileCollection(FileCollection rhs)
    {
        name = rhs.name;
        m_files.AddRange(rhs.m_files);
    }

    public void Add(FileData file)
    {
        m_files.Add(file);
    }

    public void Add(string fname, byte[] data)
    {
        Add(new FileData(fname, data));
    }

    public List<FileData> files { get { return m_files; } }

    public bool Contains(string fname)
    {
        return m_files.FindIndex(x => x.filename == fname) != -1;
    }

    public FileData Get(string fname)
    {
        return m_files.Find(x => x.filename == fname);
    }

    public void Clear()
    {
        m_files.Clear();
    }

    public IEnumerator<FileData> GetEnumerator()
    {
        return m_files.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
