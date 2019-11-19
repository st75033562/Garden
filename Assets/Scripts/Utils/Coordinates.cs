using UnityEngine;

// Utility class for converting vectors between unity and app.
// The app coordinate system is used in Gameboard and AR for user inputs.
//
// The Table below lists the correspondence of axes between two systems
// Unity    App
//   +x     +x 
//   +y     +z 
//   +z     +y
public static class Coordinates
{
    public static Vector3 ConvertVector(Vector3 pos)
    {
        return new Vector3(pos.x, pos.z, pos.y);
    }

    public static Vector3 ConvertRotation(Vector3 rot)
    {
        return new Vector3(-rot.x, -rot.z, -rot.y);
    }
}
