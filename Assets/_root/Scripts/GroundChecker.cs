using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GroundChecker
{
    private Transform _point;
    private LayerMask _layerMask;
    public GroundChecker(Transform point, LayerMask layerMask)
    {
        _point = point;
        _layerMask = layerMask;
    }

    public bool Check()
    {
        return Physics.SphereCast(_point.position, 1, -_point.up, out var hit,4,_layerMask.value);
    }
}