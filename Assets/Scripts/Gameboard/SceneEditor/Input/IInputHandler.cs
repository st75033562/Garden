using UnityEngine;

namespace Gameboard
{
    public interface IInputHandler
    {
        bool HitTest(Vector2 inputPosition);

        // NOTE: the same inputPosition used in HitTest is passed
        void OnPointerDown(Vector2 inputPosition);

        void OnPointerUp();

        void OnDrag(Vector2 inputPosition);

        bool enabled { get; }
    }
}
