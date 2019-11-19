using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AchiveCell : ScrollableCell {
    [SerializeField]
    private Image[] images;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public override void ConfigureCellData() {
        AchieveDataPack achiveDataPack = (AchieveDataPack)dataObject;
        if(achiveDataPack == null)
            return;
        for(int i=0; i< images.Length; i++) {
            if(i < achiveDataPack.datas.Count) {
                images[i].gameObject.SetActive(true);
                images[i].sprite = Resources.Load<Sprite>("AchieveIcon/" + achiveDataPack.datas[i].iconName);
            } else {
                images[i].gameObject.SetActive(false);
            }
        }

    }
}
