using System;
using UnityEngine;

[CreateAssetMenu(fileName = "ShiftAnimationOffset", menuName = "Configs/ShiftAnimationOffset")]
public class ShiftAnimationOffset : ScriptableObject
{
    public Vector2 OffsetMatrix(Vector2 v,params float[] args)
    {
        var m = new float[2, 2] { { args[0], args[1] }, { args[2], args[3] } };
        return new Vector2(m[0, 0] * v.x + m[0, 1] * v.y, m[1, 0] * v.x + m[1, 1] * v.y);
    }
}