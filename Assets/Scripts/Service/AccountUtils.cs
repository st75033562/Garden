using System;
using System.Text.RegularExpressions;

public static class AccountUtils
{
    public const string PhoneNumberRegex = @"^\d{11}$";
    public const string EmailRegex = @"^\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";

    public static bool IsPhoneNumber(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException("input");
        }

        return Regex.IsMatch(input, PhoneNumberRegex);
    }

    public static bool IsEmail(string input)
    {
        if (input == null)
        {
            throw new ArgumentNullException("input");
        }

        return Regex.IsMatch(input, EmailRegex);
    }
}
