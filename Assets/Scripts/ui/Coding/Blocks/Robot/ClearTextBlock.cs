using System.Collections;

public class ClearTextBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        CodeContext.textPanel.Clear();
        yield break;
    }
}
