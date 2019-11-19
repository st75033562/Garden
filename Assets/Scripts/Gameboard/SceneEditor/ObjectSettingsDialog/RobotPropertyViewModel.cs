using RobotSimulation;
using System;

namespace Gameboard
{
    public class RobotPropertyViewModel : ObjectPropertyViewModelBase<RobotInfo>
    {
        public event Action onColorChanged;

        private readonly Robot m_robot;
        private readonly RobotInfo m_robotInfo;
        private readonly IObjectSettingsUndo<RobotInfo> m_undoChanges;
        private readonly UndoAddRobot m_undoAdd;

        private RobotPropertyViewModel(Robot robot, RobotInfo robotInfo)
            : base(robot.GetComponent<Entity>(), robotInfo)
        {
            m_robot = robot;
            m_robotInfo = robotInfo;
        }

        public RobotPropertyViewModel(Robot robot, RobotInfo robotInfo, IObjectSettingsUndo<RobotInfo> undoChagnes)
            : this(robot, robotInfo)
        {
            if (robot.robotIndex == -1)
            {
                throw new ArgumentException("robot must not be new");
            }

            if (undoChagnes == null)
            {
                throw new ArgumentNullException("undoChanges");
            }

            m_undoChanges = undoChagnes;
        }

        public RobotPropertyViewModel(Robot robot, RobotInfo robotInfo, UndoAddRobot undoAdd)
            : this(robot, robotInfo)
        {
            if (robot.robotIndex != -1)
            {
                throw new ArgumentException("robot must be new");
            }

            if (undoAdd == null)
            {
                throw new ArgumentNullException("undoAdd");
            }

            m_undoAdd = undoAdd;
        }

        public override bool nameEditable
        {
            get { return false; }
        }

        public override string name
        {
            get
            {
                if (m_robot.robotIndex == -1)
                {
                    return "gameboard_robot_placement_title_new".Localize();
                }
                else
                {
                    return "gameboard_robot_placement_title".Localize(m_robot.robotIndex);
                }
            }
            set
            {
                throw new InvalidOperationException();
            }
        }

        public override bool IsDuplicateName(string name)
        {
            throw new InvalidOperationException();
        }

        // TODO: support for scale editing
        public override bool scaleEditable
        {
            get { return false; }
        }

        public override RotationConstraints rotationConstraints
        {
            get { return RotationConstraints.Z; }
        }

        public override bool colorEditable
        {
            get { return true; }
        }

        public override int colorId
        {
            get
            {
                return m_robot.GetComponent<RobotColor>().colorId;
            }
            set
            {
                if (colorId != value)
                {
                    var hasError = m_robot.GetComponent<PlacementErrorDetector>().hasError;
                    m_robot.GetComponent<RobotColor>().SetColor(value, !hasError);

                    if (onColorChanged != null)
                    {
                        onColorChanged();
                    }
                }
            }
        }

        protected override bool isChanged
        {
            get
            {
                return base.isChanged || colorId != m_robotInfo.colorId;
            }
        }

        protected override void ApplyChanges()
        {
            if (m_robot.robotIndex != -1)
            {
                var oldInfo = m_robotInfo.Clone();
                SaveChanges();
                m_undoChanges.Record(oldInfo, m_robotInfo, m_entity.id);
            }
            else
            {
                SaveChanges();
                m_undoAdd.Record(m_robot, m_robotInfo, -1);
            }
        }

        private void SaveChanges()
        {
            m_robotInfo.colorId = colorId;
            base.ApplyChanges();
        }

        public override void Revert()
        {
            colorId = m_robotInfo.colorId;

            base.Revert();
        }
    }
}
