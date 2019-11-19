public class PythonFolderNameValidator : ProjectNameValidator
{
    public PythonFolderNameValidator(string currentPath, string currentName, PythonRepository repo)
        : base(currentPath, currentName, repo)
    {
    }

    public override string ValidateInput(string value)
    {
        var error = base.ValidateInput(value);
        if (error != null)
        {
            return error;
        }

        if (value.TrimEnd().EndsWith(".py"))
        {
            return "ui_python_folder_invalid_suffix".Localize();
        }

        return null;
    }
}