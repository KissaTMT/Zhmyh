using ArtMesh;
using Components;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Тестовая заглушка Жмыха на ArtMesh. Геймплей и публичный контракт те же,
/// восьмисторонний шифт заменён поворотом Root по направлению движения.
/// Root — первый ребёнок, либо явно заданный _artRoot. Вперёд модели = +Z.
/// </summary>
public class ArtMeshZhmyh : Zhmyh
{
    public override State CurrentState => _sm.CurrentState;

    [SerializeField] Transform _artRoot;

    Quaternion _rootBindLocal;
    Quaternion _restFacing;
    FacingViewSource _viewSource;
    ChildBind[] _visualBinds;
    StateMachine _sm;
    CharacterController _characterController;
    IMovementHandler _movementHandler;
    Gravity _gravity;
    IGroundChecker _groundChecker;

    IdleState _idleState;
    MoveState _movementState;
    ZhmyhClimbState _climbState;
    AimState _aimingState;
    DashState _dashState;
    JumpState _jumpState;

    Vector3 _movementDirection;
    bool _isClimbing;
    bool _isAiming;
    bool _isDashing;
    bool _isJumping;
    bool _isGrounded;
    float _coyoteTime;

    protected override void OnInit()
    {
        _root = _artRoot != null ? _artRoot : transform.GetChild(0);
        _rootBindLocal = _root.localRotation;
        _characterController = GetComponent<CharacterController>();
        health = new Health(_maxHealth);

        var forward = _root.forward;
        forward.y = 0f;
        NotZeroMovementDirection = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
        _restFacing = Quaternion.LookRotation(NotZeroMovementDirection, Vector3.up);

        CacheVisualBinds();
        BindArtMesh();

        _movementHandler = new CCMovementHandler(_characterController);
        _gravity = new Gravity();
        _groundChecker = new GroundSphereOverlapChecker(LayerMask.GetMask("Default", "Ground"), transform);

        SetupStateMachine();
    }

    public override void Tick()
    {
        _isGrounded = CheckGround();

        if (_isGrounded)
        {
            _gravity.Disable();
            _coyoteTime = 0;
        }
        else
        {
            _coyoteTime += Time.deltaTime;
            if (_gravity.Enabled == false) _gravity.SetVelocity(4);
            _gravity.Enable();
        }

        _sm.Tick();
        if (_gravity.Enabled) _gravity.Tick(Time.deltaTime);
        _movementHandler.Tick(Time.deltaTime);
        Face(Facing());
    }

    public override void SetMovementDirection(Vector3 input)
    {
        if (_movementDirection == input) return;
        _movementDirection = input;

        _idleState.SetDirection(_movementDirection);
        _movementState.SetDirection(_movementDirection);
        _climbState.SetDirection(new Vector2(_movementDirection.x, _movementDirection.z));
        _dashState.SetDirection(_movementDirection);
        _jumpState.SetDirection(_movementDirection);

        if (_movementDirection == Vector3.zero) return;
        NotZeroMovementDirection = _movementDirection;
    }

    public override void SetLookDirection(Vector3 input) { }

    public override void SetShootDirection(Vector3 input) => _aimingState.SetShootDirection(input);

    public override void Dash()
    {
        if (CurrentState is DashState dash && dash.Progress < 1f) return;
        _isDashing = true;
    }

    public override void Jump()
    {
        if (_coyoteTime > 0.15f) return;
        _isJumping = true;
    }

    public override void Climb()
    {
        if (_isClimbing) _isClimbing = false;
        else if (_climbState.Climber.IsClimb()) _isClimbing = true;
    }

    public override void SetAim(bool isAiming) => _isAiming = isAiming;

    public override void Pull(bool isPull)
    {
        return;
        if (CurrentState is AimState == false) SetAim(isPull);
        _aimingState.SetPull(isPull);
        if (!isPull) SetAim(false);
    }

    void LateUpdate()
    {
        _isDashing = false;
        _isJumping = false;
    }

    bool CheckGround()
    {
        float bottom = _characterController.center.y - _characterController.height * 0.5f;
        Vector3 feet = transform.position + Vector3.up * bottom;
        return _groundChecker.Check(feet + Vector3.down * 0.05f, 0.08f);
    }

