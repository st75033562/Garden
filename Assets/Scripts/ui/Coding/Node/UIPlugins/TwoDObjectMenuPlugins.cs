public class TwoDObjectMenuPlugins : ObjectMenuPluginsBase
{
    protected override IObjectResourceDataSource objectDataSource
    {
        get { return CodeContext.twoDObjectDataSource; }
    }
}
