using ModestTree;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Spirit
{
    public class Presenter2D : MonoBehaviour
    {
        [SerializeField] private List<SpiritConfig> _configs;
        [SerializeField] private List<Transform> _bonesList;
        private Dictionary<string, Transform> _bones;
        private SkinnedMeshRenderer _smr;
        private Camera _camera;
        private void Awake()
        {
            _smr = GetComponent<SkinnedMeshRenderer>();
            _camera = Camera.main;

            _bones = new Dictionary<string, Transform>();   

            for (var i=0; i< _bonesList.Count; i++)
            {
                _bones[_bonesList[i].name] = _bonesList[i];
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
