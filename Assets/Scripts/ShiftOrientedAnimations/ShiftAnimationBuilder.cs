using NaughtyAttributes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_EDITOR
public class ShiftAnimationBuilder : MonoBehaviour
{
    [SerializeField] private ShiftAnimation _animation;
    [SerializeField] private Transform _node;
    [SerializeField, Range(0,1)] private float _timeKey;

    
    private Coroutine _coroutine;
    private int _index = 0;

    [Button("Play")]
    private void Play()
    {
        _animation.Deserialize();
        //if (_coroutine != null) Stop();
        //_coroutine = StartCoroutine(PlayRoutine());
    }
    [Button("Stop")]
    private void Stop()
    {
        if (_coroutine == null) return;
        StopCoroutine(_coroutine);
        _coroutine = null;
    }
    [Button("Add animation key")]
    private void AddAnimationKey()
    {
        if (_node == null)
        {
            Debug.LogWarning("Node is not instance");
            return;
        }
        _animation.Deserialize();
        AddAnimationKey(_animation.AnimationNodes, new ShiftAnimationData(_timeKey,
            _node.localPosition,
            _node.localScale,
            _node.localEulerAngles.z>180? _node.localEulerAngles.z - 360: _node.localEulerAngles.z));
        _animation.AnimationNodes[Shifter.GetPath(_node)] = _animation.AnimationNodes[Shifter.GetPath(_node)].OrderBy(i => i.TimeKey).ToList();
        _animation.Serialize();

        //UnityEditor.EditorUtility.SetDirty(_animation);
        //UnityEditor.AssetDatabase.SaveAssets();
    }
    [Button("Next Key")]
    private void NextKey()
    {
        Debug.Log(_index);
        _animation.Deserialize();
        var keys = _animation.AnimationNodes[Shifter.GetPath(_node)];

        var pos = keys[_index].PositionKey;
        var angle = keys[_index].AngleKey;
        _node.localPosition = new Vector3(pos.x,pos.y,_node.localPosition.z);
        _node.localEulerAngles = new Vector3(0, 0, angle);
        _index = (_index+1)%keys.Count;
    }
    //private void OnValidate()
    //{
    //    Debug.Log(_timeKey);
    //}
    private void AddAnimationKey<TValue>(Dictionary<string, List<TValue>> keys, TValue value)
    {
        if(keys.ContainsKey(Shifter.GetPath(_node))) keys[Shifter.GetPath(_node)].Add(value);
        else keys[Shifter.GetPath(_node)] = new List<TValue>() { value};
    }
    //private IEnumerator PlayRoutine()
    //{
    //    var children = GetComponentsInChildren<SpriteRenderer>();
    //
    //    var keys = _animation.AnimationNodes.Keys.ToList();
    //    var nodes = new List<Transform>(keys.Count);
    //
    //    for(var i = 0; i < keys.Count; i++)
    //    {
    //        nodes.Add(children.FirstOrDefault(item => item.name == keys[i]).GetComponent<Transform>());
    //    }
    //
    //    while (_animation.Loop)
    //    {
    //        for (var timeKey = 0f; timeKey < 1f; timeKey += _animation.PlaybackSpeed * Time.deltaTime)
    //        {
    //            for (var i = 0; i < nodes.Count; i++)
    //            {
    //                var animation = _animation.AnimationNodes[nodes[i].name];
    //                var nextAnimationNode = animation.OrderBy(item => Mathf.Abs(timeKey - item.TimeKey)).Where(item => item.TimeKey >= timeKey).ToList()[0];
    //
    //                var nextAnimationNodeIndex = animation.IndexOf(nextAnimationNode);
    //                var currentAnimationNode = nextAnimationNodeIndex == 0 ? nextAnimationNode : animation[nextAnimationNodeIndex-1];
    //                if ((Vector2)nodes[i].localPosition != nextAnimationNode.PositionKey)
    //                {
    //                    nodes[i].localPosition = Vector2.Lerp(currentAnimationNode.PositionKey,
    //                        nextAnimationNode.PositionKey, timeKey);
    //                }
    //                if (Mathf.Abs(nodes[i].localEulerAngles.z - nextAnimationNode.AngleKey)>0.01f)
    //                {
    //                    nodes[i].localRotation = Quaternion.Euler(nodes[i].localRotation.x, nodes[i].localRotation.y,
    //                        Mathf.Lerp(currentAnimationNode.AngleKey, nextAnimationNode.AngleKey, timeKey));
    //                }
    //            }
    //            yield return null;
    //        }
    //    }
    //}
}
#endif