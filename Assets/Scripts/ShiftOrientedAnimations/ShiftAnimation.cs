using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ShiftAnimation", menuName = "Configs/ShiftAnimation")]
public class ShiftAnimation : ScriptableObject
{
    public bool Loop = true;
    public float PlaybackSpeed = 1;
    public bool Overridden = false;
    public Dictionary<string, List<ShiftAnimationData>> AnimationNodes = new Dictionary<string, List<ShiftAnimationData>>();
    [SerializeField] private List<KeyValuePair<string, List<ShiftAnimationData>>> _animationNodes;

    public void Serialize()
    {
        KeyValuePair<string, List<ShiftAnimationData>>.Serialize(AnimationNodes, _animationNodes);
    }
    public void Deserialize()
    {
        KeyValuePair<string, List<ShiftAnimationData>>.Deserialize(AnimationNodes, _animationNodes);
    }
}