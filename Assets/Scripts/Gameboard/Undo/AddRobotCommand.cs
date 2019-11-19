using RobotSimulation;
using System;

namespace Gameboard
{
    public class AddRobotCommand : BaseUndoCommand
    {
        public event Action<Robot> onRobotCreated;

        private readonly Editor m_editor;
        private readonly GameboardSceneManager m_sceneManager;
        private readonly RobotInfo m_robotInfo;
        private readonly int m_robotIndex;
        private readonly int m_prevEntityId;

        public AddRobotCommand(Editor editor, GameboardSceneManager sceneManager, RobotInfo robotInfo, int robotIndex = -1)
            : base(true)
        {
            if (editor == null)
            {
                throw new ArgumentNullException("editor");
            }

            if (sceneManager == null)
            {
                throw new ArgumentNullException("sceneManager");
            }

            if (robotInfo == null)
            {
                throw new ArgumentNullException("robotInfo");
            }

            if (robotIndex > sceneManager.gameboard.robots.Count)
            {
                throw new ArgumentOutOfRangeException("robotIndex");
            }

            m_editor = editor;
            m_sceneManager = sceneManager;
            m_robotInfo = robotInfo;
            m_robotIndex = robotIndex < 0 ? sceneManager.gameboard.robots.Count : robotIndex;
            m_prevEntityId = sceneManager.objectManager.prevEntityId;
        }

        protected override void UndoImpl()
        {
            m_sceneManager.robotManager.RemoveRobot(m_robotIndex);
            m_sceneManager.gameboard.RemoveRobot(m_robotIndex);
            m_sceneManager.objectManager.prevEntityId = m_prevEntityId;
        }

        protected override void RedoImpl()
        {
            var robot = m_sceneManager.robotManager.CreateRobot(m_robotInfo);
            m_editor.SetupEntity(robot.GetComponent<Entity>(), true);

            if (onRobotCreated != null)
            {
                onRobotCreated(robot);
            }

            RedoWith(robot);
        }

        public void RedoWith(Robot robot)
        {
            m_sceneManager.robotManager.InsertRobot(m_robotIndex, robot);
            m_sceneManager.gameboard.InsertRobot(m_robotIndex, m_robotInfo);
        }
    }
}
