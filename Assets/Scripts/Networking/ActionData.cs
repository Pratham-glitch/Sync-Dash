using UnityEngine;

[System.Serializable]
public class ActionData
{
    public ActionType actionType;
    public float timestamp;
    public Vector3 position;
}

public enum ActionType
{
    Jump,
    Collect,
    Collision
}