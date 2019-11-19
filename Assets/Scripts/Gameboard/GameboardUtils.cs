using System;
using UnityEngine;

namespace Gameboard
{
    public static class GameboardUtils
    {
        public static void SelectGameboard(Action<GameboardSelectResult> onSelected = null, 
                                           Action onCancel = null,
                                           IRepositoryPath initialDir = null,
                                           int initialThemeId = 0)
        {
            PopupManager.GameBoard(result => {
                if (result != null)
                {
                    if (onSelected != null)
                    {
                        onSelected(result);
                    }
                }
                else if (onCancel != null)
                {
                    onCancel();
                }
            },
            initialDir,
            initialThemeId,
            null);
        }
    }
}
