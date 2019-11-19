using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollableAreaController : UIBehaviour {
    public enum AdapationType {
        Default,
        ModifyColumns,
        Scale,
        Resize // if scroll horizontally, resize cell height, otherwise, resize cell width
    }
    [SerializeField]
    private ScrollableCell cellPrefab;
    [SerializeField]
    private int NUMBER_OF_COLUMNS = 1;  //表示并排显示几个，比如是上下滑动，当此处为2时表示一排有两个cell

    // when auto fetch begins if current scroll position >= 1.0 - autoFetchCellSize * normalized cell height 
    public float autoFetchCellSize = 0.5f;
    public float cellWidth = 30.0f;
    public float cellHeight = 25.0f;
    public float cellSpacing;
    public Vector2 cellOffset;
    public AdapationType adapationType;

    private RectTransform content;
    private int visibleCellsTotalCount = 0;
    private int visibleCellsRowCount = 0;
    private LinkedList<ScrollableCell> localCellsPool = new LinkedList<ScrollableCell> ();
    private LinkedList<ScrollableCell> cellsInUse = new LinkedList<ScrollableCell> ();
    private ScrollRect scrollRect;

    private int previousSliceIndex = 0;
    private int firstSliceIndex = 0;
    private float adjustSize;
    private CanvasScaler canvasScaler;

    private Vector3 firstCellPostion;
    private float adaptionScale = 1.0f;
    private bool hasStarted;
    private bool dimensionChanged;

    private Vector2 cellSize;
    private Vector2 adjustedCellSize;
    private Vector2 adjustedCellOffset;
    private bool initialized;
    private float autoFetchThreshold = 1.0f;
    private bool scrollPositionChanged = true;
    private bool fetchMoreItems = false;

    public object context { set; get; }

    protected override void Awake()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (initialized) { return; }
        initialized = true;

        canvasScaler = gameObject.GetComponentInParent<CanvasScaler>();
        scrollRect = this.GetComponent<ScrollRect> ();
        content = scrollRect.content;
        selectionModel = new SelectionDataModel();
        selectionModel.onItemSelectionChanged += OnItemSelectionChanged;

        adjustedCellSize = cellSize = new Vector2(cellWidth, cellHeight);
        adjustedCellOffset = cellOffset;
    }

    protected override void Start ()
    {
        scrollRect.onValueChanged.AddListener(delegate { scrollPositionChanged = true; });
        hasStarted = true;
        ComputeVisibleCellsCount();
        FillCellsPool ();
        PopulateCells();
    }

    protected override void OnEnable()
    {
        dimensionChanged = true;
    }

    private void ComputeVisibleCellsCount()
    {
        if(horizontal) {
            visibleCellsRowCount = Mathf.CeilToInt (scrollRect.viewport.rect.width / cellWidth);
        } else {
            visibleCellsRowCount = Mathf.CeilToInt (scrollRect.viewport.rect.height / cellHeight);
        }
        ApplyAdaption();
        visibleCellsTotalCount = (visibleCellsRowCount + 1) * NUMBER_OF_COLUMNS;
    }

    void ApplyAdaption ()
    {
        switch (adapationType)
        {
        case AdapationType.ModifyColumns:
            ModifyColumnsModel();
            break;
        case AdapationType.Scale:
            ScaleModel();
            break;
        case AdapationType.Resize:
            ResizeCells();
            break;
        }
    }

    void ModifyColumnsModel ()
    {
        int axis = scrollAxis ^ 1;
        float viewportSize = scrollRect.viewport.rect.size[axis];
        NUMBER_OF_COLUMNS = Mathf.FloorToInt (viewportSize / cellSize[axis]);
        adjustedCellSize[axis] = (int)(viewportSize / NUMBER_OF_COLUMNS);
        adjustedCellOffset[axis] = (adjustedCellSize[axis] - (cellSize[axis] - cellOffset[axis] * 2)) / 2;
    }

    void ScaleModel ()
    {
        float proportionWidth = Screen.width / canvasScaler.referenceResolution.x;
        float proportionHeight = Screen.height / canvasScaler.referenceResolution.y;
        if(proportionWidth > proportionHeight) {
            adaptionScale = proportionWidth / proportionHeight;
            adjustedCellSize = cellSize * adaptionScale;
        }
    }

    void ResizeCells()
    {
        int axis = scrollAxis ^ 1;
        float size = content.rect.size[axis];
        adjustedCellSize[axis] = (int)((size - (NUMBER_OF_COLUMNS - 1) * cellSpacing) / NUMBER_OF_COLUMNS);
        foreach (var cell in localCellsPool.Concat(cellsInUse))
        {
            ResizeCell(cell);
        }
    }

    protected override void OnDestroy()
    {
        UnsubscribeEvents();
    }

    void Update ()
    {
        if (model == null)
        {
            return;
        }

        if (dimensionChanged)
        {
            Resize();
            dimensionChanged = false;
        }

        if (scrollPositionChanged)
        {
            scrollPositionChanged = false;
            fetchMoreItems = true;

            previousSliceIndex = firstSliceIndex;
            CalculateCurrentIndex();
            InternalCellsUpdate();
        }

        if (fetchMoreItems)
        {
            fetchMoreItems = false;
            if (scrollPosition >= autoFetchThreshold && model.canFetchMore)
            {
                model.fetchMore();
            }
        }
    }

    private void InternalCellsUpdate ()
    {
        if(previousSliceIndex != firstSliceIndex) {
            bool scrollingPositive = previousSliceIndex < firstSliceIndex;
            int indexDelta = Mathf.Abs (previousSliceIndex - firstSliceIndex);

            int deltaSign = scrollingPositive ? +1 : -1;

            for(int i = 1; i <= indexDelta; i++)
                this.UpdateContent (previousSliceIndex + i * deltaSign , scrollingPositive);
        }
    }

    public void updateCellData(System.Object data, int index = -1)
    {
        if (index == -1)
        {
            index = model.indexOf(data);
        }
        if (index < 0)
        {
            return;
        }
        model.setItem(index, data);
    }

    private ScrollableCell GetCellByDataIndex(int index)
    {
        foreach (var cell in cellsInUse)
        {
            if (cell.DataIndex == index)
            {
                return cell;
            }
        }
        return null;
    }

    public LinkedList<ScrollableCell> GetCellsInUse() {
        return cellsInUse;
    }

    private void CalculateCurrentIndex ()
    {
        if (!horizontal) {
            firstSliceIndex = Mathf.FloorToInt (content.localPosition.y / adjustedCellSize.y);
        } else {
            firstSliceIndex = -(int)(content.localPosition.x / adjustedCellSize.x);
        }
        int limit = Mathf.CeilToInt ((float)model.count / (float)NUMBER_OF_COLUMNS) - visibleCellsRowCount;
        if (firstSliceIndex < 0) {
            firstSliceIndex = 0;
        }
        if (limit > 0 && firstSliceIndex >= limit) {
            firstSliceIndex = limit - 1;
        }
    }

    private bool horizontal {
        get { return scrollRect.horizontal; }
    }
    
    private int scrollAxis
    {
        get { return scrollRect.horizontal ? 0 : 1; }
    }

    private void FreeCell (bool scrollingPositive)
    {
        if (cellsInUse.Count == 0)
        {
            return;
        }

        LinkedListNode<ScrollableCell> cell = null;
        // Add this GameObject to the end of the list
        if(scrollingPositive) {
            cell = cellsInUse.First;
            cellsInUse.RemoveFirst ();
            localCellsPool.AddLast (cell);
        } else {
            cell = cellsInUse.Last;
            cellsInUse.RemoveLast ();
            localCellsPool.AddFirst (cell);
        }
    }

    private void UpdateContent(int sliceIndex, bool scrollingPositive)
    {
        int startIndex = scrollingPositive
                        ? (sliceIndex - 1) * NUMBER_OF_COLUMNS + visibleCellsTotalCount 
                        : (sliceIndex + 1) * NUMBER_OF_COLUMNS - 1;
        int delta = scrollingPositive ? 1 : -1;
        for(int i = 0; i < NUMBER_OF_COLUMNS; i++) {
            this.FreeCell (scrollingPositive);
            var tempCell = GetCellFromPool (scrollingPositive);
            ConfigureCell(tempCell.Value, startIndex + i * delta);
        }
    }

    private void ConfigureCell(ScrollableCell cell, int dataIndex)
    {
        PositionCell(cell, dataIndex);
        if(dataIndex >= 0 && dataIndex < model.count) {
            cell.gameObject.SetActive(true);
            cell.Init(this, model.getItem(dataIndex), dataIndex);
            cell.ConfigureCell();
        } else {
            cell.gameObject.SetActive(false);
        }
    }

    public void InitializeWithData (IList cellDataList, bool keepPosition = true)
    {
        if (cellDataList == null)
        {
            throw new ArgumentNullException();
        }

        InitializeWithData(new SimpleListItemModel(cellDataList), keepPosition);
    }

    public void InitializeWithData(IListItemModel model, bool keepPosition = true)
    {
        if (model == null)
        {
            throw new ArgumentNullException();
        }

        Initialize();
        UnsubscribeEvents();

        // make sure this is called before subscribing events since we need to
        // update cells after clearing selection state
        selectionModel.Reset(model);

        this.model = model;
        SubscribeEvents();

        if (!keepPosition)
        {
            scrollPosition = 0.0f;
        }

        if (hasStarted)
        {
            PopulateCells();
        }
    }

    private void SubscribeEvents()
    {
        this.model.onItemInserted += OnItemInserted;
        this.model.onItemRemoved += OnItemRemoved;
        this.model.onItemUpdated += OnItemUpdated;
        this.model.onReset += OnModelReset;
        this.model.onItemIndexChanged += OnItemIndexChanged;
        this.model.onItemMoved += OnItemMoved;
    }

    private void UnsubscribeEvents()
    {
        if (this.model != null)
        {
            this.model.onItemInserted -= OnItemInserted;
            this.model.onItemRemoved -= OnItemRemoved;
            this.model.onItemUpdated -= OnItemUpdated;
            this.model.onReset -= OnModelReset;
            this.model.onItemIndexChanged -= OnItemIndexChanged;
            this.model.onItemMoved -= OnItemMoved;
        }
    }

    private void PopulateCells()
    {
        if (model == null || visibleCellsRowCount == 0)
        {
            return;
        }

        if (cellsInUse.Count > 0)
        {
            FreeAllCells();
        }

        UpdateContentSize();
        firstCellPostion = new Vector3(cellOffset.x, cellOffset.y, 0) * adaptionScale;
        for (int i = 0; i < visibleCellsTotalCount; i++)
        {
            var tempCell = GetCellFromPool(true);
            if (tempCell == null || tempCell.Value == null)
                continue;
            int currentDataIndex = i + firstSliceIndex * NUMBER_OF_COLUMNS;
            ConfigureCell(tempCell.Value, currentDataIndex);
        }
    }

    private void FreeAllCells()
    {
        foreach (var cell in cellsInUse)
        {
            localCellsPool.AddLast(cell);
        }
        cellsInUse.Clear();
    }

    void UpdateContentSize ()
    {
        int sliceCount = (int)Math.Ceiling ((float)model.count / NUMBER_OF_COLUMNS);
        int axis = scrollAxis;

        content.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, sliceCount * adjustedCellSize[axis]);
        float scrollSize = content.sizeDelta[axis] - scrollRect.viewport.rect.size[axis];
        autoFetchThreshold = scrollSize > 0 ? (scrollSize - adjustedCellSize[axis] * autoFetchCellSize) / scrollSize : 1.0f;

        // clamp the scrolling position in case content size has shrunk
        if (axis == 0)
        {
            scrollRect.horizontalNormalizedPosition = scrollRect.horizontalNormalizedPosition;
        }
        else
        {
            scrollRect.verticalNormalizedPosition = scrollRect.verticalNormalizedPosition;
        }
    }

    private void PositionCell(ScrollableCell go, int index)
    {
        int indexInSlice = index % NUMBER_OF_COLUMNS;
        Vector3 offset;
        if (!horizontal)
        {
            offset = new Vector3(adjustedCellSize[0] * indexInSlice, -(index / NUMBER_OF_COLUMNS) * adjustedCellSize[1], 0);
        }
        else
        {
            offset = new Vector3((index / NUMBER_OF_COLUMNS) * adjustedCellSize[0], -adjustedCellSize[1] * indexInSlice, 0);
        }
        int axis = scrollAxis ^ 1;
        offset[axis] += indexInSlice * cellSpacing;
        go.transform.localPosition = firstCellPostion + offset;
    }

    private void FillCellsPool ()
    {
        for (int i = localCellsPool.Count; i < visibleCellsTotalCount; i++) {
            localCellsPool.AddLast (this.InstantiateCell ());
        }
    }

    private ScrollableCell InstantiateCell ()
    {
        GameObject cellTempObject = Instantiate(cellPrefab.gameObject, content.transform);
        cellTempObject.layer = this.gameObject.layer;
        cellTempObject.transform.localScale = cellPrefab.transform.localScale * adaptionScale;
        cellTempObject.gameObject.SetActive(false);

        var cell = cellTempObject.GetComponent<ScrollableCell>();
        if (adapationType == AdapationType.Resize)
        {
            ResizeCell(cell);
        }
        return cell;
    }

    private void ResizeCell(ScrollableCell cell)
    {
        var trans = cell.transform as RectTransform;
        int axis = scrollAxis ^ 1;
        trans.SetSizeWithCurrentAnchors((RectTransform.Axis)axis, adjustedCellSize[axis]);
    }

    private LinkedListNode<ScrollableCell> GetCellFromPool(bool scrollingPositive)
    {
        if(localCellsPool.Count == 0)
            return null;

        LinkedListNode<ScrollableCell> cell = localCellsPool.First;
        localCellsPool.RemoveFirst ();

        if(scrollingPositive)
            cellsInUse.AddLast (cell);
        else
            cellsInUse.AddFirst (cell);
        return cell;
    }

    protected override void OnRectTransformDimensionsChange()
    {
        if (hasStarted && isActiveAndEnabled)
        {
            // delay the resize
            // bug can arise when
            // 1. destroy the root canvas
            // 2. resize due to CanvasScalar was disabled
            // 3. get the cell data which can be different than the one before destroy
            dimensionChanged = true;
        }
    }

    private void Resize()
    {
        var oldNumItemsPerSlice = NUMBER_OF_COLUMNS;
        var oldCellsPerRow = visibleCellsRowCount;
        var oldVisibleCells = visibleCellsTotalCount;

        ComputeVisibleCellsCount();

        if (oldCellsPerRow == visibleCellsRowCount &&
            oldVisibleCells == visibleCellsTotalCount &&
            oldNumItemsPerSlice == NUMBER_OF_COLUMNS)
        {
            RepositionVisibleCells();
            return;
        }

        FreeAllCells();

        if (localCellsPool.Count > visibleCellsTotalCount) {
            do {
                Destroy(localCellsPool.Last.Value.gameObject);
                localCellsPool.RemoveLast();
            } while (localCellsPool.Count > visibleCellsTotalCount);
        } else if (localCellsPool.Count < visibleCellsTotalCount) {
            FillCellsPool();
        }

        CalculateCurrentIndex();
        PopulateCells();
    }

    private void RepositionVisibleCells()
    {
        foreach (var cell in cellsInUse)
        {
            PositionCell(cell, cell.DataIndex);
        }
    }

    public float scrollPosition
    {
        get
        {
            return 1.0f - (horizontal ? scrollRect.horizontalNormalizedPosition : scrollRect.verticalNormalizedPosition);
        }
        set
        {
            if (horizontal)
            {
                scrollRect.horizontalNormalizedPosition = 1.0f - value;
            }
            else
            {
                scrollRect.verticalNormalizedPosition = 1.0f - value;
            }
        }
    }

    public bool scrollable
    {
        get { return scrollRect.enabled; }
        set { scrollRect.enabled = value; }
    }

    public void StopScrolling()
    {
        scrollRect.StopMovement();
    }

    /// <summary>
    /// used for keeping track of selections.
    /// </summary>
    public SelectionDataModel selectionModel
    {
        get;
        private set;
    }

    public IListItemModel model
    {
        get;
        private set;
    }

    private void OnItemRemoved(int index)
    {
        int cellIndex = GetCellIndex(index);
        if (cellIndex < visibleCellsTotalCount)
        {
            PopulateCells();
        }

        var contentSize = scrollRect.content.sizeDelta[scrollAxis];
        fetchMoreItems = contentSize <= scrollRect.viewport.rect.size[scrollAxis];
    }

    private void OnItemInserted(Range range)
    {
        if (GetCellIndex(range.start) >= visibleCellsTotalCount)
        {
            UpdateContentSize();
            return;
        }
        PopulateCells();
    }

    private void OnItemUpdated(int index)
    {
        var cell = GetCellByDataIndex(index);
        if (cell != null)
        {
            cell.DataObject = model.getItem(index);
        }
    }

    private void OnModelReset()
    {
        PopulateCells();
        selectionModel.ClearSelections();
    }

    private void OnItemSelectionChanged(int index, bool selected)
    {
        int cellIndex = GetCellIndex(index);
        if (cellIndex >= 0 && cellIndex < visibleCellsTotalCount)
        {
            GetCellByDataIndex(index).ConfigureCell();
        }
    }

    private int GetCellIndex(int itemIndex)
    {
        return itemIndex - firstSliceIndex * NUMBER_OF_COLUMNS;
    }

    private int GetItemIndex(int cellIndex)
    {
        return firstSliceIndex * NUMBER_OF_COLUMNS + cellIndex;
    }

    private void OnItemMoved(int from, int to)
    {
        int fromCellIndex = GetCellIndex(from);
        int toCellIndex = GetCellIndex(to);
        if (fromCellIndex >= visibleCellsTotalCount && toCellIndex >= visibleCellsTotalCount)
        {
            return;
        }
        PopulateCells();
    }

    private void OnItemIndexChanged(ItemIndexChanges changes)
    {
        PopulateCells();
    }

    public void Refresh()
    {
        PopulateCells();
    }
}
