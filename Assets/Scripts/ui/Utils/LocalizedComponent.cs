using UnityEngine;

public abstract class LocalizedComponent : MonoBehaviour
{
    protected virtual void Awake()
    {
        LocalizationManager.instance.onLanguageChanged += OnLanguageChanged;
    }

    protected virtual void Start()
    {
        OnLanguageChanged();
    }

    protected virtual void OnDestroy()
    {
        LocalizationManager.instance.onLanguageChanged -= OnLanguageChanged;
    }

    protected abstract void OnLanguageChanged();
}
