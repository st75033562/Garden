using NUnit.Framework;

public class UtilsTest
{
    [Test]
    public void CamelCaseToUnderscore()
    {
        Assert.AreEqual("abc", Utils.CamelCaseToUnderscore("abc"));
        Assert.AreEqual("", Utils.CamelCaseToUnderscore(""));
        Assert.AreEqual("a_bc_de", Utils.CamelCaseToUnderscore("aBcDe"));
    }
}
