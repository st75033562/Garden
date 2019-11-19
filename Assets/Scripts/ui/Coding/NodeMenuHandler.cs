using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeMenuHandler
{
    private readonly DoubleClickDetector m_detector = new DoubleClickDetector();

    private readonly UIWorkspace m_Workspace;

    private static readonly Color MenuItemHighlightColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);
    private const string MenuItemCopyPaste = "menu_copy_paste_blocks";
    private const string MenuItemCopy = "menu_copy_blocks";
    private const string MenuItemPaste = "menu_paste_blocks";
    private const string MenuItemCopyAll = "menu_copy_all_blocks";
    private const string MenuItemEditFunction = "menu_edit_function";

    public NodeMenuHandler(UIWorkspace workspace)
    {
        if (workspace == null)
        {
            throw new ArgumentNullException("workspace");
        }

        m_Workspace = workspace;
    }

    public bool OnPointerClick(PointerEventData eventData)
    {
        if (ShouldShowMenu(eventData))
        {
            // find the first node which is not transient
            var clickedNode = eventData.pointerPress.GetComponentInParent<FunctionNode>();
            if (clickedNode)
            {
                clickedNode.CancelNodePluginClick();
            }

            while (clickedNode && clickedNode.IsTransient)
            {
                clickedNode = clickedNode.transform.parent.GetComponentInParent<FunctionNode>();
            }

            var config = new UIMenuConfig();
            config.items = GetMenuItems(clickedNode).ToArray();
            if (config.items.Length == 0)
            {
                return false;
            }

            config.highlightColor = MenuItemHighlightColor;
            config.position = eventData.pressPosition;

            var menuDialog = UIDialogManager.g_Instance.GetDialog<UIMenuDialog>();
            menuDialog.Configure(config, (item) => {
                switch (item)
                {
                case MenuItemCopyPaste:
                    OnCopyPaste(eventData.pressPosition, clickedNode);
                    break;

                case MenuItemCopy:
                    OnCopy(clickedNode);
                    break;

                case MenuItemPaste:
                    OnPaste(eventData.pressPosition);
                    break;

                case MenuItemCopyAll:
                    OnCopyAll();
                    break;

                case MenuItemEditFunction:
                    OnEditFunction(clickedNode);
                    break;
                }
            }, m_Workspace.CurrentZoom);

            menuDialog.OpenDialog();

            return true;
        }

        return false;
    }

    private bool ShouldShowMenu(PointerEventData eventData)
    {
        if (Application.isEditor || !Application.isMobilePlatform)
        {
            return eventData.button == PointerEventData.InputButton.Right;
        }
        else
        {
            return m_detector.Detect(eventData);
        }
    }

    private IEnumerable<UIMenuItem> GetMenuItems(FunctionNode node)
    {
        if (!node || !node.IsTemplate)
        {
            yield return new UIMenuItem(MenuItemCopyPaste, node != null);
            yield return new UIMenuItem(MenuItemCopy, node != null);

            yield return new UIMenuItem(MenuItemPaste, WorkspaceUtils.CanPasteFromClipboard(m_Workspace));

            yield return new UIMenuItem(MenuItemCopyAll);
        }

        if (node is FunctionDeclarationNode)
        {
            yield return new UIMenuItem(MenuItemEditFunction);
        }
    }

    private void OnCopyPaste(Vector2 pressedPos, FunctionNode sourceNode)
    {
        OnCopy(sourceNode);
        OnPaste(pressedPos);
    }

    private void OnCopy(FunctionNode sourceNode)
    {
        WorkspaceUtils.CopyNodesToClipboard(m_Workspace, sourceNode);
    }

    private void OnPaste(Vector2 pointerPos)
    {
        WorkspaceUtils.PasteNodesFromClipboard(m_Workspace, pointerPos);
    }

    private void OnCopyAll()
    {
        WorkspaceUtils.CopyNodesToClipboard(m_Workspace, m_Workspace.CodePanel.Nodes.Where(x => x.IsFreeNode));
    }

    private void OnEditFunction(FunctionNode node)
    {
        var dialog = UIDialogManager.g_Instance.GetDialog<UIEditFunctionDialog>();
        dialog.Configure(m_Workspace, (node as FunctionDeclarationNode).Declaration);
        dialog.OpenDialog();
    }
}
