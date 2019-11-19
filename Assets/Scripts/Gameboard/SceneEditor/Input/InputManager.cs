using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    /// <summary>
    /// a simple input manager, handlers are tested in registration order
    /// </summary>
    public class InputManager
    {
        private readonly List<IInputHandler> m_objects = new List<IInputHandler>();

        public void Register(IInputHandler obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            m_objects.Add(obj);
        }

        public void Unregister(IInputHandler obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            m_objects.Remove(obj);
        }

        // return true if any input object is hit
        public bool OnPointerDown(Vector2 inputPosition)
        {
            if (currentInputHandler != null)
            {
                return false;
            }

            currentInputHandler = m_objects.Find(x => x.enabled && x.HitTest(inputPosition));
            if (currentInputHandler != null)
            {
                currentInputHandler.OnPointerDown(inputPosition);
                return true;
            }
            return false;
        }

        public bool OnDrag(Vector2 inputPosition)
        {
            if (currentInputHandler != null)
            {
                currentInputHandler.OnDrag(inputPosition);
                return true;
            }
            return false;
        }

        public void OnPointerUp()
        {
            if (currentInputHandler != null)
            {
                currentInputHandler.OnPointerUp();
                currentInputHandler = null;
            }
        }

        public IInputHandler currentInputHandler
        {
            get;
            private set;
        }

        public bool isHandlingInput
        {
            get { return currentInputHandler != null; }
        }
    }
}
