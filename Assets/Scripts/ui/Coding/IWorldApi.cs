using System.Collections.Generic;
using UnityEngine;

public interface IWorldApi
{
    void SetRobotPosition(int index, Vector3 pos);
    Vector2 GetRobotPosition(int index);

    void SetRobotRotaton(int index, float rotation);
    float GetRobotRotation(int index);

    AsyncRequest<int> CreateObject(int assetId);

    void SetObjectPosition(int objectId, Vector3 pos);
    Vector3 GetObjectPosition(int objectId);

    void SetObjectRotation(int objectId, float rotation);
    void SetObjectRotation(int objectId, float rotationAlpha, float rotationBate, float rotationTheta);
    float GetObjectRotation(int objectId);

    void SetObjectScale(int objectId, Vector3 scale);

    void MoveObject(int objectId, float angularSpeed, float linearSpeed);

    void SetObjectCollision(int objectId, bool enabled);

    void SetObjectMass(int objectId, float mass);

    void ShowObject(int objectId, bool visible);

    void DeleteObject(int objectId);

    void PlayAction(int objectId, int actionId, params string[] args);

    void AttachObjectToObject(int sourceObjectId, int targetObjectId);
    void AttachObjectToRobot(int sourceObjectId, int robotIndex);

    void DetachObject(int objectId);

    void PrintTextOnObject(int objectId, string text, int size, Color color);

    void SetLightFlux(int objectId, float flux);

    IEnumerable<int> GetRobotCollidedObjects(int robotIndex, bool clear);

    IEnumerable<int> GetObjectCollidedObjects(int objectId, bool clear);

    int TakeObjectFirstCollidedObject(int objectId);

    void RotateTowards(int objectId, Vector2 pos);

    void MoveTowards(int objectId, int targetId);

    void SetTrigger(int objectId, bool isTrigger);
}
