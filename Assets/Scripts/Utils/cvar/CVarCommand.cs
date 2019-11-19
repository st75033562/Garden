using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public interface IVarCommand
{
    // return response string
    string Execute(string[] args);
}
