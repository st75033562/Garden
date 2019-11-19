using System;
using System.IO;
using Google.Protobuf;

public class CodeSession
{
    private static readonly A8.Logger s_logger = A8.Logger.GetLogger<CodeSession>();

    private const string SessionFileName = "session";

    private static string s_sessionFilePath;

    private readonly string m_workingDir;
    private readonly Project m_proj;

    public CodeSession(string workingDir, Project proj)
    {
        if (workingDir == null)
        {
            throw new ArgumentNullException("workingDir");
        }
        if (proj == null)
        {
            throw new ArgumentNullException("proj");
        }

        m_workingDir = workingDir;
        m_proj = proj;
    }

    public string workingDirectory { get { return m_workingDir; } }

    public Project project { get { return m_proj; } }

    public static void Init(string dir)
    {
        if (string.IsNullOrEmpty(dir))
        {
            throw new ArgumentException("dir");
        }

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        s_sessionFilePath = Path.Combine(dir, SessionFileName);
    }

    public static CodeSession Load()
    {
        try
        {
            if (File.Exists(s_sessionFilePath))
            {
                var saveData = Save_CodeSession.Parser.ParseFrom(File.ReadAllBytes(s_sessionFilePath));
                var project = new Project();
                project.name = saveData.Name;
                project.code = saveData.Code.ToByteArray();
                project.leaveMessageData = saveData.Messages.ToByteArray();
                return new CodeSession(saveData.WorkingDirectory, project);
            }
        }
        catch (Exception e)
        {
            s_logger.LogException(e);
        }

        return null;
    }

    public static void Save(CodeSession session)
    {
        if (session == null)
        {
            throw new ArgumentNullException("session");
        }

        var saveData = new Save_CodeSession();
        saveData.WorkingDirectory = session.workingDirectory;
        saveData.Name = session.project.name;
        saveData.Code = ByteString.CopyFrom(session.project.code);
        saveData.Messages = ByteString.CopyFrom(session.project.leaveMessageData);

        File.WriteAllBytes(s_sessionFilePath, saveData.ToByteArray());
    }

    public static void Delete()
    {
        FileUtils.safeDelete(s_sessionFilePath);
    }
}
