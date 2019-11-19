public class ThreeDObjectMenuPlugins : ObjectMenuPluginsBase
{
    protected override IObjectResourceDataSource objectDataSource
    {
        get { return CodeContext.threeDObjectDataSource; }
    }
}
