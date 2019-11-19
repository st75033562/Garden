using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum AdapationType {
    ModifyColumns,
    ModifyColumnsCenter,
    FixedColumns,
    FixedColumnsCenter
}
public class ScrollLoopController : UIBehaviour {
    public Vector2 CellSize = new Vector2(50, 50);
    public Vector2 cellOffset;

    [SerializeField]
    private ScrollCell cellPrefab;
    [SerializeField]
    private int numberOfColumns = 1;  //表示并排显示几个，比如是上下滑动，当此处为2时表示一排有两个cell -1表示系统自动判断个数
    [SerializeField]
    private AdapationType adapationType = AdapationType.ModifyColumns;

    private ScrollRect scrollRect;
    private int visibleCellsRowCount;
    private int visibleCellsTotalCount;
    private int preFirstVisibleIndex ;
    private int firstVisibleIndex;
    private IList allData;
    private Vector2 initCellSize ;
    private Vector2 initCellOffset;
    private Vector2 viewportSize;
    private bool effective = true;
    private bool initialized;
    private bool scrollPositionChanged;

    private LinkedList<ScrollCell> localCellsPool = new LinkedList<ScrollCell>();
    private LinkedList<ScrollCell> cellsInUse = new LinkedList<ScrollCell>();

    private bool resized;
    private bool started;

    private bool horizontal {
        get { return scrollRect.horizontal; }
    }

    public object context { set; get; }

    protected override void Awake()
    {
        init();
    }

    protected override void Start()
    {
        started = true;
    }

    public void initWithData(IList cellDataList, bool keepPosition = true) {
        if(cellDataList == null)
            return;

        init();
        preFirstVisibleIndex = firstVisibleIndex = 0;
        allData = cellDataList;
		selectionModel.Reset(new SimpleListItemModel(allData));

        if (!keepPosition) {
            normalizedPosition = new Vector2(0, 1);
        }

        if (started) {
            initLayoutStates();
            AdjustCellPoolSize();
            refresh();
        }
    }

    private void init()
    {
        if(!initialized) {
            initialized = true;

            initCellSize = CellSize;
            initCellOffset = cellOffset;

            scrollRect = gameObject.GetComponent<ScrollRect>();
            scrollRect.onValueChanged.AddListener(valueChange);

            selectionModel = new SelectionDataModel();
            selectionModel.onItemSelectionChanged += OnItemSelectionChanged;
        }
    }

    void initLayoutStates() {
        CellSize = initCellSize;
        cellOffset = initCellOffset;
        viewportSize = getViewRect(scrollRect.viewport);
        int axis = 0;

        if(horizontal) {
            axis = 1;
            visibleCellsRowCount = Mathf.CeilToInt(viewportSize.x / CellSize.x);
        } else {
            axis = 0;
            visibleCellsRowCount = Mathf.CeilToInt(viewportSize.y / CellSize.y);
        }

        if(adapationType == AdapationType.ModifyColumns) {
            numberOfColumns = (int)(viewportSize[axis] / CellSize[axis]);
            numberOfColumns = numberOfColumns < 1 ? 1 : numberOfColumns;
        } else if(adapationType == AdapationType.ModifyColumnsCenter) {
            numberOfColumns = (int)(viewportSize[axis] / CellSize[axis]);
            float cellWidth = CellSize[axis];
            CellSize[axis] = viewportSize[axis] / numberOfColumns;
            if(horizontal) {
                cellOffset[axis] = -CellSize[axis] / 2;
            } else {
                cellOffset[axis] = CellSize[axis] / 2;
            }
        } else if(adapationType == AdapationType.FixedColumnsCenter) {
            float cellWidth = CellSize[axis];
            CellSize[axis] = viewportSize[axis] / numberOfColumns;
            if(horizontal) {
                cellOffset[axis] = -CellSize[axis] / 2;
            } else {
                cellOffset[axis] = CellSize[axis] / 2;
            }
        }

        visibleCellsTotalCount = (visibleCellsRowCount + 1) * numberOfColumns;
    }

    Vector2 getViewRect(RectTransform rect) {
        if(rect.rect.width == 0 || rect.rect.height == 0) {
            return scrollRect.GetComponent<RectTransform>().rect.size;
        } 
        return rect.rect.size;
    }

