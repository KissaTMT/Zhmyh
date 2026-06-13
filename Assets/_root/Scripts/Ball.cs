using UnityEngine;

public class Ball:MonoBehaviour
{
    [SerializeField] private Vector3 _impulseForce = new Vector3(-0.1f, 0, 0);

    private Impulse _impulse;
    private Rigidbody _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _impulse = new Impulse();

        _impulse.Apply(new Vector3(4, 0, 0), 5);

        _rb.linearVelocity = Vector3.right * 10;
    }
    private void Update()
    {
        //_impulse.Tick(Time.deltaTime);
        //_rb.MovePosition(_rb.position + _impulse.Value * Time.deltaTime);
    }
}
