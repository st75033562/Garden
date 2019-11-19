using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PopupGameBoardSelect : PopupController
{
    public UISortMenuWidget uiSortMenuWidget;
    private UISortSetting sortSetting;
    private List<string> gbBankCatalogs;
    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_single_pk_sort_name"
    };

    private enum SortType
    {
        CreateTime,
        Name
    }
    public enum VisibleType
    {
        All,
        SourceAvailable,
    }

    public class ConfigureParameter
    {
        public VisibleType visibleType;
        public Func<Gameboard.Gameboard, bool> filter; // if not null, extra filtering is performed
        public Action<IRepositoryPath> selectCallBack;
    }

    [SerializeField]
    private ScrollLoopController scroll;

    private ConfigureParameter configureParameter;
    private IRepositoryPath currentPath;

    protected override void Start()
    {
        base.Start();
        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        currentPath = GameboardRepository.instance.createDirPath("");
        configureParameter = (ConfigureParameter)payload;
        scroll.context = this;
        UISortSettingDefault uiSortSetting = new UISortSettingDefault();
        sortSetting = new UISortSetting("PopupGameBoardSelect", uiSortSetting);
        sortSetting.ascending = false;
        ShowItems();
    }

    void ShowItems()
    {
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);

        var gameboardDataList = GetItems();
        var comparer = GetComparison(sortSetting.sortKey, sortSetting.ascending);
        if (comparer != null)
        {
            gameboardDataList.Sort(comparer);
        }
        scroll.initWithData(gameboardDataList);
    }

    void OnSortChanged()
    {
        sortSetting.sortKey = (int)uiSortMenuWidget.activeSortOption;
        sortSetting.ascending = uiSortMenuWidget.sortAsc;
        //  sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        ShowItems();
    }

    static Comparison<GameBoardSelectItemData> GetComparison(int type, bool asc)
    {
        Comparison<GameBoardSelectItemData> comp = null;
        switch ((SortType)type)
        {
            case SortType.CreateTime:
                comp = (x, y) =>
                {
                    return x.pathInfo.creationTime.CompareTo(y.pathInfo.creationTime);
                };
                break;

            case SortType.Name:
                comp = (x, y) => string.Compare(x.path.name, y.path.name, StringComparison.CurrentCultureIgnoreCase);
                break;
        }
        return comp != null ? comp.Invert(!asc) : null;
    }
    List<GameBoardSelectItemData> GetItems()
    {
        var showAll = configureParameter.visibleType == VisibleType.All;

        var items = new List<GameBoardSelectItemData>();
        foreach (var pathInfo in GameboardRepository.instance.listFileInfos(currentPath.ToString()))
        {
            var item = new GameBoardSelectItemData();
            item.path = pathInfo.path;
            item.pathInfo = pathInfo;
            if (pathInfo.path.isFile)
            {
                var gameboard = GameboardRepository.instance.getGameboard(pathInfo.path.ToString());
                if (gameboard != null)
                {
                    if (!showAll && !gameboard.sourceCodeAvailable)
                    {
                        continue;
                    }

                    if (configureParameter.filter != null && !configureParameter.filter(gameboard))
                    {
                        continue;
                    }

                    item.themeId = gameboard.themeId;
                }
                else
                {
                    continue;
                }
            }
            items.Add(item);
        }
        items.Sort((x, y) => x.path.CompareTo(y.path));
        return items;
    }

    public void ClickCell(IRepositoryPath path)
    {
        if (path.isDir)
        {
            currentPath = path;
            ShowItems();
        }
        else
        {
            Close();
            configureParameter.selectCallBack(path);
        }
    }

    public void OnClickBack()
    {
        if (currentPath.ToString() == "")
        {
            OnCloseButton();
        }
        else
        {
            currentPath = currentPath.parent;
            ShowItems();
        }
    }
}
