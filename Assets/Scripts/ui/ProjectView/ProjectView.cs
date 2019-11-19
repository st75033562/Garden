using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g_WebRequestManager = Singleton<WebRequestManager>;


public class ProjectViewConfig
{
    public Action<IRepositoryPath> projectSelectCallback;
    public Action closeCallback;
    public bool showSettingButton = true;
    public bool showDeleteButton = true;
    public bool showAddCell = true;
    public IRepositoryPath initialDir;
}

public class ProjectView : MonoBehaviour
{
    public GameObject m_SettingButton;
    public ScrollableAreaController m_ScrollController;
    public GameObject btnFolderGo;
    public GameObject delButton;
    public GameObject addButton;
    public Button[] disableModeBtns;
    public ButtonColorEffect btnBack;
    public GameObject btnCancle;
    public GameObject addPanel;
    public UISortMenuWidget uiSortMenuWidget;

    public ScrollRect m_Scroll;

	private bool m_deletingMode;
    private bool m_canAddProject;

    private readonly List<ProjectItemData> m_localProjectItems = new List<ProjectItemData>();
    private Action<IRepositoryPath> m_selectCallback;
    private Action m_closeCallback;
    private IRepositoryPath currentPath;
    private UISortSetting sortSetting;

    private static readonly string[] s_sortOptions = {
        "ui_single_pk_sort_creation_time",
        "ui_modified_time",
        "ui_single_pk_sort_name"
    };

    public void Initialize(ProjectViewConfig config)
	{
        m_selectCallback = config.projectSelectCallback;
        m_closeCallback = config.closeCallback;
        if (delButton != null)
        {
            delButton.SetActive(config.showDeleteButton);
        }

        if (config.initialDir != null && CodeProjectRepository.instance.isDirectory(config.initialDir))
        {
            currentPath = config.initialDir;
        }
        else
        {
            currentPath = CodeProjectRepository.instance.createDirPath("");
        }

        m_canAddProject = config.showAddCell;
        addButton.SetActive(config.showAddCell);

        uiSortMenuWidget.SetOptions(s_sortOptions.Select(x => x.Localize()).ToArray());
        RefreshOnDirChange();

        SetDeletingMode(false);
    }

    void RefreshOnDirChange()
    {
        InitSortSetting();
        // avoid triggering OnSortChange as we need to reload on refresh
        uiSortMenuWidget.onSortChanged.RemoveListener(OnSortChanged);
        uiSortMenuWidget.SetCurrentSort(sortSetting.sortKey, sortSetting.ascending);
        // register after SetCurrentSort
        uiSortMenuWidget.onSortChanged.AddListener(OnSortChanged);
        Refresh();
    }

    public void OnClickItem(ProjectItemView view)
	{
        if(view.ItemData.pathInfo.path != null && view.ItemData.pathInfo.path.isDir) {
            if(m_deletingMode) {
                PopupManager.YesNo("ui_confirm_delete".Localize(), () => {
                    DeleteCloudProject(view.ItemData.pathInfo.path.ToString());
                }, null);
            } else {
                m_Scroll.normalizedPosition = new Vector2(0, 1);
                currentPath = view.ItemData.pathInfo.path;
                RefreshOnDirChange();
            }
        } else {
            OnClickLocalProject(view);
        }
	}

    private void Refresh(bool reload = true)
    {
        btnFolderGo.SetActive(currentPath.depth + 1 < ProjectRepository.MaxDepth && m_canAddProject);
        if (reload)
        {
            InitializeLocalProjectItems();
        }
        addPanel.SetActive(m_localProjectItems.Count == 0 && m_canAddProject && !m_deletingMode);
        foreach (var item in m_localProjectItems)
        {
            item.isDeleting = m_deletingMode;
        }
        SortItems();
        m_ScrollController.InitializeWithData(m_localProjectItems);
    }

