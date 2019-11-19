using UnityEngine;
using UnityEngine.UI;

public class ProjectItemData
{
    public bool isDeleting;
    public PathInfo pathInfo;
}

public class ProjectItemView : ScrollableCell
{
	public Text textProjectName;
    public RaceLamp textAnimation;
    public Image imageIcon;
    public Sprite folderIcon;
    public Sprite fileIcon;
    public GameObject markGo;

    public override void ConfigureCellData()
    {
        markGo.SetActive(ItemData.isDeleting);
        textProjectName.text = ItemData.pathInfo.path.name;

        if (ItemData.pathInfo.path.isDir)
        {
            imageIcon.sprite = folderIcon;
        }
        else
        {
            imageIcon.sprite = fileIcon;
        }
		
        textAnimation.ResetPostion();
    }

    public ProjectItemData ItemData
    {
        get { return dataObject as ProjectItemData; }
    }

	public string ProjectPath
	{
        get { return ItemData.pathInfo.path.ToString(); }
	}
}
