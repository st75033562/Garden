using System;

public class ProjectNameValidator : IDialogInputValidator
{
    private readonly string m_currentPath;
    private readonly string m_currentName;
    private readonly ProjectRepository m_repo;

    public ProjectNameValidator(string currentPath, string currentName, ProjectRepository repo)
    {
        if (currentPath == null)
        {
            throw new ArgumentNullException("currentPath");
        }
        if (repo == null)
        {
            throw new ArgumentNullException("repo");
        }
        m_currentPath = currentPath;
        m_currentName = currentName ?? string.Empty;
        m_repo = repo;
    }

    public virtual string ValidateInput(string value)
    {
        if (value == null)
        {
            throw new ArgumentNullException("value");
        }

        value = value.TrimEnd();

        string result = Validate(m_repo, value);
        if (result != null)
        {
            return result;
        }

        var fullPath = m_repo.getAbsPath(value);
        if (fullPath.Length > ProjectRepository.MaxPathLength)
        {
            return "file_path_too_long".Localize();
        }

        if (m_currentName == value)
        {
            return null;
        }

        if (m_repo.existsPath(m_currentPath, value))
        {
            return "name_already_in_use".Localize();
        }

        return null;
    }

    public static string Validate(ProjectRepository repo, string name)
    {
        if (repo == null)
        {
            throw new ArgumentNullException("repo");
        }

        if (name == null)
        {
            throw new ArgumentNullException("name");
        }

        if (name.Trim() == string.Empty)
        {
            return "name_white_space_only".Localize();
        }

        var result = repo.validateFileName(name);
        switch (result)
        {
        case FileNameValidationResult.InvalidChar:
            return "file_name_invalid_char".Localize();

        case FileNameValidationResult.InvalidPrefix:
            return "path_invalid_prefix".Localize();

        case FileNameValidationResult.ReservedName:
            return "file_name_reserved".Localize();

        case FileNameValidationResult.FileNameTooLong:
            return "file_name_too_long".Localize(ProjectRepository.MaxFileNameLength);
        }

        return null;
    }
}