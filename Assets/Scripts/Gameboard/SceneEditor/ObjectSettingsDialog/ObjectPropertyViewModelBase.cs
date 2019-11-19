using RobotSimulation;
using System;
using UnityEngine;

namespace Gameboard
{
    public abstract class ObjectPropertyViewModelBase<T> : IObjectPropertyViewModel where T : IObjectInfo
    {
        public event Action<IObjectPropertyViewModel> onApplyingChanges;
        public event Action<IObjectPropertyViewModel> onChangesApplied;

        protected Vector3 m_currentPosition;
        protected Vector3 m_currentScale;
        protected Vector3 m_currentRotation;
        protected readonly Entity m_entity;
        protected readonly T m_objInfo;

        private const float MaxRotation = 360;

        protected ObjectPropertyViewModelBase(Entity entity, T objInfo)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            if (objInfo == null)
            {
                throw new ArgumentNullException("objInfo");
            }

            m_entity = entity;
            m_objInfo = objInfo;

            ReloadTransformValues();
        }

        protected void ReloadTransformValues()
        {
            m_currentPosition = Coordinates.ConvertVector(m_objInfo.position);
            m_currentRotation = Coordinates.ConvertRotation(m_objInfo.rotation);
            m_currentScale = Coordinates.ConvertVector(m_objInfo.scale);
        }

        public abstract bool nameEditable
        {
            get;
        }

        public abstract string name
        {
            get;
            set;
        }

        public abstract bool IsDuplicateName(string name);

        public Vector3 position
        {
            get
            {
                return m_currentPosition;
            }
            set
            {
                m_currentPosition = value;
                m_entity.positional.position = value;
            }
        }

        public virtual bool zPosEditable
        {
            get { return true; }
        }


        public abstract bool scaleEditable
        {
            get;
        }

        public Vector3 scale
        {
            get
            {
                return m_currentScale;
            }
            set
            {
                if (!scaleEditable)
                {
                    throw new InvalidOperationException();
                }

                m_currentScale = value;
                m_entity.positional.localScale = value;
            }
        }

        public Vector3 rotation
        {
            get
            {
                return m_currentRotation;
            }
            set
            {
                for (int i = 0; i < 3; ++i)
                {
                    m_currentRotation[i] = Mathf.Clamp(value[i], -MaxRotation, MaxRotation);
                }
                m_entity.positional.rotation = m_currentRotation;
            }
        }

        public abstract RotationConstraints rotationConstraints
        {
            get;
        }

        public abstract bool colorEditable
        {
            get;
        }

        public abstract int colorId
        {
            get;
            set;
        }

        public bool isValid
        {
            get { return !m_entity.GetComponent<PlacementErrorDetector>().hasError; }
        }

        protected virtual bool isChanged
        {
            get
            {
                return m_currentPosition != Coordinates.ConvertVector(m_objInfo.position) ||
                        m_currentRotation != Coordinates.ConvertRotation(m_objInfo.rotation) ||
                        m_currentScale != Coordinates.ConvertVector(m_objInfo.scale);
            }
        }

        public virtual void Apply()
        {
            if (onApplyingChanges != null)
            {
                onApplyingChanges(this);
            }

            ApplyChanges();

            if (onChangesApplied != null)
            {
                onChangesApplied(this);
            }
        }

        protected virtual void ApplyChanges()
        {
            m_objInfo.position = Coordinates.ConvertVector(m_currentPosition);
            m_objInfo.rotation = Coordinates.ConvertRotation(m_currentRotation);
            m_objInfo.scale = Coordinates.ConvertVector(m_currentScale);
        }

        public virtual void Revert()
        {
            ReloadTransformValues();

            m_entity.positional.position = m_currentPosition;
            m_entity.positional.rotation = m_currentRotation;
            m_entity.positional.localScale = m_currentScale;
        }
    }
}