    Vector3 Facing()
    {
        if (CurrentState is DashState dash && dash.Direction.sqrMagnitude > 0.0001f) return dash.Direction;
        if (_movementDirection.sqrMagnitude > 0.01f) return _movementDirection;
        return NotZeroMovementDirection;
    }

    void Face(Vector3 worldDirection)
    {
        worldDirection.y = 0f;
        if (_root == null || _root.parent == null || worldDirection.sqrMagnitude < 0.0001f) return;

        Vector3 bindForward = _root.parent.rotation * (_rootBindLocal * Vector3.forward);
        bindForward.y = 0f;
        if (bindForward.sqrMagnitude < 0.0001f) bindForward = Vector3.forward;

        Quaternion yaw = Quaternion.FromToRotation(bindForward.normalized, worldDirection.normalized);
        _root.rotation = yaw * (_root.parent.rotation * _rootBindLocal);
        HoldVisualWorldRotation();

        if (_viewSource == null) return;
        Vector3 facing = worldDirection.normalized;
        facing.y = 0f;
        _viewSource.InternalForward = facing.sqrMagnitude > 0.0001f ? facing : NotZeroMovementDirection;
    }

    void CacheVisualBinds()
    {
        int count = _root.childCount;
        _visualBinds = new ChildBind[count];
        for (int i = 0; i < count; i++)
        {
            Transform child = _root.GetChild(i);
            _visualBinds[i] = new ChildBind { Transform = child, WorldRotation = child.rotation };
        }
    }

    void HoldVisualWorldRotation()
    {
        if (_visualBinds == null) return;
        for (int i = 0; i < _visualBinds.Length; i++)
        {
            Transform child = _visualBinds[i].Transform;
            if (child == null) continue;
            child.rotation = _visualBinds[i].WorldRotation;
        }
    }

    void BindArtMesh()
    {
        ArtMeshRenderer[] renderers = GetComponentsInChildren<ArtMeshRenderer>(true);
        _viewSource = new FacingViewSource(_restFacing);
        for (int i = 0; i < renderers.Length; i++)
        {
            _viewSource.Remember(renderers[i].transform);
            renderers[i].ViewSource = _viewSource;
        }
        _viewSource.InternalForward = NotZeroMovementDirection;
    }

    struct ChildBind
    {
        public Transform Transform;
        public Quaternion WorldRotation;
    }

    /// <summary>
    /// Камера и внутреннее направление персонажа считаются здесь.
    /// ArtMesh получает уже готовый локальный вектор и сам камеру не читает.
    /// </summary>
    sealed class FacingViewSource : IArtMeshViewSource
    {
        public Vector3 InternalForward;
        readonly Quaternion _restFacing;
        readonly Dictionary<Transform, Quaternion> _bindWorld = new Dictionary<Transform, Quaternion>();
        Vector3 _last = Vector3.forward;

        public FacingViewSource(Quaternion restFacing) => _restFacing = restFacing;

        public void Remember(Transform part)
        {
            if (part != null) _bindWorld[part] = part.rotation;
        }

        public Vector3 Resolve(ArtMeshAsset asset, Transform part, ArtMeshViewBlender blender, ArtMeshState state)
        {
            Camera cam = Camera.main;
            if (cam != null && part != null)
            {
                Vector3 forward = InternalForward;
                forward.y = 0f;
                if (forward.sqrMagnitude < 0.0001f) forward = Vector3.forward;

                Quaternion facing = Quaternion.LookRotation(forward.normalized, Vector3.up);
                Quaternion turn = facing * Quaternion.Inverse(_restFacing);
                Quaternion bind = _bindWorld.TryGetValue(part, out Quaternion cached) ? cached : part.rotation;
                Vector3 toCam = cam.transform.position - part.position;
                _last = Quaternion.Inverse(turn * bind) * toCam;
                _last.y = 0f;
            }

            if (_last.sqrMagnitude > 1e-12f) _last.Normalize();
            else _last = Vector3.forward;

            blender.Blend(asset, _last, state);
            return _last;
        }
    }

