public class GetVerificationCodeRequest :
    CmdHttpRequest<CMD_Get_Verification_Code_r_Parameters, CMD_Get_Verification_Code_a_Parameters>
{
    public override Command_ID cmdId
    {
        get { return Command_ID.CmdGetVerificationCodeR; }
    }
}
