using UnityEngine;

public class SimpleMarkToggle : SimpleToggle
{
    public GameObject markGo;
    public MonoBehaviour[] targets;

    protected override void OnToggleChanged(bool isOn)
    {
        base.OnToggleChanged(isOn);

        if (markGo)
        {
            markGo.SetActive(isOn);
        }

        if (targets != null)
        {
            foreach (var target in targets)
            {
                target.enabled = isOn;
            }
        }
    }
}
