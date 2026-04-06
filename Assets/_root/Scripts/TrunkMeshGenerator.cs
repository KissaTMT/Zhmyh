using System.Collections.Generic;
using UnityEngine;

public class TrunkMeshGenerator : MonoBehaviour
{
    [SerializeField] private Sprite _trunk;
    [SerializeField] private Transform[] _trunks;
    private MeshRenderer _renderer;
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;

    public void Generate()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _renderer = GetComponent<MeshRenderer>();
        _meshCollider = GetComponent<MeshCollider>();

        var mesh = GenerateMesh();

        var combines = new List<CombineInstance>();

        foreach(var trunk in _trunks)
        {
            var instance = new CombineInstance()
            {
                mesh = mesh,
                transform = transform.worldToLocalMatrix * trunk.localToWorldMatrix
            };
            combines.Add(instance);

            Destroy(trunk.gameObject);
        }

        var combinedMesh = new Mesh() { name = "trunk" };

        //combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        combinedMesh.CombineMeshes(combines.ToArray());

        _meshFilter.sharedMesh = combinedMesh;
        _meshCollider.sharedMesh = combinedMesh;

        var pb = new MaterialPropertyBlock();
        pb.SetTexture("_MainTex", _trunk.texture);

        _renderer.SetPropertyBlock(pb);
    }
    private Mesh GenerateMesh()
    {
        var mesh = new Mesh();

        var vertices = new Vector3[_trunk.vertices.Length];
        var uv = new Vector2[_trunk.uv.Length];
        var triangles = new int[_trunk.triangles.Length];

        for (var i = 0; i < vertices.Length; i++)
        {
            vertices[i] = _trunk.vertices[i];
        }

        for (var i = 0; i < triangles.Length; i++)
        {
            triangles[i] = _trunk.triangles[i];
        }
        for (int i = 0; i < uv.Length; i++)
        {
            uv[i] = _trunk.uv[i];
        }


        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        return mesh;
    }
}