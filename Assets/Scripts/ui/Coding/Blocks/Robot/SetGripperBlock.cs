using Robomation;
using System.Collections;
using UnityEngine;

public class SetGripperBlock : RobotBlockBase
{
    private DownMenuPlugins m_menu;

    protected override void Start()
    {
        base.Start();

        m_menu = GetComponentInChildren<DownMenuPlugins>();
    }

    protected override IEnumerator DoAction(BlockState state)
    {
        var robot = state.robot;
        robot.write(Hamster.IO_MODE_A, Hamster.IO_MODE_DO);
        robot.write(Hamster.IO_MODE_B, Hamster.IO_MODE_DO);

        switch (m_menu.GetMenuValue())
        {
        case "gripper_state_open":
            robot.write(Hamster.OUTPUT_A, 1);
            robot.write(Hamster.OUTPUT_B, 0);
            break;

        case "gripper_state_close":
            robot.write(Hamster.OUTPUT_A, 0);
            robot.write(Hamster.OUTPUT_B, 1);
            break;

        case "gripper_state_off":
            robot.write(Hamster.OUTPUT_A, 0);
            robot.write(Hamster.OUTPUT_B, 0);
            break;
        }

        yield break;
    }
}