    private void InitializeLocalProjectItems()
    {
        m_localProjectItems.Clear();

        var paths = CodeProjectRepository.instance.listFileInfos(currentPath.ToString());
        m_localProjectItems.AddRange(
            paths.Select(x => new ProjectItemData {
                    pathInfo = x
                }));
    }

    public void OnClickAddProject() {
        NotifySelectedProject("");
    }

	void OnClickLocalProject(ProjectItemView view)
	{
        if (m_deletingMode)
        {
            PopupManager.YesNo("ui_confirm_delete".Localize(), () => {
                DeleteCloudProject(view.ProjectPath);
            }, null);
        }
        else
        {
            NotifySelectedProject(view.ItemData.pathInfo.path.name);
        }
	}

    void DeleteCloudProject(string path) {
        var request = new DeleteRequest();
        request.type = GetCatalogType.SELF_PROJECT_V2;
        request.basePath = RequestUtils.EncodePath(path);
        request.userId = UserManager.Instance.UserId;
        request.Success((t)=> {
            DeleteProject(path);
        })
            .Execute();
 
    }

    private void NotifySelectedProject(string name)
    {
        if (m_selectCallback != null)
        {
            m_selectCallback(currentPath.AppendFile(name));
        }
    }

	private void DeleteProject(string path)
	{
		if (CodeProjectRepository.instance.delete(path))
		{
            var index = m_localProjectItems.FindIndex(x => {
                return x.pathInfo.path != null && x.pathInfo.path.ToString() == path;
            });
            m_localProjectItems.RemoveAt(index);
            Refresh(false);
        }
    }

	public void OnClickSettings()
	{
        PopupManager.Settings();
	}

	public void OnClickDelete()
	{
        SetDeletingMode(true);
	}

	public void OnClickCancel()
	{
        SetDeletingMode(false);
	}

	private void SetDeletingMode(bool deleting)
	{
        if(btnCancle.activeSelf && deleting) {
            return;
        }
        btnCancle.SetActive(deleting);
        foreach (Button btn in disableModeBtns)
        {
            btn.interactable = !deleting;
        }
        btnBack.interactable = !deleting;
        m_deletingMode = deleting;
        Refresh(false);
	}

	public void OnClickReturn()
	{
        if(currentPath.ToString() == "") {
            if(m_closeCallback != null) {
                m_closeCallback();
            }
        } else {
            currentPath = currentPath.parent;
            RefreshOnDirChange();
        }       
	}

    void SortItems() {
        var comparer = new PathInfoComparer((PathInfoMember)sortSetting.sortKey, sortSetting.ascending);
        m_localProjectItems.SortBy(x => x.pathInfo, comparer);
    }

    public void OnClickFolder() {
        PopupManager.InputDialog("ui_new_folder", "", "ui_input_new_folder_hint",
            (str) => {
                var newFolderPath = currentPath.AppendLogicalDir(str.TrimEnd());

                var request = Uploads.CreateFolder(newFolderPath.ToString());
                request.type = GetCatalogType.SELF_PROJECT_V2;
                request.Success(() => {
                    var dirInfo = CodeProjectRepository.instance.createDirectory(newFolderPath.ToString(), request.creationTime);
                    var itemData = new ProjectItemData {
                        pathInfo = new PathInfo(newFolderPath, dirInfo.creationTime, dirInfo.updateTime)
                    };

                    m_localProjectItems.Add(itemData);
                    Refresh(false);
                })
                .Execute();
            },
            new ProjectNameValidator(currentPath.ToString(), "", CodeProjectRepository.instance).ValidateInput);
    }

    void OnSortChanged() {
        sortSetting.SetSortCriterion(uiSortMenuWidget.activeSortOption, uiSortMenuWidget.sortAsc);
        Refresh(false);
    }

    void InitSortSetting() {
        sortSetting = (UISortSetting)UserManager.Instance.userSettings.
            Get(CodeProjectSortSetting.GetKey(currentPath.ToString()), true);
    }
}
