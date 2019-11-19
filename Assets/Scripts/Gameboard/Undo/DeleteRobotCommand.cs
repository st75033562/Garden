using RobotSimulation;
using System;

namespace Gameboard
{
    public class DeleteRobotCommand : BaseUndoCommand
    {
        private readonly AddRobotCommand m_addCommand;
        private readonly bool m_wasSelected;
        private readonly int m_entityId;
        private readonly int m_robotIndex;
        private readonly Editor m_editor;
        private readonly GameboardSceneManager m_sceneManager;

        public DeleteRobotCommand(Editor editor, GameboardSceneManager sceneManager, int index)
            : base(true)
        {
            m_robotIndex = index;
            m_sceneManager = sceneManager;
            m_editor = editor;

            var robot = sceneManager.robotManager.GetRobot(index);
            var entity = robot.GetComponent<Entity>();
            m_wasSelected = entity == editor.selectedEntity;
            m_entityId = entity.id;

            m_addCommand = new AddRobotCommand(editor, sceneManager, sceneManager.gameboard.robots[index], index);
            m_addCommand.onRobotCreated += OnRobotCreated;
        }

        private void OnRobotCreated(Robot robot)
        {
            robot.GetComponent<Entity>().id = m_entityId;
        }

        protected override void UndoImpl()
        {
            m_addCommand.Redo();

            if (m_wasSelected)
            {
                var robot = m_sceneManager.robotManager.GetRobot(m_robotIndex);
                m_editor.selectedEntity = robot.GetComponent<Entity>();
            }
        }

        protected override void RedoImpl()
        {
            m_addCommand.Undo();
        }
    }
}
