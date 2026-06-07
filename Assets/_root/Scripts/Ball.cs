using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Ball:MonoBehaviour
{
    [SerializeField] private Vector3 _impulse = new Vector3(-0.1f, 0, 0);

    private Rigidbody _rb;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.AddForce(_impulse);
    }
}
