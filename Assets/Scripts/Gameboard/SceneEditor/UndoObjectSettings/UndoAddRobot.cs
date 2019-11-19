using RobotSimulation;
using System;

namespace Gameboard
{
    public class UndoAddRobot
    {
        private readonly UndoManager m_undoManager;
        private readonly Editor m_editor;
        private readonly GameboardSceneManager m_sceneManager;

        public UndoAddRobot(UndoManager undoManager, Editor editor, GameboardSceneManager sceneManager)
        {
            if (undoManager == null)
            {
                throw new ArgumentNullException("undoManager");
            }
            if (editor == null)
            {
                throw new ArgumentNullException("editor");
            }
            if (sceneManager == null)
            {
                throw new ArgumentNullException("sceneManager");
            }

            m_undoManager = undoManager;
            m_editor = editor;
            m_sceneManager = sceneManager;
        }

        public void Record(Robot robot, RobotInfo robotInfo, int robotIndex)
        {
            var cmd = new AddRobotCommand(m_editor, m_sceneManager, robotInfo, robotIndex);
            if (robot)
            {
                cmd.RedoWith(robot);
            }
            m_undoManager.AddUndo(cmd, !robot);
        }
    }
}
