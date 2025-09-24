using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ShiftAnimation", menuName = "Configs/ShiftAnimation")]
public class ShiftAnimation : ScriptableObject
{
    public bool Loop = true;
    public float PlaybackSpeed = 1;
    public bool Overridden = false;
    public Dictionary<string, List<ShiftAnimationData>> AnimationNodes = new Dictionary<string, List<ShiftAnimationData>>();
    [SerializeField] private List<KeyValuePair<string, List<ShiftAnimationData>>> _animationNodes;
    [SerializeField] private List<KeyValuePair<string, Func<Vector2, float[]>>> offset;

    public void Serialize()
    {
        KeyValuePair<string, List<ShiftAnimationData>>.Serialize(AnimationNodes, _animationNodes);
    }
    public void Deserialize()
    {
        KeyValuePair<string, List<ShiftAnimationData>>.Deserialize(AnimationNodes, _animationNodes);
    }
}