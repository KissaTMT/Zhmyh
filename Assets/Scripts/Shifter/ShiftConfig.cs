using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShiftConfig", menuName = "Configs/ShiftConfig")]
public class ShiftConfig : ScriptableObject
{
    public Vector2 Direction;
    
    public Dictionary<string, ShiftTransformData> ShiftTransformData = new();
    public Dictionary<string, Sprite> ShiftVisualData = new();

    [SerializeField, Header("Transform")] private List<KeyValuePair<string, ShiftTransformData>> _shiftTransformData = new();
    [SerializeField, Header("Visual")] private List<KeyValuePair<string, Sprite>> _shiftVisualData = new();

    public void Serialize()
    {
        KeyValuePair<string, ShiftTransformData>.Serialize(ShiftTransformData, _shiftTransformData);
        KeyValuePair<string, Sprite>.Serialize(ShiftVisualData, _shiftVisualData);
    }

    public void Deserialize()
    {
        KeyValuePair<string, ShiftTransformData>.Deserialize(ShiftTransformData, _shiftTransformData);
        KeyValuePair<string, Sprite>.Deserialize(ShiftVisualData, _shiftVisualData);
    }
}
