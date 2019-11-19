using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataAccess;

public class UIARObjectDialog : UIInputDialogBase
{
    public ScrollableAreaController m_scrollController;
    public SimpleTabWidget m_tabWidget;

    private UserARObjects m_objects;
    private ARObjectService m_service;
    private IDialogInputCallback m_callback;

    protected override void Awake()
    {
        base.Awake();
        m_tabWidget.onTabChanged.AddListener(OnTabChanged);
    }

    public void Configure(UserARObjects objects, ARObjectService service, IDialogInputCallback callback)
    {
        if (objects == null)
        {
            throw new ArgumentNullException("objects");
        }
        if (service == null)
        {
            throw new ArgumentNullException("service");
        }

        m_objects = objects;
        m_service = service;
        m_callback = callback;

        m_tabWidget.SetTabs(m_service.GetCategories().Select(x => x.name.Localize()));
        m_tabWidget.activeTabIndex = 0;
    }

    void OnTabChanged(int index)
    {
        int categoryId = m_service.GetCategories().First(x => x.order == index).id;
        var itemsData = m_service.GetObjectsByCategory(categoryId)
                                 .Select(x => new UIARObjectItemData {
                                     objectData = x,
                                     unlocked = m_objects.IsUnlocked(x.id)
                                 })
                                 .ToArray();
        m_scrollController.InitializeWithData(itemsData);
    }

    public void OnClickItem(UIARObjectItem item)
    {
        var objectId = item.data.objectData.id;
        if (item.data.unlocked)
        {
            if (m_callback != null)
            {
                m_callback.InputCallBack(objectId.ToString());
            }
            CloseDialog();
        }
        else
        {
            PopupManager.Purchase("ui_ar_object_unlock_hint".Localize(item.data.objectData.price),
                                  item.data.objectData.price,
                                  "ui_ar_object_unlock_button_text".Localize(),
                                  () => {
                                      UnlockObject(item.data);
                                  });
        }
    }

    private void UnlockObject(UIARObjectItemData item)
    {
        var maskId = PopupManager.ShowMask();
        m_service.Unlock(item.objectData.id, success => {
            if (success)
            {
                m_objects.Unlock(item.objectData.id);
                item.unlocked = true;
                m_scrollController.model.updatedItem(item);
            }
            PopupManager.Close(maskId);
        });
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UIARObjectDialog; }
    }
}
