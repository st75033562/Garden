using DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Gameboard
{
    public interface IObjectDragHandler
    {
        void OnBeginDrag(BundleAssetData asset, Vector2 ponterPos);
        void OnDrag(Vector2 pointerPos);
        void OnEndDrag(Vector2 pointerPos);
    }

    public class UIAssetListView : MonoBehaviour
    {
        public ScrollableAreaController m_scrollController;
        public Canvas m_canvas;

        private BundleAssetData m_draggingAsset;

        public void Initialize(IEnumerable<BundleAssetData> assets)
        {
            if (assets == null)
            {
                throw new ArgumentNullException("assets");
            }

            m_scrollController.InitializeWithData(assets.ToArray());
        }

        public IObjectDragHandler dragHandler { get; set; }

        public void Show(bool visible)
        {
            m_canvas.enabled = visible;
        }

        public void OnBeginDrag(UIAssetCell cell)
        {
            m_draggingAsset = cell.data;
            if (dragHandler != null)
            {
                dragHandler.OnBeginDrag(m_draggingAsset, Input.mousePosition);
            }
        }

        public void OnDrag(UIAssetCell cell)
        {
            if (dragHandler != null)
            {
                dragHandler.OnDrag(Input.mousePosition);
            }
        }

        public void OnEndDrag(UIAssetCell cell)
        {
            if (dragHandler != null)
            {
                dragHandler.OnEndDrag(Input.mousePosition);
            }
            m_draggingAsset = null;
        }
    }
}