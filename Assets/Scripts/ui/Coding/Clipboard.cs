using System.Collections.Generic;

public enum CodeType
{
    Robot,
    Gameboard,
}

public static class Clipboard
{
    public static BlockSaveStates nodeStates { get; set; }

    public static bool isEmpty
    {
        get { return nodeStates == null || nodeStates.isEmpty; }
    }

    public static CodeType type { get; set; }

    public static string workspaceId { get; set; }
}
