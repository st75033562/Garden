using System;
using UnityEngine;

namespace Gameboard
{
    public class ObjectPropertyViewModel : ObjectPropertyViewModelBase<ObjectInfo>
    {
        private readonly ObjectNameValidator m_nameValidator;
        private readonly IObjectSettingsUndo<ObjectInfo> m_undo;

        private string m_name;

        public ObjectPropertyViewModel(
            Entity entity, 
            ObjectInfo objInfo, 
            ObjectNameValidator nameValidator,
            IObjectSettingsUndo<ObjectInfo> undo)
            : base(entity, objInfo)
        {
            if (nameValidator == null)
            {
                throw new ArgumentNullException("nameValidator");
            }
            if (undo == null)
            {
                throw new ArgumentNullException("undo");
            }

            m_name = entity.entityName;
            m_nameValidator = nameValidator;
            m_undo = undo;
        }

        public override bool nameEditable
        {
            get { return true; }
        }

        public override string name
        {
            get
            {
                return m_name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("value");
                }

                if (IsDuplicateName(value))
                {
                    throw new ArgumentException("duplicate");
                }

                m_name = value;
            }
        }

        public override bool IsDuplicateName(string name)
        {
            return m_nameValidator.IsDuplicate(name);
        }

        public override bool zPosEditable
        {
            get { return m_entity.asset.threeD; }
        }

        public override bool scaleEditable
        {
            get { return true; }
        }

        public override RotationConstraints rotationConstraints
        {
            get
            {
                var constraints = RotationConstraints.All;
                var rb = m_entity.GetComponent<Rigidbody>();
                if (rb)
                {
                    if ((rb.constraints & RigidbodyConstraints.FreezeRotationX) != 0)
                    {
                        constraints &= ~RotationConstraints.X;
                    }
                    if ((rb.constraints & RigidbodyConstraints.FreezeRotationY) != 0)
                    {
                        constraints &= ~RotationConstraints.Z;
                    }
                    if ((rb.constraints & RigidbodyConstraints.FreezeRotationZ) != 0)
                    {
                        constraints &= ~RotationConstraints.Y;
                    }
                }
                return constraints;
            }
        }

        public override bool colorEditable
        {
            get { return false; }
        }

        public override int colorId
        {
            get
            {
                return -1;
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        protected override bool isChanged
        {
            get
            {
                return base.isChanged || m_name != m_objInfo.name;
            }
        }

        protected override void ApplyChanges()
        {
            var oldInfo = m_objInfo.Clone();

            base.ApplyChanges();

            m_objInfo.name = m_name;
            m_entity.entityName = m_name;

            m_undo.Record(oldInfo, m_objInfo, m_entity.id);
        }

        public override void Revert()
        {
            m_name = m_objInfo.name;

            base.Revert();
        }
    }
}
