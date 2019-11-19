using UnityEngine;

namespace Gameboard
{
    public abstract class TextObject : MonoBehaviour
    {
        public abstract void SetText(string text, int size, Color color);
    }
}
