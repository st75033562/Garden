public class PythonNameValidator : ProjectNameValidator
{
    public PythonNameValidator(string currentPath, string currentName)
        : base(currentPath, currentName, PythonRepository.instance)
    {
    }

    public override string ValidateInput(string value)
    {
        return base.ValidateInput(FileUtils.ensureExtension(value, ".py"));
    }
}
