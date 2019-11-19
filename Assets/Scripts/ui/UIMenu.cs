using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using UnityEngine.UI;


public delegate void UIMenuSelectCallback(string str);

public enum UIMenuAnchor
{
    TopLeft,
    TopRight
}

public class UIMenuItem
{
    public static readonly Color DefaultColor = Color.black;

    public string text;
    public string userData; // userData is returned if not null, otherwise item is returned
    public Color color = DefaultColor;
    public bool enabled = true;

    public UIMenuItem(string text, bool enabled = true)
    {
        this.text = text;
        this.enabled = enabled;
    }

    public UIMenuItem(string text, Color color, bool enabled = true)
    {
        this.text = text;
        this.color = color;
        this.enabled = enabled;
    }

    public UIMenuItem(string text, string userData)
    {
        this.text = text;
        this.userData = userData;
    }
}

public class UIMenuConfig
{
    public static readonly Color DefaultDisabledTextColor = Color.grey;
    public static readonly Color DefaultDisabledHighlightColor = Color.clear;

    public UIMenuItem[] items;
    public Color? bgColor;
    public Color? highlightColor;
    public Color disabledTextColor = DefaultDisabledTextColor;
    public Color disabledHighlightColor = DefaultDisabledHighlightColor;
    public RectTransform target;
    public Rect? targetRect; // can be null if target is null, in world space

    // only used when menu is opened at the specified position
    public UIMenuAnchor anchor = UIMenuAnchor.TopLeft;

    public void SetItems(IEnumerable<string> items)
    {
        this.items = items.Select(x => new UIMenuItem(x)).ToArray();
    }

    // show the menu at the specified position
    public Vector2 position
    {
        set { targetRect = new Rect(value, Vector2.zero); }
        get
        {
            if (targetRect != null)
            {
                return targetRect.Value.min;
            }
            return Vector2.zero;
        }
    }

    public void FromJson(string json)
    {
        var data = JsonMapper.ToObject(json);
        items = data["items"].GetStringArray().Select(x => new UIMenuItem(x)).ToArray();
        bgColor = GetColor(data, "color");
        highlightColor = GetColor(data, "hicolor");
        disabledTextColor = GetColor(data, "disabled_color", DefaultDisabledTextColor);
        disabledHighlightColor = GetColor(data, "disabled_hicolor", DefaultDisabledHighlightColor);
    }

    private static Color GetColor(JsonData data, string key, Color defaultColor)
    {
        var color = GetColor(data, key);
        return color != null ? color.Value : defaultColor;
    }

    private static Color? GetColor(JsonData data, string key)
    {
        if (data.HasKey(key))
        {
            return UIUtils.ParseColor((string)data[key]);
        }
        return null;
    }

    public static UIMenuConfig Parse(string json)
    {
        var config = new UIMenuConfig();
        config.FromJson(json);
        return config;
    }
}

public class UIMenu : MonoBehaviour
{
    private const float RelativeScrollEpsilon = 0.01f;

    [Serializable]
    public class ItemSelectedEvent : UnityEvent<int> { }

    public ItemSelectedEvent m_ItemSelected;

    public Color m_DefaultColor = Color.white;
    public Image m_BackgroundImage;

    public float m_ScrollInterval; // also used as the delay before starting scrolling
    public int m_MaxVisibleItems;
    public GameObject m_UpArrow;
    public GameObject m_DownArrow;

    public ScrollRect m_ScrollRect;
    public RectTransform m_Screen; // the screen used to determine how to position the menu

	public UIMenuOption m_TextTemplate;
	public VerticalLayoutGroup m_Lay;
	private List<UIMenuOption> m_MenuList;
	int m_Index;

    RectTransform m_Transform;

	UIMenuSelectCallback m_delegate;

    private Vector2 m_viewportMargin;
    private float m_scrollError;
    private int m_visibleItems;
    private float m_menuItemHeight;

    private enum ScrollDirection
    {
        None,
        Upward,
        Downward,
    }
    private ScrollDirection m_scrollDirection = ScrollDirection.None;

	public void Init()
	{
        if (!m_Screen)
        {
            m_Screen = transform.root as RectTransform;
        }

        m_Transform = GetComponent<RectTransform>();
        Debug.Assert(m_Screen && m_Transform.IsChildOf(m_Screen));

        if (m_UpArrow && m_DownArrow)
        {
            m_ScrollRect.onValueChanged.AddListener(OnScrollValueChanged);
        }

		gameObject.SetActive(false);

        // assuming identity scale
        m_viewportMargin.x = m_Transform.rect.width - m_ScrollRect.viewport.rect.width;
        m_viewportMargin.y = m_Transform.rect.height - m_ScrollRect.viewport.rect.height;

        var templateTransform = m_TextTemplate.GetComponent<RectTransform>();
        m_menuItemHeight = templateTransform.rect.height;
	}

	public void OpenMenu(string cmd, Vector3 position, UIMenuAnchor anchor = UIMenuAnchor.TopLeft)
	{
        var config = UIMenuConfig.Parse(cmd);
        config.position = position;
        config.anchor = anchor;
        OpenMenu(config);
	}

