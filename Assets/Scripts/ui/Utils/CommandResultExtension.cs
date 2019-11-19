using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public static class CommandResultExtension
{
    public static string Localize(this Command_Result code)
    {
        return ("ui_cmd_result_" + (int)code).Localize();
    }
}
