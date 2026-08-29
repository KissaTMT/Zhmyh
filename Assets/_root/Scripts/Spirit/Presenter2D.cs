using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spirit
{
    public class Presenter2D : MonoBehaviour
    {
        [SerializeField] private List<SpiritConfig> _configs;
        private Dictionary<string, Transform> _bones;
        private SkinnedMeshRenderer _smr;
        private Camera _camera;
        private void Awake()
        {
            _smr = GetComponent<SkinnedMeshRenderer>();
            _camera = Camera.main;

            var bones = transform.GetComponentsInChildren<Transform>().Skip(1).ToArray();

            _bones = new Dictionary<string, Transform>();   

            for (var i=0; i<bones.Length; i++)
            {
                _bones[bones[i].name] = bones[i];
            }
        }

        private void Update()
        {
            var config = _configs[(int)(Mathf.Abs(_camera.transform.position.x) % 5)];
            _smr.sharedMesh.uv = config.Uvs;
            for(var i = 0; i < _bones.Count; i++)
            {
                var name = config.Names[i];
                var indx = config.Names.IndexOf(name);

                _bones[name].localPosition = config.Positions[indx];
                _bones[name].localRotation = config.Rotations[indx];
                _bones[name].localScale = config.Scales[indx];
            }
            _smr.sharedMesh.UploadMeshData(false);
        }

    }
}
