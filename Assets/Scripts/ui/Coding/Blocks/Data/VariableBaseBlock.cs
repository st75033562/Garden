public class VariableBaseBlock : BlockBehaviour
{
    protected bool isWritable(BaseVariable variable)
    {
        return variable.scope == NameScope.Local ||
               variable.globalVarOwner == GlobalVarOwner.All ||
               CodeContext.currentGlobalVarWriter == variable.globalVarOwner;
    }
}
