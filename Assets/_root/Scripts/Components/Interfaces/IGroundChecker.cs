using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Components
{
    public interface IGroundChecker
    {
        bool Check(Vector3 point, float radius);
    }
}
