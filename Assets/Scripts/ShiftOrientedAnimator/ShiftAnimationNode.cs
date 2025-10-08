using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShiftAnimationNode
{
    //private static float[,] identityMatrix = new float[2, 2] { { 1, 0 }, { 0, 1 } };
    private const string RH = "Right hand";
    private const string LH = "Left hand";
    private const string RL = "Right leg";
    private const string LL = "Left leg";
    private const string T = "tie";
    public bool Enabled { get; set; }
    public IReadOnlyDictionary<string, List<ShiftAnimationData>> Animations => _animations;
    public Transform Transform => _transform;
    private Dictionary<string, List<ShiftAnimationData>> _animations;
    private ShiftNode _shiftNode;
    private Transform _transform;

    private string _name;
    private string _currentAnimation;
    public ShiftAnimationNode(ShiftNode node)
    {
        Enabled = true;
        _shiftNode = node;
        _transform = node.Transform;
        _name = _transform.name;
        _animations = new Dictionary<string, List<ShiftAnimationData>>();

    }
    public void AddClip(string name, List<ShiftAnimationData> clip)
    {
        _animations.Add(name, clip);
    }

    public void SetAnimation(string animation)
    {
        _currentAnimation = animation;
    }
    public void Animate(float progress, Vector2 direction)
    {
        if (!_animations.ContainsKey(_currentAnimation) || _animations[_currentAnimation].Count == 0) return;

        var clip = _animations[_currentAnimation];

        int nextIndex = FindIndex(clip, progress);

        var previousIndex = nextIndex - 1;
        var previous = clip[previousIndex];
        var next = clip[nextIndex];

        var t = Mathf.Clamp01((progress - previous.TimeKey) / (next.TimeKey - previous.TimeKey));


        _transform.localPosition = Vector3.Lerp(GetPosition(GetOffset(previous.PositionKey, direction)),
            GetPosition(GetOffset(next.PositionKey, direction)), t);

        _transform.localEulerAngles = new Vector3(0, 0,
            Mathf.LerpAngle(Mathf.Sign(direction.x) * previous.AngleKey, Mathf.Sign(direction.x) * next.AngleKey, t));

        //_transform.localScale = Vector2.Lerp(prevData.ScaleKey, nextData.ScaleKey, t);
    }
    public void SetAnimationData(float timeKey, Vector2 direction)
    {
        var clip = _animations[_currentAnimation];
        _transform.localPosition = GetPosition(GetOffset(clip[FindIndex(clip,timeKey)].PositionKey, direction));
        _transform.localEulerAngles = new Vector3(0, 0, Mathf.Sign(direction.x) * clip[FindIndex(clip, timeKey)].AngleKey);
        //_transform.localScale = clip[clip.FindIndex(d => d.TimeKey >= timeKey)].ScaleKey;
    }
    private Vector2 GetOffset(Vector2 v, Vector2 direction)
    {
        var result = v;

        if (_name == RL || _name == LL) result = new Vector2(direction.x > 0 ? v.x : -v.x, v.y);
        else if (_name == RH || _name == LH) result = new Vector2(direction.x >= 0 ? v.x  : v.x - 10, v.y);
        else if (_name == T) result = (Vector2)_shiftNode.CurrentView.Position + v;

        return result;
    }
    private int FindIndex(List<ShiftAnimationData> clip, float progress)
    {
        var result = -1;

        for (var i = 0; i < clip.Count; i++)
        {
            if (clip[i].TimeKey >= progress)
            {
                result = i;
                break;
            }
        }

        return result;
    }
    private Vector3 GetPosition(Vector2 pos) => new Vector3(pos.x, pos.y, _transform.localPosition.z);
}