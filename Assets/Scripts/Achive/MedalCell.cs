using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DataAccess;

public class MedalCell : ScrollableCell {
    [SerializeField]
    private Image image;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void ConfigureCellData() {
        MedalData medalData = (MedalData)dataObject;
        if(medalData == null)
            return;
        image.sprite = Resources.Load<Sprite>("AchieveIcon/" + medalData.iconName);
    }
}
