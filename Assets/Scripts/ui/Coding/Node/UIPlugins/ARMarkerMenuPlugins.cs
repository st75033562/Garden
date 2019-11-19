public class ARMarkerMenuPlugins : DownMenuPlugins
{
	public override void Clicked()
	{
		SetMenuItems(CodeContext.arSceneManager.GetMarkerIds());
		base.Clicked();
	}
}
