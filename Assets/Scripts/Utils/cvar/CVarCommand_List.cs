using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CVarCommand_List : IVarCommand
{
    public string Execute(string[] args)
    {
        return string.Join("\n", CVar.VarNames.OrderBy(x => x).ToArray());
    }
}
