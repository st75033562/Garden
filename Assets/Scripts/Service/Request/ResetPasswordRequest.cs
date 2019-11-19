public class ResetPasswordRequest : CmdHttpRequest<CMD_Reset_Password_r_Parameters, CMD_Reset_Password_a_Parameters>
{
    public override Command_ID cmdId
    {
        get { return Command_ID.CmdResetPasswordR; }
    }
}
