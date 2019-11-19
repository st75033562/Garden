using NUnit.Framework;

public class MathUtilsTest
{
    [Test]
    public void LeftNumberOfDigits()
    {
        Assert.AreEqual(0, MathUtils.LeftNumberOfDigits(0, 10));
        Assert.AreEqual(0, MathUtils.LeftNumberOfDigits(12345, 0));
        Assert.AreEqual(0, MathUtils.LeftNumberOfDigits(-12345, 0));
        Assert.AreEqual(123, MathUtils.LeftNumberOfDigits(12345, 3));
        Assert.AreEqual(-123, MathUtils.LeftNumberOfDigits(-12345, 3));
        Assert.AreEqual(int.MinValue, MathUtils.LeftNumberOfDigits(int.MinValue, 100));
        Assert.AreEqual(int.MaxValue, MathUtils.LeftNumberOfDigits(int.MaxValue, 100));
    }

    [Test]
    public void NumberOfDigits()
    {
        Assert.AreEqual(1, MathUtils.NumberOfDigits(0));
        Assert.AreEqual(1, MathUtils.NumberOfDigits(1));
        Assert.AreEqual(1, MathUtils.NumberOfDigits(-1));
        Assert.AreEqual(3, MathUtils.NumberOfDigits(123));
        Assert.AreEqual(3, MathUtils.NumberOfDigits(-123));
    }
}
