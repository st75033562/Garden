using RobotSimulation;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Gameboard
{
    public class UIObjectSettingsDialog : MonoBehaviour
    {
        [Serializable]
        public struct CustomColorBlock
        {
            public Color normal;
            public Color highlighted;
            public Color pressed;

            public void Update(Selectable target)
            {
                var colors = target.colors;
                colors.normalColor = normal;
                colors.highlightedColor = highlighted;
                colors.pressedColor = pressed;
            }
        }

        public UnityEvent onOk;
        public UnityEvent onCancel;
        public UnityEvent onColorChanged;

        public CustomColorBlock invalidInputColors;
        public Button okButton;

        public InputField[] positionInputs;
        public InputField[] scaleInputs;
        public InputField[] rotationInputs;
        public Text titleText;

        public GameObject editNameIcon;
        public GameObject colorSettingsGo;
        public RobotColorSettings colorSettings;
        public RobotColorButton colorButtonTemplate;
        public RectTransform colorListTransform;

        private ColorBlock m_validColors;
        private bool m_colorInitialized;

        private IObjectPropertyViewModel m_viewModel;
        private bool m_updatingInput;

        private const string DisplayFormat = "0.###";

        void Awake()
        {
            m_validColors = positionInputs[0].colors;
            ReformatInputOnEndEdit();
        }

        private void ReformatInputOnEndEdit()
        {
            var inputs = positionInputs.Concat(scaleInputs).Concat(rotationInputs);
            foreach (var input in inputs)
            {
                var currentInput = input;
                input.onEndEdit.AddListener(str => {
                    m_updatingInput = true;
                    currentInput.text = (str != "" ? float.Parse(str) : 0).ToString(DisplayFormat);
                    m_updatingInput = false;
                });
            }
        }

        private void InitializeColors()
        {
            if (m_colorInitialized) { return; }

            m_colorInitialized = true;
            for (int i = 0; i < colorSettings.colorCount; ++i)
            {
                var instance = (GameObject)Instantiate(colorButtonTemplate.gameObject, colorListTransform);
                instance.SetActive(true);
                var button = instance.GetComponent<RobotColorButton>();
                button.colorId = i;
            }
        }

        public void SetViewModel(IObjectPropertyViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            m_updatingInput = true;

            m_viewModel = viewModel;

            editNameIcon.SetActive(viewModel.nameEditable);
            titleText.text = viewModel.name;

            colorSettingsGo.SetActive(viewModel.colorEditable);
            if (viewModel.colorEditable)
            {
                SetCurrentColor(viewModel.colorId);
            }

            InitVectorInputs(viewModel.position, positionInputs);
            positionInputs[2].interactable = viewModel.zPosEditable;

            InitVectorInputs(viewModel.rotation, rotationInputs);
            for (int i = 0; i < rotationInputs.Length; ++i)
            {
                rotationInputs[i].interactable = ((1 << i) & (int)viewModel.rotationConstraints) != 0;
            }

            InitVectorInputs(viewModel.scale, scaleInputs);
            foreach (var input in scaleInputs)
            {
                input.interactable = viewModel.scaleEditable;
            }

            m_updatingInput = false;
        }

        private void InitVectorInputs(Vector3 v, InputField[] inputs)
        {
            for (int i = 0; i < 3; ++i)
            {
                inputs[i].text = v[i].ToString(DisplayFormat);
            }
        }

        private void SetCurrentColor(int colorId)
        {
            InitializeColors();
            var toggle = colorListTransform.GetChild(colorId).GetComponent<Toggle>();
            toggle.isOn = true;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            m_viewModel = null;
            gameObject.SetActive(false);
        }

        public void OnPositionChanged(int comp)
        {
            if (comp < 0 || comp >= positionInputs.Length)
            {
                throw new ArgumentOutOfRangeException("comp");
            }

            if (m_updatingInput) { return; }

            var currentPos = m_viewModel.position;
            currentPos[comp] = GetFloat(positionInputs[comp]);
            m_viewModel.position = currentPos;

            if (!m_viewModel.isValid)
            {
                invalidInputColors.Update(positionInputs[comp]);
            }
            else
            {
                ResetInputColor();
            }
        }

        public void OnScaleChanged(int comp)
        {
            if (comp < 0 || comp >= scaleInputs.Length)
            {
                throw new ArgumentOutOfRangeException("comp");
            }

            if (m_updatingInput) { return; }

            if (!m_viewModel.scaleEditable) { return; }

            var currentScale = m_viewModel.scale;
            currentScale[comp] = GetFloat(scaleInputs[comp]);
            m_viewModel.scale = currentScale;

            if (!m_viewModel.isValid)
            {
                invalidInputColors.Update(scaleInputs[comp]);
            }
            else
            {
                ResetInputColor();
            }
        }

        public void OnRotationChanged(int comp)
        {
            if (comp < 0 || comp >= rotationInputs.Length)
            {
                throw new ArgumentOutOfRangeException("comp");
            }

            if (m_updatingInput || m_updatingInput) { return; }

            if (((1 << comp) & (int)m_viewModel.rotationConstraints) == 0)
            {
                return;
            }

            var currentRotation = m_viewModel.rotation;
            currentRotation[comp] = GetFloat(rotationInputs[comp]);
            m_viewModel.rotation = currentRotation;

            if (currentRotation[comp] != m_viewModel.rotation[comp])
            {
                m_updatingInput = true;
                rotationInputs[comp].text = m_viewModel.rotation[comp].ToString();
                m_updatingInput = false;
            }
            if (!m_viewModel.isValid)
            {
                invalidInputColors.Update(rotationInputs[comp]);
            }
            else
            {
                ResetInputColor();
            }
        }

        private float GetFloat(InputField input)
        {
            float v;
            float.TryParse(input.text, out v);
            return v;
        }

        private void ResetInputColor()
        {
            foreach (var input in positionInputs)
            {
                input.colors = m_validColors;
            }

            foreach (var input in scaleInputs)
            {
                input.colors = m_validColors;
            }

            foreach (var input in rotationInputs)
            {
                input.colors = m_validColors;
            }
        }

        public void OnClickEditName()
        {
            if (!m_viewModel.nameEditable) { return; }

            PopupManager.InputDialog("ui_edit_object_name".Localize(), m_viewModel.name, "",
                input => {
                    m_viewModel.name = input;
                    titleText.text = input;
                },
                input => {
                    if (m_viewModel.IsDuplicateName(input.Trim()))
                    {
                        return "ui_edit_object_name_duplicate_error".Localize();
                    }
                    return null;
                });
        }

        public void OnClickOk()
        {
            m_viewModel.Apply();

            Close();
            if (onOk != null)
            {
                onOk.Invoke();
            }
        }

        public void OnClickCancel()
        {
            m_viewModel.Revert();

            Close();
            if (onCancel != null)
            {
                onCancel.Invoke();
            }
        }

        public void OnClickColor(int colorId)
        {
            m_viewModel.colorId = colorId;
        }
    }
}
