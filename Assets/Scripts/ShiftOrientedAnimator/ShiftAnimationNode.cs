using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class ShiftAnimationNode
{
    public bool Enabled { get; set; }
    public IReadOnlyDictionary<string, List<ShiftAnimationData>> Animations => _animations;
    public Transform Transform => _transform;
    private Dictionary<string, List<ShiftAnimationData>> _animations;
    private Transform _transform;

    private string _currentAnimation;
    public ShiftAnimationNode(Transform transform)
    {
        Enabled = true;
        _transform = transform;
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

        int nextIndex = clip.FindIndex(d => d.TimeKey >= progress);

        var previousIndex = nextIndex - 1;
        var previous = clip[previousIndex];
        var next = clip[nextIndex];

        var t = Mathf.Clamp01((progress - previous.TimeKey) / (next.TimeKey - previous.TimeKey));

        _transform.localPosition = Vector3.Lerp(GetPosition(GetOffset(previous.PositionKey, direction)),
            GetPosition(GetOffset(next.PositionKey, direction)),t);
        _transform.localEulerAngles = new Vector3(0, 0, 
            Mathf.LerpAngle(Mathf.Sign(direction.x) * previous.AngleKey, Mathf.Sign(direction.x) * next.AngleKey, t));
        //_transform.localScale = Vector2.Lerp(prevData.ScaleKey, nextData.ScaleKey, t);
    }
    public void SetAnimationData(float timeKey, Vector2 direction)
    {
        var clip = _animations[_currentAnimation];
        _transform.localPosition = GetPosition(GetOffset(clip[clip.FindIndex(d => d.TimeKey >= timeKey)].PositionKey,direction));
        _transform.localEulerAngles = new Vector3(0, 0, Mathf.Sign(direction.x) * clip[clip.FindIndex(d => d.TimeKey >= timeKey)].AngleKey);
        //_transform.localScale = clip[clip.FindIndex(d => d.TimeKey >= timeKey)].ScaleKey;
    }
    private Vector2 GetOffset(Vector2 v, Vector2 direction)
    {
        if (_transform.name == "Right leg" || _transform.name == "Left leg")
        {
            var m = new float[2, 2] { { direction.x, 0 }, { 0, 1 } };
            return new Vector2(m[0, 0] * v.x + m[0, 1] * v.y, m[1, 0] * v.x + m[1, 1] * v.y);
        }
        else if (_transform.name == "Right hand" || _transform.name == "Left hand") return new Vector2(direction.x <= 0 ? v.x : v.x, v.y);
        else return v;
    }
    private Vector3 GetPosition(Vector2 pos) => new Vector3(pos.x,pos.y, _transform.localPosition.z);
}