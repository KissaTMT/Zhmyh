using UnityEngine;

public class ZhmyhJumpState : DecorateState
{
    public float Progress => Mathf.Clamp01(_progress);
    public CCJumper Jumper => _jumper;
    private CCJumper _jumper;

    private float _progress;
    public ZhmyhJumpState(CharacterController characterController, AnimationCurve curve, float height, float duration, Transform groundChecker, LayerMask layerMask)
    {
        _jumper = new CCJumper(characterController, curve, height, duration, groundChecker, layerMask);
    }
    public override void OnEnter()
    {
        _jumper.PerfomJump();
    }
    public override void OnTick()
    {
        _progress = _jumper.Jump();
    }
}
