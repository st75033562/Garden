using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class CodeProjectRepository : ProjectRepository
{
    public const string FolderPrefix = "____";

	public const string ProjectFileName      = "main.sc";
	public const string ThumbnameFileName    = "thumbnail.tex";
	public const string LeaveMessageFileName = "Leave.msg";
    public const string SaveTempCodeName     = "saveTempCode_a8";

    private readonly VoiceRepository m_voiceRepo;

    public static CodeProjectRepository instance { get; set; }

    public CodeProjectRepository(VoiceRepository voiceRepo)
    {
        if (voiceRepo == null)
        {
            throw new ArgumentNullException();
        }
        m_voiceRepo = voiceRepo;
    }

    /// <summary>
    /// load the given project
    /// </summary>
    public Project loadCodeProject(string path, bool isTemporary = false)
    {
        if (string.IsNullOrEmpty(path))
        {
            throw new ArgumentException("empty project path");
        }

        try
        {
            if (Directory.Exists(getAbsPath(path)))
            {
                var proj = new Project();
                load(proj, path);
                return proj;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        return null;
    }

    protected void load(Project proj, string path)
    {
        proj.name = Path.GetFileName(path);
        proj.code = loadFile(path, ProjectFileName);
        proj.leaveMessageData = loadFile(path, LeaveMessageFileName);
    }

    public byte[] loadCode(string name)
    {
        return loadFile(name, ProjectFileName);
    }

    protected void saveCode(string name, byte[] data)
    {
        saveFile(name, ProjectFileName, data);
    }

    public byte[] loadMessages(string name)
    {
        return loadFile(name, LeaveMessageFileName);
    }

    protected void saveMessages(string name, byte[] data)
    {
		saveFile(name, LeaveMessageFileName, data);
    }

    protected override void onUpdatedFileStats(string path, FileStats stats)
    {
        base.onUpdatedFileStats(path, stats);

        if (path.EndsWith("/" + ProjectFileName, StringComparison.OrdinalIgnoreCase))
        {
            string folderPath = Path.GetDirectoryName(path);

            var projStats = getFileStats(folderPath);
            projStats.creationTime = stats.creationTime;
            projStats.writeTime = stats.writeTime;
        }
    }

    public void saveImage(string name, byte[] data)
	{
		saveFile(name, ThumbnameFileName, data);
	}

    /// <summary>
    /// save the given project
    /// </summary>
    public void save(Project project)
    {
        if (string.IsNullOrEmpty(project.name))
        {
            throw new InvalidOperationException("empty project name");
        }

        // TODO: report error
        saveCode(project.name, project.code);
        saveMessages(project.name, project.leaveMessageData);
    }

    protected override void doDelete(string projectName)
    {
        base.doDelete(projectName);

        // delete all voices
        var leaveMessageDataSource = new LeaveMessageDataSource();
        byte[] messageData = loadMessages(projectName);
        if (messageData == null)
        {
            return;
        }
        leaveMessageDataSource.loadMessages(messageData);
        var voiceNames = leaveMessageDataSource.messages.SelectMany(x => x.LeaveMessages)
            .Where(x => x.m_Type == LeaveMessageType.Voice)
            .Select(x => x.TextLeaveMessage);
        foreach (var voiceName in voiceNames)
        {
            m_voiceRepo.delete(voiceName);
        }
    }

    public override FileNameValidationResult validateFileName(string name)
    {
        var error = base.validateFileName(name);
        if (error != FileNameValidationResult.NoError)
        {
            return error;
        }
        if (name.StartsWith(FolderPrefix))
        {
            return FileNameValidationResult.InvalidPrefix;
        }
        return FileNameValidationResult.NoError;
    }

    public override IRepositoryPath createPath(string path, bool isFile)
    {
        return new EncodedRepositoryPath(this, path, isFile);
    }

    public override string makeDirName(string logicalName)
    {
        if (string.IsNullOrEmpty(logicalName))
        {
            throw new ArgumentException("logicalName");
        }
        return logicalName.StartsWith(FolderPrefix) ? logicalName : FolderPrefix + logicalName;
    }

    public override bool isDirName(string rawName)
    {
        if (rawName == null)
        {
            throw new ArgumentNullException("rawName");
        }
        return rawName.StartsWith(FolderPrefix);
    }

    public override string getLogicalDirName(string rawName)
    {
        if (rawName == null)
        {
            throw new ArgumentNullException("rawName");
        }

        return isDirName(rawName) ? rawName.Substring(FolderPrefix.Length) : rawName;
    }
}
