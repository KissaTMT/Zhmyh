using Components;
using System.Collections;
using UnityEngine;

public class Inverted : MonoBehaviour
{
    [SerializeField] private Vector3 _impulseForce = new Vector3(-0.1f, 0, 0); // сила, а не смещение
    [SerializeField] private float _atenuationSpeed = 1;
    [SerializeField] private bool _isPhysical = false;

    private Transform _transform;
    private Rigidbody _rigidbody;
    private Gravity _gravity;
    private IGroundChecker _groundChecker;

    private bool _isGrounded;

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _rigidbody = GetComponent<Rigidbody>();
        _gravity = new Gravity();
        _gravity.Zero();
        _groundChecker = new GroundSphereOverlapChecker(LayerMask.GetMask("Default", "Ground"), _transform);

        StartCoroutine(AtenuationImpulseRoutine());
    }

    private void FixedUpdate()
    {
        _isGrounded = CheckGround();

        if (_isGrounded) _gravity.Zero();

        Vector3 gravityForce = CalculateGravityForce();

        ApplyCustomForces(gravityForce, _impulseForce);
    }

    private Vector3 CalculateGravityForce()
    {
        Vector3 gravityDisplacement = _gravity.Apply();
        Vector3 gravityVelocity = gravityDisplacement / Time.deltaTime;
        return gravityVelocity;
    }

    private void ApplyCustomForces(Vector3 gravityForce, Vector3 additionalForce)
    {
        _rigidbody.linearVelocity = gravityForce + additionalForce;
    }

    private bool CheckGround()
    {
        return _groundChecker.Check(transform.position + Vector3.down * 0.5f, 0.05f);
    }

    private IEnumerator AtenuationImpulseRoutine()
    {
        var start = _impulseForce;
        for (float t = 0; t < 1; t += Time.deltaTime / _atenuationSpeed)
        {
            _impulseForce = Vector3.Lerp(start, Vector3.zero, t);
            yield return null;
        }
        _impulseForce = Vector3.zero;
    }
}


