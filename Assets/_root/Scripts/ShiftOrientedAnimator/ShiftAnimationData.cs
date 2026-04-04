using System;
using UnityEngine;

[Serializable]
public struct ShiftAnimationData
{
    [Range(0,1)] public float TimeKey;
    public Vector2 PositionKey;
    public Vector2 ScaleKey;
    public float AngleKey;

    public ShiftAnimationData(float timeKey, Vector2 positionKey, Vector2 scaleKey, float angleKey)
    {
        if (timeKey < 0 || timeKey > 1) throw new ArgumentException("TimeKey must be in range[0,1]");
        TimeKey = timeKey;
        PositionKey = positionKey;
        ScaleKey = scaleKey;
        AngleKey = angleKey;
    }
    public override string ToString() => TimeKey.ToString();
}
