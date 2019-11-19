public static class VariableManagerExtensions
{
    public static void setVar(this VariableManager manager, string name, float value)
    {
        var varData = manager.get<VariableData>(name);
        if (varData != null)
        {
            varData.setValue(value);
        }
    }

    public static int getVarInt(this VariableManager manager, string name, int defaultValue = 0)
    {
        var variable = manager.get<VariableData>(name);
        return variable != null ? (int)variable.getValue() : defaultValue;
    }
}
