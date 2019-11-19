using UnityEngine;

public class AttachmentCellData
{
    public LocalResData resData;
    public bool deletable;
}

public class AttachmentCell : AttachmentCellBase
{
    public GameObject m_addResGo;
    public GameObject m_contentGo;
    public GameObject m_closeButton;

    public override void ConfigureCellData()
    {
        var cellData = (AttachmentCellData)DataObject;
        if (cellData.resData == null)
        {
            m_addResGo.SetActive(true);
            m_contentGo.SetActive(false);
        }
        else
        {
            m_addResGo.SetActive(false);
            m_contentGo.SetActive(true);
            base.ConfigureCellData();
        }
        m_closeButton.SetActive(cellData.deletable);
    }

    protected override LocalResData resourceData
    {
        get
        {
            return ((AttachmentCellData)DataObject).resData;
        }
    }
}
