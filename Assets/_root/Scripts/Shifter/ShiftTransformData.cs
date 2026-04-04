using System;
using UnityEngine;

[Serializable]
public struct ShiftTransformData
{
    public string Name;
    public Vector3 Position;
    public Vector3 Scale;
    public float Angle;
    

    public ShiftTransformData(string name, Vector3 position, Vector3 scale, float angle)
    {
        Name = name;
        Position = position;
        Scale = scale;
        Angle = angle;
    }
    public string GetParentName() => Name.Split('/')[1];
}
