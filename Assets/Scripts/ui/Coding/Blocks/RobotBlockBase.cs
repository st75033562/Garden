using System.Collections;
using System.Collections.Generic;

public abstract class RobotBlockBase : BlockBehaviour
{
    public struct BlockState
    {
        private readonly CodeContext m_codeContext;

        public BlockState(CodeContext codeContext, int robotIndex, IList<string> slotValues)
        {
            m_codeContext = codeContext;
            this.robotIndex = robotIndex;
            this.slotValues = slotValues;
        }

        public int robotIndex { get; private set; }

        public IList<string> slotValues { get; private set; }

        public IRobot robot
        {
            get { return m_codeContext.robotManager.get(robotIndex); }
        }

        public RobotRuntimeState runtimeState
        {
            get { return m_codeContext.robotRuntime.GetState(robotIndex); }
        }
        public void SetStateCount(int count)
        {
            m_codeContext.robotRuntime.SetStateCount(count);
        }
    }

    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        //int robotIndex;
        //if (!int.TryParse(slotValues[0], out robotIndex))
        //{
        //    yield break;
        //}

        var state = new BlockState(CodeContext, 0, slotValues);
        if (state.robot == null)
        {
            yield break;
        }

        yield return DoAction(state);
    }

    protected abstract IEnumerator DoAction(BlockState state);

}
