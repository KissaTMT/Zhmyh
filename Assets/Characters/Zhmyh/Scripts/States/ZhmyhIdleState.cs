public class ZhmyhIdleState : State
{
    private ShiftAnimator _shiftAnimator;
    public ZhmyhIdleState(ShiftAnimator shiftAnimator)
    {
        _shiftAnimator = shiftAnimator;
    }
    public override void OnEnter()
    {
        _shiftAnimator.SetAnimation("idle");
    }
}
