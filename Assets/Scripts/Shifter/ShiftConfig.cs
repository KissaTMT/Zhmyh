using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShiftConfig : ScriptableObject
{
    public Vector2 Direction;
    public List<Vector3> LocalPositions = new List<Vector3>();
    public List<Vector3> LocalScales = new List<Vector3>();
    public List<Vector3> EulerAngles = new List<Vector3>();
    public List<Sprite> Sprites = new List<Sprite>();
}
