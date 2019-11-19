using NUnit.Framework;

public class UIUtilsTest
{
    [Test]
    public void QuatifyNumber_LessThan1KReturnsOriginalValue()
    {
        Assert.AreEqual("0", UIUtils.Quantify(0, 0));
        Assert.AreEqual("999", UIUtils.Quantify(999, 0));
    }

    [Test]
    public void QuatifyNumber_LessThan1W()
    {
        Assert.AreEqual("1k", UIUtils.Quantify(1000, 10));
        Assert.AreEqual("1.123k", UIUtils.Quantify(1123, 10));
        Assert.AreEqual("1.1k", UIUtils.Quantify(1123, 4));
    }
}