    void SetupStateMachine()
    {
        _sm = new StateMachine();

        _idleState = new IdleState();
        _movementState = new MoveState(_movementSpeed);
        _climbState = new ZhmyhClimbState(transform, null, null, _climbSpeed);
        _aimingState = new AimState(_bow);
        _dashState = new DashState(transform, _dashCurve, _dashDistance, _dashDuration, this, _gravity);
        _jumpState = new JumpState(transform, _jumpCurve, _movementSpeed * 0.9f, _jumpHeight, _jumpDuration, this, _gravity);

        _movementHandler.Add(_idleState);
        _movementHandler.Add(_movementState);
        _movementHandler.Add(_dashState);
        _movementHandler.Add(_jumpState);
        _movementHandler.Add(_gravity);

        _sm.AddTransition(_idleState, _movementState, IdleToMovement);
        _sm.AddTransition(_movementState, _idleState, MovementToIdle);
        _sm.AddTransition(_idleState, _jumpState, ToJump);
        _sm.AddTransition(_movementState, _jumpState, ToJump);
        _sm.AddAnyTransition(_climbState, ToClimb);
        _sm.AddTransition(_jumpState, _idleState, JumpToIdle);
        _sm.AddTransition(_jumpState, _movementState, JumpToMovement);
        _sm.AddTransition(_jumpState, _dashState, JumpToDash);
        _sm.AddTransition(_climbState, _idleState, ClimbToIdle);
        _sm.AddTransition(_idleState, _aimingState, ToAim, Empty);
        _sm.AddTransition(_movementState, _aimingState, ToAim, _movementState.Move);
        _sm.AddTransition(_aimingState, _movementState, AimToMovement);
        _sm.AddTransition(_aimingState, _idleState, AimToIdle);
        _sm.AddTransition(_movementState, _dashState, MovementToDash);
        _sm.AddTransition(_dashState, _movementState, DashToMovement);
        _sm.AddTransition(_dashState, _idleState, DashToIdle);

        _sm.SetState(_idleState);

        bool IdleToMovement() => _movementDirection.sqrMagnitude > 0.01f;
        bool MovementToIdle() => !IdleToMovement();
        bool ToClimb() => _isClimbing;
        bool ClimbToIdle() => !ToClimb();
        bool ToAim() => _isAiming;
        bool AimToIdle() => !ToAim() && MovementToIdle();
        bool AimToMovement() => !ToAim() && IdleToMovement();
        bool ToDash() => IdleToMovement() && _isDashing;
        bool MovementToDash() => ToDash();
        bool DashTo() => _dashState.Progress == 1;
        bool DashToMovement() => DashTo() && IdleToMovement();
        bool DashToIdle() => DashTo() && !IdleToMovement();
        bool ToJump() => _isJumping;
        bool JumpTo() => _jumpState.Progress > 0.2f && _isGrounded;
        bool JumpToIdle() => JumpTo() && !IdleToMovement();
        bool JumpToMovement() => JumpTo() && IdleToMovement();
        bool JumpToDash() => IdleToMovement() && ToDash();
        void Empty() { }
    }

    class IdleState : State, IContributable<Vector3>
    {
        public Vector3 Contribute => Vector3.zero;
        public void SetDirection(Vector3 direction) { }
    }

    class MoveState : State, IContributable<Vector3>
    {
        public Vector3 Contribute => _result;
        readonly Mover _mover;
        Vector3 _result;

        public MoveState(float speed) => _mover = new Mover(speed);

        public override void OnExit() => _result = Vector3.zero;
        public override void OnTick() => Move();

        public void Move()
        {
            _mover.Tick(Time.deltaTime);
            _result = _mover.Contribute;
        }

        public void SetDirection(Vector3 direction) => _mover.SetDirection(direction);
    }

    class AimState : DecorateState
    {
        readonly Bow _bow;
        Vector3 _shootDirection;
        bool _isPull;

        public AimState(Bow bow) => _bow = bow;

        public override void OnTick()
        {
            if (_isPull && _bow != null) _bow.Pull();
        }

        public void SetShootDirection(Vector3 shootDirection) => _shootDirection = shootDirection;

        public void SetPull(bool isPull)
        {
            if (_isPull && !isPull && _bow != null) _bow.Release(_shootDirection);
            _isPull = isPull;
        }
    }

    class DashState : State, IContributable<Vector3>
    {
        public float Progress => Mathf.Clamp01(_progress);
        public Vector3 Contribute => _result;
        public Vector3 Direction => _currentDirection;

