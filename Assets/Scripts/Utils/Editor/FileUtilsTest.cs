using NUnit.Framework;

public class FileUtilTest
{
    [Test]
    public void IsParentPathOfSelf_ShouldReturnTrue()
    {
        Assert.IsTrue(FileUtils.isParentPath("a", "a"));
    }

    [Test]
    public void TrailingSlashDoesNotChangeResultOfIsParentPath()
    {
        Assert.IsTrue(FileUtils.isParentPath("a/", "a/"));
        Assert.IsTrue(FileUtils.isParentPath("a", "a/"));
        Assert.IsTrue(FileUtils.isParentPath("a/", "a"));

        Assert.IsFalse(FileUtils.isParentPath("b/", "a"));
        Assert.IsFalse(FileUtils.isParentPath("b", "a/"));
        Assert.IsFalse(FileUtils.isParentPath("b/", "a/"));
    }

    [Test]
    public void TestIsParentPath()
    {
        Assert.IsFalse(FileUtils.isParentPath("a", "ab"));
        Assert.IsFalse(FileUtils.isParentPath("ab", "a"));
        Assert.IsFalse(FileUtils.isParentPath("abc/a", "a"));

        Assert.IsTrue(FileUtils.isParentPath("abc", "abc/a"));
    }
}
