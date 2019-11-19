public class AppTicker : Singleton<AppTicker>
{
    public void Update()
    {
        UIInputContext.RegisterPendingContexts();
    }

    public void LateUpdate()
    {
        SizeOnChild.Layout();
        LogicTransform.FlushPendingUpdates();
    }
}
