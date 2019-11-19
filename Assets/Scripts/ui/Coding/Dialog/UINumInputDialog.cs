using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Text.RegularExpressions;

public class UINumInputDialogConfig
{
    public string title;
    public string number;
    public string unit;
    public string description;
    public bool showUnit;
    public bool showPoint;
    public bool showSign;
    public bool showDescription;
}

public class UINumInputDialog : UIInputDialogBase
{
    public RectTransform m_TextRect;
    public Text m_Number;
    public Text m_Unit;
    public Text m_Title;
    public Text m_NumberDescription;
    public GameObject m_DecimalPointBtn;
    public GameObject m_NegativeBtn;
    public int m_MaxDigitNum;

    private string m_Value;
    private bool m_bNegative;
    private bool m_clearOnInputDigits = true;

    IDialogInputCallback m_Callback;

    public void OnClickBackSpace()
    {
        m_clearOnInputDigits = false;
        m_Value = m_Value.Remove(m_Value.Length - 1);
        if (m_Value == string.Empty)
        {
            m_Value = "0";
            m_bNegative = false;
        }
        UpdateNumberText();
    }

    public void Confirm()
    {
        float mRt;
        if (float.TryParse(m_Value, out mRt))
        {
            if (m_Callback != null)
            {
                string result = "";
                if (m_bNegative)
                {
                    result = "-";
                }
                result += m_Value;
                m_Callback.InputCallBack(result);
            }
            CloseDialog();
        }
    }

    public void Cancel()
    {
        CloseDialog();
    }

    public void OnClickDigit(string key)
    {
        if (m_clearOnInputDigits)
        {
            m_Value = "";
            m_bNegative = false;
            m_clearOnInputDigits = false;
        }

        if (m_Value.Length >= m_MaxDigitNum)
        {
            return;
        }
        if (key[0] == '.')
        {
            if (m_Value == "")
            {
                m_Value = "0";
            }
            else if (m_Value.Contains("."))
            {
                return;
            }
        }
        if (m_Value == "0")
        {
            if (key == "0")
            {
                return;
            }
            else if (key[0] >= '1' && key[0] <= '9')
            {
                m_Value = key;
            }
            else
            {
                m_Value += key;
            }
        }
        else
        {
            m_Value += key;
        }
        UpdateNumberText();
    }

    public void Configure(UINumInputDialogConfig config, IDialogInputCallback callback)
    {
        SetText(config.title.Localize(), ValidateNum(config.number), config.unit.Localize(), config.description.Localize());
        SetEnvironment(config.showUnit, config.showPoint, config.showSign, config.showDescription);
        m_Callback = callback;
    }

    string ValidateNum(string input)
    {
        string num = "0";
        var match = Regex.Match(input, @"^\s*(\+|-)?((\d+\.\d*)|(\.?\d+))");
        if (match.Success)
        {
            var numWithoutSign = match.Groups[2].Value;
            if (numWithoutSign.Contains("."))
            {
                numWithoutSign = numWithoutSign.TrimStart('0');
                if (numWithoutSign[0] == '.')
                {
                    numWithoutSign = '0' + numWithoutSign;
                }
            }
            num = match.Groups[1].Value + numWithoutSign;
        }
        return num;
    }

    void SetText(string title, string num, string unit, string description)
    {
        m_Title.text = title;
        m_Unit.text = unit;
        m_NumberDescription.text = description;
        if (num[0] == '-')
        {
            m_bNegative = true;
            m_Value = num.Substring(1, num.Length - 1);
        }
        else
        {
            m_bNegative = false;
            m_Value = num;
        }
        UpdateNumberText();
    }

    void SetEnvironment(bool showUnit, bool showPoint, bool showNegative, bool showDescription)
    {
        m_Unit.gameObject.SetActive(showUnit);
        m_DecimalPointBtn.SetActive(showPoint);
        m_NegativeBtn.SetActive(showNegative);
        m_NumberDescription.gameObject.SetActive(showDescription);
    }

    public void ClearText()
    {
        m_Value = "0";
        m_bNegative = false;
        m_clearOnInputDigits = false;
        UpdateNumberText();
    }

    public void Negate()
    {
        if (m_Value == "0")
        {
            return;
        }

        m_bNegative = !m_bNegative;
        m_clearOnInputDigits = false;
        UpdateNumberText();
    }

    private void UpdateNumberText()
    {
        if (m_bNegative)
        {
            m_Number.text = "-" + m_Value;
        }
        else
        {
            m_Number.text = m_Value;
        }
    }

    public override UIDialog dialogType
    {
        get { return UIDialog.UINumInputDialog; }
    }
}