    void setContentSize() {
        int cellOneWayCount = (int)Math.Ceiling((float)allData.Count / numberOfColumns);
        if(horizontal) {
            scrollRect.content.sizeDelta = new Vector2(cellOneWayCount * CellSize.x, scrollRect.content.sizeDelta.y);
            scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(scrollRect.horizontalNormalizedPosition);
        } else {
            scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x, cellOneWayCount * CellSize.y);
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }
    }
    
    protected override void OnRectTransformDimensionsChange() {
        if(!effective)
            return;

        resized = true;
    }

    protected override void OnDisable() {
        base.OnDisable();
        effective = false;
    }

    protected override void OnEnable() {
        base.OnEnable();
        effective = true;
    }

    void Update()
    {
        if (resized)
        {
            resized = false;
            if (allData != null)
            {
                initLayoutStates();
                AdjustCellPoolSize();
                InternalRefresh();
            }
        }

        if (scrollPositionChanged)
        {
            scrollPositionChanged = false;
            CalculateFirstVisibleIndex();
            InternalCellsUpdate();
        }
    }

    void ShowCell(int cellIndex, bool scrollingPositive, ScrollCell tempCell = null)
    {
        if(tempCell == null)       
            tempCell = GetCellFromPool(scrollingPositive);
        if(tempCell == null)
            return;
        if(cellIndex >= allData.Count) {
            tempCell.gameObject.SetActive(false);
            tempCell.DataIndex = -1;
        } else {
            PositionCell(tempCell.gameObject, cellIndex);
            tempCell.gameObject.SetActive(true);
            tempCell.init(this, allData[cellIndex], cellIndex);
            tempCell.configureCellData();
        }
    }

    void InternalCellsUpdate() {
        if(preFirstVisibleIndex != firstVisibleIndex) {
            bool scrollingPositive = preFirstVisibleIndex < firstVisibleIndex;
            int indexDelta = Mathf.Abs(preFirstVisibleIndex - firstVisibleIndex);

            int deltaSign = scrollingPositive ? +1 : -1;

            for(int i = 1; i <= indexDelta; i++)
                UpdateContent(preFirstVisibleIndex + i * deltaSign, scrollingPositive);

            preFirstVisibleIndex = firstVisibleIndex;
        }
    }

    void UpdateContent(int sliceIndex, bool scrollingPositive) {
        int index = scrollingPositive ? ((sliceIndex - 1) * numberOfColumns) + (visibleCellsTotalCount) : (sliceIndex * numberOfColumns);
        
        for(int i = 0; i < numberOfColumns; i++) {
            FreeCell(scrollingPositive);
            if(scrollingPositive) {
                ShowCell(index + i, scrollingPositive);
            } else {
                ShowCell(index + numberOfColumns - i - 1, scrollingPositive);
            }
        }
    }

    void FreeCell(bool scrollingPositive) {
        LinkedListNode<ScrollCell> cell = null;
        if(scrollingPositive) {
            cell = cellsInUse.First;
            cellsInUse.RemoveFirst();
            localCellsPool.AddLast(cell);
        } else {
            cell = cellsInUse.Last;
            cellsInUse.RemoveLast();
            localCellsPool.AddFirst(cell);
        }
    }

    ScrollCell GetCellFromPool(bool scrollingPositive) {
        if(localCellsPool.Count == 0)
            return null;
        LinkedListNode<ScrollCell> cell = localCellsPool.First;
        localCellsPool.RemoveFirst();

        if(scrollingPositive)
            cellsInUse.AddLast(cell);
        else
            cellsInUse.AddFirst(cell);
        return cell.Value;
    }

    void PositionCell(GameObject go, int index) {
        int rowMod = index % numberOfColumns;
        if(horizontal)
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(CellSize.x * (index / numberOfColumns) + cellOffset.x, -rowMod * CellSize.y + cellOffset.y);
        else
            go.GetComponent<RectTransform>().anchoredPosition = new Vector2(CellSize.x * rowMod + cellOffset.x, -(index / numberOfColumns) * CellSize.y + cellOffset.y);
    }

    void FreeCells() {
        while(cellsInUse.Count > 0) {
            cellsInUse.Last.Value.gameObject.SetActive(false);
            localCellsPool.AddLast(cellsInUse.Last.Value);
            cellsInUse.RemoveLast();
        }
    }

    void AdjustCellPoolSize() {
        FreeCells();
        int outSideCount = localCellsPool.Count - visibleCellsTotalCount;
        if(outSideCount > 0) {
            while(outSideCount > 0) {
                outSideCount--;
                LinkedListNode<ScrollCell> cell = localCellsPool.Last;
                localCellsPool.RemoveLast();
                Destroy(cell.Value.gameObject);
            }
        } else if(outSideCount < 0) {
            for(int i=0; i< -outSideCount; i++ ) {
                GameObject go = Instantiate(cellPrefab.gameObject, scrollRect.content.transform) as GameObject;
                localCellsPool.AddLast(go.GetComponent<ScrollCell>());
                go.SetActive(false);
            }
        }
    }

    public void updateCellData(System.Object data, int index = -1) {
        if(index == -1) {
            index = allData.IndexOf(data);
        }
        if(index < 0) {
            return;
        }

        var cell = GetCellByDataIndex(index);
        if (cell != null) {
            cell.DataObject = data;
        }
    }

    private ScrollCell GetCellByDataIndex(int index)
    {
        foreach(ScrollCell scrollCell in cellsInUse) {
            if(scrollCell.DataIndex == index) {
                return scrollCell;
            }
        }
        return null;
    }

    public void add(object obj) {
        insert(allData.Count , obj);
    }

    public void addRange(IList cellDataList) {
        foreach(object obj in cellDataList) {
            add(obj);
        }
    }

    public void insert(int index, object obj) {
        allData.Insert(index, obj);
        selectionModel.OnItemInserted(new Range(index, 1));
        setContentSize();

        if(firstVisibleIndex * numberOfColumns + visibleCellsTotalCount > allData.Count - 1) {
            ShowCell(allData.Count - 1, true);
        }else if(index < firstVisibleIndex * numberOfColumns + visibleCellsTotalCount) {
            LinkedListNode<ScrollCell> clInsert = null;
            LinkedListNode<ScrollCell> cl = cellsInUse.First;
            while(cl != null) {
                if(cl.Value.DataIndex >= index) {
                    if(cl.Value.DataIndex == index)
                        clInsert = cl;
                    PositionCell(cl.Value.gameObject, cl.Value.DataIndex + 1);
                    cl.Value.DataIndex = cl.Value.DataIndex + 1;
                }
                cl = cl.Next;
            }
            if(clInsert == null) {
                clInsert = cellsInUse.First;
            }
            int showIndex = index > firstVisibleIndex * numberOfColumns ? index : firstVisibleIndex * numberOfColumns;
            if(localCellsPool.Count == 0) {
                ScrollCell cell = cellsInUse.Last.Value;

                cellsInUse.AddBefore(clInsert, cell);
                cellsInUse.RemoveLast();

                ShowCell(showIndex, true, cell);
            } else {
                ShowCell(showIndex, true);
            }
        } 
    }


    public void remove(object obj) {
        int index = allData.IndexOf(obj);
        if(index < 0) {
            return;
        }
        removeAt(index);
    }
    public void removeAt(int index) {
        if(index < 0 || allData.Count <= 0)
            return;
        allData.RemoveAt(index);
        selectionModel.OnItemRemoved(index);
        setContentSize();

        if(index < firstVisibleIndex * numberOfColumns + visibleCellsTotalCount) {

            ScrollCell cell = null;
            foreach(ScrollCell cl in cellsInUse) {
                if(cl.DataIndex == index) {
                    cell = cl;
                } else if(cl.DataIndex > index) {
                    PositionCell(cl.gameObject, cl.DataIndex - 1);
                    cl.DataIndex = cl.DataIndex - 1;
                }
            }
            if(cell == null) {
                cell = cellsInUse.First.Value;
            }

            cellsInUse.Remove(cell);
            localCellsPool.AddLast(cell);

            ShowCell(firstVisibleIndex * numberOfColumns + visibleCellsTotalCount - 1, true);
        }
    }

    public void clear() {
        allData.Clear();
        refresh();
    }

    public void refresh() {
        selectionModel.ResetSelections();
        InternalRefresh();
    }

    private void InternalRefresh()
    {
        FreeCells();
        setContentSize();

        CalculateFirstVisibleIndex();
        preFirstVisibleIndex = firstVisibleIndex;

        for(int i = 0; i < visibleCellsTotalCount; i++) {
            ShowCell(i + firstVisibleIndex * numberOfColumns, true);
        }
        scrollPositionChanged = false;
        resized = false;
    }

    void valueChange(Vector2 value) {
        scrollPositionChanged = true;
    }

    private void CalculateFirstVisibleIndex()
    {
        if (allData == null) {
            return;
        }
        preFirstVisibleIndex = firstVisibleIndex;

        int totalColumns = Mathf.CeilToInt(allData.Count / (float)numberOfColumns);
        float columnsNormal = 1.0f / (totalColumns - visibleCellsRowCount);

        if(horizontal)
        {
            firstVisibleIndex = (int)(scrollRect.horizontalNormalizedPosition / columnsNormal);
        }
        else
        {
            firstVisibleIndex = (int)((1 - scrollRect.verticalNormalizedPosition) / columnsNormal);
        }

        int limit = Mathf.Max(1, totalColumns - visibleCellsRowCount);
        firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, limit - 1);
    }

    public void reset() {
        FreeCells();
        if (scrollRect)
        {
            scrollRect.normalizedPosition = Vector2.zero;
        }
        preFirstVisibleIndex = 0;
        firstVisibleIndex = 0;
    }

    public SelectionDataModel selectionModel
    {
        get;
        private set;
    }

    private void OnItemSelectionChanged(int index, bool selected)
    {
        var visibleIndex = index - firstVisibleIndex * numberOfColumns;
        if (visibleIndex >= 0 && visibleIndex < visibleCellsTotalCount)
        {
            GetCellByDataIndex(index).configureCellData();
        }
    }

    public LinkedList<ScrollCell> GetCellsInUse() {
        return cellsInUse;
    }

    public Vector2 normalizedPosition {
        set {
            scrollRect.normalizedPosition = value;
        }
        get {
            return scrollRect.normalizedPosition;
        }
    }
}
