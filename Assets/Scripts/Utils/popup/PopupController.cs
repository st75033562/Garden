using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
public class PopupController : MonoBehaviour, IInputListener {
    [SerializeField] protected Text _titleText;
    [SerializeField] protected Text _content;
    [SerializeField] protected Text _leftText;
    [SerializeField] protected Text _rightText;
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField]
    protected float FadeTime = 0.3f;

    protected int baseSortingOrder;

    public string title { get; set; }
    public string content { get; set; }
    public string rightText { get; set; }
    public string leftText { get; set; }
    public object payload { get; set; }

    public Action leftAction = null;
    public Action rightAction = null;
    public Action closeAction = null;

    /// <summary>
    /// true if this is a full screen Opaque popup
    /// the rendering of all previously stacked popups will be disabled when a full screen opaque is shown
    /// </summary>
    public bool fullscreenOpaque;
    public bool closeOnBackPressed = true;

    /// <summary>
    /// modal pop up will prevent application from quitting by default
    /// </summary>
    public bool isModal;

    private bool closing;
    private Canvas m_canvas;

    public int Id
    {
        get;
        internal set;
    }

    protected virtual void Awake()
    {
        m_canvas = GetComponent<Canvas>();
        isVisible = true;

        ApplicationEvent.onQuit += OnQuit;
    }

    protected virtual void OnDestroy()
    {
        ApplicationEvent.onQuit -= OnQuit;
    }

    protected virtual void Start()
    {
        if (_titleText != null && title != null)
        {
            _titleText.text = title;
        }
        if (_content != null && content != null)
        {
            _content.text = content;
        }
        if (_leftText != null && leftText != null)
        {
            _leftText.text = leftText;
        }
        if (_rightText != null && rightText != null)
        {
            _rightText.text = rightText;
        }
	}

    protected virtual void OnEnable()
    {
        RegisterInputListener();
    }

    protected void RegisterInputListener()
    {
        InputListenerManager.instance.Pop(this);
        int priority = InputUtils.GetUIPriority(m_canvas ? m_canvas.sortingOrder : 0);
        InputListenerManager.instance.Push(this, priority);
    }

    protected virtual void OnDisable()
    {
        InputListenerManager.instance.Pop(this);
    }

    public virtual void OnRightClose()
    {
        if (rightAction != null)
            rightAction();
        Close();
    }

    public virtual void OnLeftClose()
    {
        Close();
        if (leftAction != null)
            leftAction();
    }

    public virtual void OnCloseButton() {
        Close();
    }

    public void Close()
    {
        if (closing) {
            return;
        }
        closing = true;
        DoClose();
    }

    protected virtual void DoClose()
    {
        Destroy (gameObject);
        PopupManager.onClosing(this);
        if (closeAction != null)
            closeAction();
    }

    public virtual void FadeOutCloseBtn() {
        FadeOutClose();
    }

    public void FadeOutClose() {
        if (closing) {
            return;
        }
        closing = true;
        StartCoroutine(FadeOutCloseImpl());
    }

    private IEnumerator FadeOutCloseImpl() {
        var previous = PopupManager.GetPrevious(this);
        if (previous) {
            previous.Show(true);
        }

        float time = 0;
        while(canvasGroup.alpha > 0) {
            time += Time.deltaTime;
            canvasGroup.alpha = 1 - time / FadeTime;
            yield return null;
        }
        DoClose();
    }

    public IEnumerator FadeIn() {
        canvasGroup.alpha = 0;
        float time = 0;
        while(canvasGroup.alpha < 1) {
            time += Time.deltaTime;
            canvasGroup.alpha = time / FadeTime;
            yield return null;
        }
    }

    public int BaseSortingOrder
    {
        set
        {
            baseSortingOrder = value;
            SetBaseSortingOrder(value);
        }
        get
        {
            return baseSortingOrder;
        }
    }

    protected virtual void SetBaseSortingOrder(int order)
    {
        if (m_canvas)
        {
            m_canvas.sortingOrder = order;
        }
        RegisterInputListener();
    }

    public virtual int SortingLayers
    {
        get { return 1; }
    }

    public virtual void Show(bool visible)
    {
        if (m_canvas)
        {
            m_canvas.enabled = visible;
        }
        isVisible = visible;
    }

    public bool isVisible
    {
        get;
        private set;
    }

    protected virtual void OnBackPressed()
    {
        if (closeOnBackPressed)
        {
            OnCloseButton();
        }
    }

    public void BringToTop()
    {
        PopupManager.BringToTop(this);

        // make sure the handler receive the event first
        ApplicationEvent.onQuit -= OnQuit;
        ApplicationEvent.onQuit += OnQuit;
    }

    protected virtual void OnQuit(ApplicationQuitEvent evt)
    {
        if (isModal)
        {
            evt.Ignore();
        }
        else
        {
            evt.Accept();
            Close();
        }
    }

    public virtual bool isInputEnabled
    {
        get { return isVisible && gameObject.activeInHierarchy; }
    }

    public virtual bool OnKey(KeyEventArgs eventArgs)
    {
        if (!isInputEnabled)
        {
            return false;
        }

        if (eventArgs.isPressed && eventArgs.key == KeyCode.Escape)
        {
            OnBackPressed();
        }
        return true;
    }
}
