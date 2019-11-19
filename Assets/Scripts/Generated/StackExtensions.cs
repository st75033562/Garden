public partial class Save_GameboardObjectAssetInfo
{
    partial void OnConstruction()
    {
        NextObjectNum = 1;
    }
}

public partial class Save_GameboardRobot
{
    partial void OnConstruction()
    {
        Scale = new Save_Vector3 {
            X = 1,
            Y = 1,
            Z = 1
        };
    }
}

public partial class Save_Vector3
{
    public UnityEngine.Vector3 ToVector3()
    {
        return new UnityEngine.Vector3(X, Y, Z);
    }

    public Save_Vector3(UnityEngine.Vector3 v)
    {
        X = v.x;
        Y = v.y;
        Z = v.z;
    }
}

public partial class Save_Vector2
{
    public Save_Vector2(float x, float y)
    {
        this.x_ = x;
        this.y_ = y;
    }

    public UnityEngine.Vector2 ToVector2()
    {
        return new UnityEngine.Vector3(X, Y);
    }
}

public partial class Save_Variable
{
    partial void OnConstruction()
    {
        GlobalVarOwner = Save_GlobalVarOwner.Invalid;
    }
}