using UnityEngine;
using UnityEngine.UI;

public abstract class BaseTaskPoolCell : ScrollableCell
{
    [SerializeField]
    private Text textName;
    [SerializeField]
    private Text textTime;
    [SerializeField]
    private GameObject loading;

    public override void ConfigureCellData()
    {
        loading.SetActive(false);

        textName.text = template.name;
        textTime.text = TimeUtils.GetLocalizedTime(template.createTime);
    }

    protected TaskTemplate template
    {
        get { return (TaskTemplate)dataObject; }
    }
}
