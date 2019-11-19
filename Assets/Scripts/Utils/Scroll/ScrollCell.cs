using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollCell : MonoBehaviour {
    private ScrollLoopController scrollController;
    private System.Object dataObject;
    private int dataIndex = -1;

    public System.Object DataObject {
        get { return dataObject; }
        set {
            dataObject = value;
            configureCellData();
        }
    }

    public int DataIndex
    {
        get { return dataIndex; }
        set
        {
            if (dataIndex != value)
            {
                dataIndex = value;
                dataIndexChanged();
            }
        }
    }

    public object Context {
        get { return scrollController.context; }
    }

    public void init(ScrollLoopController controller, System.Object data, int index) {
        this.scrollController = controller;
        dataObject = data;
        dataIndex = index;
    }

    public bool IsSelected {
        get { return scrollController.selectionModel.IsSelected(dataIndex); }
        set { scrollController.selectionModel.Select(dataIndex, value); }
    }

    public virtual void configureCellData() { }

    protected virtual void dataIndexChanged() { }
}
