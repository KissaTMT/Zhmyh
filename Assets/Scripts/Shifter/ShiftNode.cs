using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ShiftNode
{
    private SpriteRenderer _renderer;
    private Transform _transform;

    public ShiftNode(SpriteRenderer renderer, Transform transform)
    {
        _renderer = renderer;
        _transform = transform;
    }
    public void ApplyConfig(ShiftConfig config)
    {
        
    }
}
