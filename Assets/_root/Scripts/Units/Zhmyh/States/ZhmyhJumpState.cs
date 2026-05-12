using UnityEngine;

public class ZhmyhJumpState : State
{
    public float Progress => Mathf.Clamp01(_progress);
    public CCJumper Jumper => _jumper;
    private CCJumper _jumper;
    private CharacterController _characterController;
    private float _progress;

    private Vector3 _direction;
    private float _speedInAir;
    public ZhmyhJumpState(Transform transform, AnimationCurve curve, float speedInAir,float height, float duration, Transform groundChecker, LayerMask layerMask)
    {
        _characterController = transform.GetComponent<CharacterController>();
        _jumper = new CCJumper(transform, curve, height, duration, groundChecker, layerMask);
        _speedInAir = speedInAir;
    }
    public override void OnEnter()
    {
        _jumper.PerfomJump();
    }
    public override void OnTick()
    {
        _progress = _jumper.Jump();
        _characterController.Move(_direction * _speedInAir * Time.deltaTime);
    }
    public void SetDirection(Vector3 direction)
    {
        _direction = direction;
    }
}