    public void OpenMenu(string cmd, RectTransform targetTrans)
    {
        var config = UIMenuConfig.Parse(cmd);
        config.target = targetTrans;
        OpenMenu(config);
    }

    // targetRect is in world space
    public void OpenMenu(string cmd, Rect targetRect)
    {
        var config = UIMenuConfig.Parse(cmd);
        config.targetRect = targetRect;
        OpenMenu(config);
    }

    public void OpenMenu(UIMenuConfig config)
    {
		gameObject.SetActive(true);
		ResetState();

        // reset
        m_ScrollRect.verticalNormalizedPosition = 1.0f;

        foreach (var item in config.items)
        {
            AddMenu(item, config);
        }

        if (config.bgColor != null)
        {
            m_BackgroundImage.color = config.bgColor.Value;
        }
        else
        {
            m_BackgroundImage.color = m_DefaultColor;
        }

        // in local space of m_Screen
        Rect targetRect;
        if (config.target)
        {
            targetRect = config.target.GetRectIn(m_Screen);
        }
        else
        {
            Assert.IsTrue(config.targetRect.HasValue);
            // convert to local space of m_Screen
            targetRect = m_Screen.InverseTransform(config.targetRect.Value);
        }

        // NOTE: assuming identity scale for all calculations
        var layoutGroupTransform = m_Lay.GetComponent<RectTransform>();

        // calculate the approximate number of visible items
        int visibleItems = (int)((m_Screen.rect.height - m_Lay.padding.top - m_Lay.padding.bottom - m_viewportMargin.y + m_Lay.spacing) /
                                    (m_menuItemHeight + m_Lay.spacing));
        if (m_MaxVisibleItems > 0)
        {
            visibleItems = Mathf.Min(visibleItems, m_MaxVisibleItems);
        }
        m_visibleItems = Mathf.Min(config.items.Length, visibleItems);

        float height = m_Lay.padding.top + m_Lay.padding.bottom + (m_visibleItems - 1) * m_Lay.spacing + m_menuItemHeight * m_visibleItems;
        height *= layoutGroupTransform.localScale.y;

        // make sure the size is not smaller than the target transform
        LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroupTransform);
        float menuWidth = Mathf.Max(layoutGroupTransform.rect.width + m_viewportMargin.x, targetRect.width);
        m_Transform.sizeDelta = new Vector2(menuWidth, height + m_viewportMargin.y);

