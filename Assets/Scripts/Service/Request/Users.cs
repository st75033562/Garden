public static class Users
{
    public static GetVerificationCodeRequest
        GetVerificationCode(string phoneNumber, string invitationCode, bool register)
    {
        var request = new GetVerificationCodeRequest();
        request.argument.CellphoneNum = phoneNumber;
        if (!string.IsNullOrEmpty(invitationCode))
        {
            request.argument.InviteCode = invitationCode;
        }
        request.argument.VcType = (uint)(register ? 0 : 1);
        return request;
    }

    public static ResetPasswordRequest ResetPassword(string password, string phoneNumber, string code)
    {
        var request = new ResetPasswordRequest();
        request.argument.NewPassword = password;
        request.argument.CellphoneNum = phoneNumber;
        request.argument.VerificationCode = code;
        return request;
    }
}
