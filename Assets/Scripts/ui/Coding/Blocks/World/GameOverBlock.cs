using Gameboard;
using System.Collections;
using System.Collections.Generic;

public class GameOverBlock : BlockBehaviour
{
    public override IEnumerator ActionBlock(ThreadContext context)
    {
        var slotValues = new List<string>();
        yield return Node.GetSlotValues(context, slotValues);
        int score;
        int.TryParse(slotValues[0], out score);
        UIGameboard board = FindObjectOfType<UIGameboard>();
        if (board != null)
        {
            board.RunAndSubmit();
        }
        //只为比较git新branch tortoise加此一行
        CodeContext.gameboardService.SetRobotScore(score);

    }
}