        m_scrollError = CalculateMenuItemNormalizedHeight(false) * RelativeScrollEpsilon;
        // update arrow buttons
        OnScrollValueChanged(new Vector2(0, m_ScrollRect.verticalNormalizedPosition));
        PositionMenuInsideScreen(targetRect, config.anchor);
    }

    void PositionMenuInsideScreen(Rect targetRect, UIMenuAnchor anchor)
    {
        var menuScreenSize = m_Transform.sizeDelta;
        menuScreenSize.x *= m_Transform.localScale.x;
        menuScreenSize.y *= m_Transform.localScale.y;

        bool isPointRect = targetRect.size == Vector2.zero;

        var screenRect = m_Screen.rect;
        Vector2 topLeft;
        if (isPointRect && anchor == UIMenuAnchor.TopRight)
        {
            topLeft.x = targetRect.xMin - menuScreenSize.x;
            if (topLeft.x < screenRect.xMin)
            {
                topLeft.x = screenRect.xMin;
            }
        }
        else
        {
            topLeft.x = targetRect.xMin;
            if (topLeft.x + menuScreenSize.x > screenRect.xMax)
            {
                topLeft.x = targetRect.xMax - menuScreenSize.x;
            }
        }

        if (targetRect.yMin - menuScreenSize.y >= screenRect.yMin)
        {
            topLeft.y = targetRect.yMin;
        }
        else if (targetRect.yMax + menuScreenSize.y <= screenRect.yMax)
        {
            topLeft.y = targetRect.yMax + menuScreenSize.y;
        }
        else
        {
            topLeft.y = screenRect.yMax;
        }

        m_Transform.position = m_Screen.TransformPoint(topLeft);
    }

	public void CloseMenu()
	{
		gameObject.SetActive(false);
	}

	void AddMenu(UIMenuItem item, UIMenuConfig config)
	{
		UIMenuOption itemUI = GetText();
        itemUI.key = item.userData ?? item.text;
        itemUI.text = item.text.Localize();
        itemUI.clickable = item.enabled;
        if (item.enabled)
        {
            if (config.highlightColor != null)
            {
                itemUI.highlightColor = config.highlightColor.Value;
            }
        }
        else
        {
            itemUI.textColor = config.disabledTextColor;
            itemUI.highlightColor = config.disabledHighlightColor;
        }
	}

	UIMenuOption GetText()
	{
		UIMenuOption item = null;
		if (m_Index < m_MenuList.Count)
		{
			item = m_MenuList[m_Index];
		}
		if (null == item)
		{
			item = Instantiate(m_TextTemplate, m_ScrollRect.content);
			RectTransform mObjRect = item.GetComponent<RectTransform>();
			mObjRect.localScale = Vector3.one;
			m_MenuList.Add(item);
		}
        item.index = m_Index;
        ++m_Index;
		item.gameObject.SetActive(true);
		return item;
	}

	void ResetState()
	{
		m_Index = 0;
		if (null == m_MenuList)
		{
			m_MenuList = new List<UIMenuOption>();
		}
		for (int i = 0; i < m_MenuList.Count; ++i)
		{
			m_MenuList[i].gameObject.SetActive(false);
		}
	}

	public void SelectOne(UIMenuOption item)
	{
		if (null != m_delegate)
		{
			m_delegate(item.key);
		}

        if (m_ItemSelected != null)
        {
            m_ItemSelected.Invoke(item.index);
        }
		CloseMenu();
	}

    public UIMenuOption GetItem(int index)
    {
        if (index < 0 || index >= m_Index)
        {
            throw new ArgumentOutOfRangeException();
        }

        return m_MenuList[index];
    }

	public void SetSelectCallback(UIMenuSelectCallback callBack)
	{
		m_delegate = callBack;
	}

    // call this after OpenMenu is called
    public void SetFirstVisibleItem(int index)
    {
        //if (m_Index == 0) { return; }
        //index = Mathf.Clamp(index, 0, m_Index - 1);

        //var trans = m_MenuList[index].GetComponent<RectTransform>();
        throw new NotImplementedException();
    }

    public void OnBeginScrollDown()
    {
        StopAllCoroutines();
        StartCoroutine(StartScroll(ScrollDirection.Downward));
    }

    private IEnumerator StartScroll(ScrollDirection dir)
    {
        Debug.Assert(dir != ScrollDirection.None);

        m_scrollDirection = dir;
        float movementDir = dir == ScrollDirection.Upward ? 1.0f : -1.0f;
        float menuItemNormalizedHeight = CalculateMenuItemNormalizedHeight(true);

        // align to the boundary of menu item
        m_ScrollRect.verticalNormalizedPosition = m_ScrollRect.verticalNormalizedPosition;

        yield return new WaitForSeconds(m_ScrollInterval);

        // item index counting bottom up
        float index = m_ScrollRect.verticalNormalizedPosition / menuItemNormalizedHeight;
        if (m_scrollDirection == ScrollDirection.Upward)
        {
            // scroll upward, need to align to previous item (items are arranged top down)
            m_ScrollRect.verticalNormalizedPosition = Mathf.CeilToInt(index) * menuItemNormalizedHeight;
        }
        else
        {
            // scroll downward, need to align to next item
            m_ScrollRect.verticalNormalizedPosition = Mathf.FloorToInt(index) * menuItemNormalizedHeight;
        }

        while (movementDir > 0 && m_ScrollRect.verticalNormalizedPosition < 1.0f - m_scrollError ||
               movementDir < 0 && m_ScrollRect.verticalNormalizedPosition > m_scrollError)
        {
            yield return new WaitForSeconds(m_ScrollInterval);
            m_ScrollRect.verticalNormalizedPosition = 
                Mathf.Clamp01(m_ScrollRect.verticalNormalizedPosition + menuItemNormalizedHeight * movementDir);
        }
    }

    private float CalculateMenuItemNormalizedHeight(bool countSpace)
    {
        var templateTrans = m_TextTemplate.GetComponent<RectTransform>();
        var scrollHeight = m_ScrollRect.content.sizeDelta.y - m_ScrollRect.viewport.rect.height;
        if (allItemsVisible)
        {
            scrollHeight = m_ScrollRect.content.sizeDelta.y;
        }
        return (templateTrans.rect.height + (countSpace ? m_Lay.spacing : 0)) / scrollHeight;
    }

    private bool allItemsVisible
    {
        get { return m_visibleItems == m_Index; }
    }

    public void OnBeginScrollUp()
    {
        StopAllCoroutines();
        StartCoroutine(StartScroll(ScrollDirection.Upward));
    }

    public void OnEndScroll()
    {
        m_scrollDirection = ScrollDirection.None;
        StopAllCoroutines();
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        if (!m_UpArrow || !m_DownArrow)
        {
            return;
        }

        m_UpArrow.SetActive(value.y < 1.0f - m_scrollError && !allItemsVisible);
        m_DownArrow.SetActive(value.y > m_scrollError && !allItemsVisible);

        // OnEndScroll won't be called if arrow is inactive
        if (!m_UpArrow.activeSelf && m_scrollDirection == ScrollDirection.Upward ||
            !m_DownArrow.activeSelf && m_scrollDirection == ScrollDirection.Downward)
        {
            OnEndScroll();
        }
    }

    public void SetPostionToVisibleCell(int index)
    {
        Vector3 menuPos = m_MenuList[index].transform.localPosition;
        float interval = Mathf.Abs(menuPos.y) - m_ScrollRect.viewport.rect.height;
        if (interval > 0)
        {
            Vector3 layV3 = m_Lay.transform.localPosition;
            layV3.y = interval;
            m_Lay.transform.localPosition = layV3;
            StartCoroutine(StartScroll(ScrollDirection.Downward));
        }
    }
}
