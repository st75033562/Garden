using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputCharacterLimit : MonoBehaviour {
    private InputField inputField;

    const int EnglishCharacterLimit = 32;
    const int DefaultCharacterLimit = 23;
	// Use this for initialization
	void Start () {
        inputField = GetComponent<InputField>();
        LocalizationManager.instance.onLanguageChanged += OnLanguageChanged;
        OnLanguageChanged();
    }

    void OnLanguageChanged() {
        if(LocalizationManager.instance.currentLocaleDir == "en") {
            inputField.characterLimit = EnglishCharacterLimit;
        } else {
            inputField.characterLimit = DefaultCharacterLimit;
        }
    }

    void OnDestroy() {
        LocalizationManager.instance.onLanguageChanged -= OnLanguageChanged;
    }
}
