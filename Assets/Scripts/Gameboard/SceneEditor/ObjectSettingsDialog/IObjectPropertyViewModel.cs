using System;
using UnityEngine;

namespace Gameboard
{
    [Flags]
    public enum RotationConstraints
    {
        X = 1 << 0,
        Y = 1 << 1,
        Z = 1 << 2,
        All = ~0
    }

    public interface IObjectPropertyViewModel
    {
        event Action<IObjectPropertyViewModel> onApplyingChanges;
        event Action<IObjectPropertyViewModel> onChangesApplied;

        bool nameEditable { get; }

        // if nameEditable is false, set will throw InvalidOperationException
        string name { get; set; }

        bool IsDuplicateName(string name);

        Vector3 position { get; set; }

        bool zPosEditable { get; }

        bool scaleEditable { get; }

        Vector3 scale { get; set; }

        Vector3 rotation { get; set; }

        RotationConstraints rotationConstraints { get; }

        bool colorEditable { get; }

        // if colorEditable is false, set will throw InvalidOperationException
        int colorId { get; set; }

        bool isValid { get; }

        // apply changes
        void Apply();

        // revert changes
        void Revert();
    }
}
