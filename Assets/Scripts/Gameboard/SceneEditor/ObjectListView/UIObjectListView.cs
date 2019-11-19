using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Gameboard
{
    public class UIObjectListView : MonoBehaviour
    {
        public ScrollableAreaController m_scrollController;
        public UIObjectMenu m_objectMenu;
        public Editor m_editor;

        private ObjectManager m_objectManager;
        private Gameboard m_gameboard;
        private bool m_menuOpen;

        void Awake()
        {
            m_editor.onSelectionChanged += OnSelectionChanged;
            editable = true;
        }

        public void Initialize(ObjectManager objectManager)
        {
            if (objectManager == null)
            {
                throw new ArgumentNullException("objectManager");
            }
            m_objectManager = objectManager;
        }

        public void SetGameboard(Gameboard gameboard)
        {
            if (gameboard == null)
            {
                throw new ArgumentNullException("gameboard");
            }

            if (m_gameboard != null)
            {
                m_gameboard.onObjectAdded -= OnObjectAdded;
                m_gameboard.onObjectRemoved -= OnObjectRemoved;
                m_gameboard.onObjectUpdated -= OnObjectUpdated;
            }

            m_gameboard = gameboard;
            m_gameboard.onObjectAdded += OnObjectAdded;
            m_gameboard.onObjectRemoved += OnObjectRemoved;
            m_gameboard.onObjectUpdated += OnObjectUpdated;

            m_scrollController.InitializeWithData(m_gameboard.objects.ToList());
        }

        private void OnSelectionChanged()
        {
            if (m_editor.selectedEntity)
            {
                // null for robot
                var objectInfo = m_gameboard.GetObject(m_editor.selectedEntity.entityName);
                if (objectInfo != null)
                {
                    var index = m_scrollController.model.indexOf(objectInfo);
                    m_scrollController.selectionModel.Select(index, true);
                }
                else
                {
                    m_scrollController.selectionModel.ClearSelections();
                }
            }
            else
            {
                m_scrollController.selectionModel.ClearSelections();
            }

            if (m_menuOpen)
            {
                m_objectMenu.Close();
            }
        }

        private void OnObjectAdded(int index, ObjectInfo obj)
        {
            m_scrollController.model.insertItem(index, obj);
        }

        private void OnObjectRemoved(ObjectInfo obj)
        {
            m_scrollController.model.removeItem(obj);
        }

        private void OnObjectUpdated(ObjectInfo obj)
        {
            m_scrollController.model.updatedItem(obj);
        }

        public bool editable
        {
            get;
            set;
        }

        public void OnClick(UIObjectCell cell)
        {
            if (!editable) { return; }

            var entity = m_objectManager.Get(cell.data.name);
            Assert.IsNotNull(entity);
            m_editor.selectedEntity = entity;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            if (m_menuOpen)
            {
                m_menuOpen = false;
                m_objectMenu.gameObject.SetActive(false);
            }

            gameObject.SetActive(false);
        }

        public void ToggleObjectMenu(RectTransform button)
        {
            if (!m_menuOpen)
            {
                var pos = button.transform.position.xy();
                pos.x += 10;
                pos.y -= 10;

                m_objectMenu.Open(OnObjectMenuClosed);
                (m_objectMenu.transform as RectTransform).Position(pos, UIAnchor.LeftCenter);

                m_menuOpen = true;
                m_scrollController.scrollable = false;
            }
            else
            {
                m_objectMenu.Close();
            }

        }

        void OnObjectMenuClosed()
        {
            m_menuOpen = false;
            m_scrollController.scrollable = true;
        }
    }
}
