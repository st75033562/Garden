using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class CVarCommand_Set : IVarCommand
{
    public string Execute(string[] args)
    {
        if (args.Length < 2)
        {
            throw new ArgumentException("not enough arguments");
        }
        var cvar = CVar.Find(args[0]);
        if (cvar == null)
        {
            throw new ArgumentException("invalid variable: " + args[0]);
        }

        cvar.stringValue = args[1];
        return null;
    }
}
