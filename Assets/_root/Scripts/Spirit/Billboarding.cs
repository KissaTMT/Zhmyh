using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Spirit
{
    public class Billboarding
    {
        private Transform[] _bones;
        private Camera _cameraMain;
        private Transform _cameraTransformCashed;
        public Billboarding(Transform[] bones)
        {
            _bones = bones;
            _cameraMain = Camera.main;
            _cameraTransformCashed = _cameraMain.GetComponent<Transform>();
        }
        public void Billboard()
        {
            if (_bones == null || _bones.Length == 0) return;

            for (var i = 0; i < _bones.Length; i++)
            {
                var bone = _bones[i];

                bone.rotation = Quaternion.LookRotation(bone.position - _cameraTransformCashed.position, Vector3.up);
                //bone.rotation = _cameraTransformCashed.rotation;
            }
        }
    }
}
