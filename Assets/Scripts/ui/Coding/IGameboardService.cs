using System.Collections;
using UnityEngine;

public interface IGameboardService
{
    void SetRobotScore(int robotIndex, int score);
    void SetRobotScore(int score);

    string GetRobotNickname(int index);

    IEnumerator StartRobotCode();

    void StopRobotCode();

    Vector2 mouseWorldPosition { get; }

    Vector2 mouseScreenPosition { get; }

    Vector2 screenSize { get; }

    /// <summary>
    /// get the position of the marker in world space, return Vector3.zero if id is not valid
    /// </summary>
    Vector3 GetMarkerPosition(int markerId);

    /// <summary>
    /// get the yaw of the marker in world space, return 0 if id is not valid
    /// </summary>
    float GetMarkerRotation(int markerId);
}