        readonly Dasher _dasher;
        readonly Transform _transform;
        readonly MonoBehaviour _ctx;
        readonly Gravity _gravity;
        readonly Vector3 _startLocalScale;

        Vector3 _direction;
        Vector3 _currentDirection;
        Vector3 _startPosition;
        Vector3 _result;
        float _progress;

        public DashState(Transform transform, AnimationCurve dash, float distance, float duration, MonoBehaviour ctx, Gravity gravity)
        {
            _transform = transform;
            _ctx = ctx;
            _gravity = gravity;
            _startLocalScale = transform.localScale;
            _dasher = new Dasher(dash, distance, duration);
        }

        public override void OnEnter()
        {
            _gravity.Zero();
            _currentDirection = _direction.sqrMagnitude > 0.0001f ? _direction.normalized : _transform.forward;
            _startPosition = _transform.position;
            _dasher.PerfomDash(_startPosition, _currentDirection);
            _ctx.StartCoroutine(Pop(0.2f));
        }

        public override void OnTick()
        {
            _dasher.Tick(Time.deltaTime);
            _progress = _dasher.Current;
            _result = _dasher.Contribute;
        }

        public override void OnExit()
        {
            _result = Vector3.zero;
            _ctx.StartCoroutine(Pup());
        }

        public void SetDirection(Vector3 direction) => _direction = direction;

        IEnumerator Pop(float offset)
        {
            var start = _startLocalScale;
            for (var i = 0f; i < 1f; i += Time.deltaTime * 4)
            {
                _transform.localScale = new Vector3(start.x, Mathf.Lerp(start.y, start.y - offset, i), start.z);
                yield return null;
            }
            _transform.localScale = new Vector3(start.x, start.y - offset, start.z);
        }

        IEnumerator Pup()
        {
            var start = _transform.localScale;
            var delta = _startLocalScale.y - start.y;
            for (var i = 0f; i < 1f; i += Time.deltaTime * 6)
            {
                _transform.localScale = new Vector3(start.x, Mathf.Lerp(start.y, start.y + delta, i), start.z);
                yield return null;
            }
            _transform.localScale = _startLocalScale;
        }
    }

    class JumpState : State, IContributable<Vector3>
    {
        public float Progress => _progress;
        public Vector3 Contribute => _result;

        readonly Transform _transform;
        readonly Jumper _jumper;
        readonly float _speedInAir;
        readonly MonoBehaviour _ctx;
        readonly Vector3 _scale;
        readonly Gravity _gravity;

        Vector3 _direction;
        Vector3 _result;
        float _progress;

        public JumpState(Transform transform, AnimationCurve curve, float speedInAir, float height, float duration, MonoBehaviour ctx, Gravity gravity)
        {
            _transform = transform;
            _jumper = new Jumper(curve, height, duration);
            _speedInAir = speedInAir;
            _ctx = ctx;
            _scale = transform.localScale;
            _gravity = gravity;
        }

        public override void OnEnter()
        {
            _gravity.SetVelocity(4);
            _jumper.PerfomJump();
            _transform.localScale = _scale;
            _ctx.StartCoroutine(PopupRoutine(0.1f));
        }

        public override void OnExit()
        {
            _result = Vector3.zero;
            _ctx.StartCoroutine(PopupRoutine(0.2f));
        }

        public override void OnTick()
        {
            _jumper.Tick(Time.deltaTime);
            _progress = _jumper.Current;
            _result = _jumper.Contribute + _direction * _speedInAir * Time.deltaTime;
        }

        public void SetDirection(Vector3 direction) => _direction = direction;

        IEnumerator PopupRoutine(float offset)
        {
            var start = _transform.localScale;
            var current = start;

            for (var i = 0f; i < 1f; i += Time.deltaTime * 8)
            {
                _transform.localScale = new Vector3(start.x, Mathf.Lerp(current.y, current.y - offset, i), start.z);
                yield return null;
            }

            current = _transform.localScale;
            for (var i = 0f; i < 1f; i += Time.deltaTime * 8)
            {
                _transform.localScale = new Vector3(start.x, Mathf.Lerp(current.y, current.y + offset, i), start.z);
                yield return null;
            }

            _transform.localScale = _scale;
        }
    }
}
