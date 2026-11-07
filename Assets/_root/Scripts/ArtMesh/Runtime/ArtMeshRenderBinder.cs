using System;
using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Отвечает только за передачу состояния в материал: материалы [заливка, обводка], MaterialPropertyBlock,
    /// массив сетки и данные правок вершин (1D-буфер RGBAFloat, индекс = SV_VertexID).
    /// </summary>
    public sealed class ArtMeshRenderBinder : IDisposable
    {
        static readonly int idTint = Shader.PropertyToID("_Tint"),
            idOutC = Shader.PropertyToID("_OutlineColor"), idOutW = Shader.PropertyToID("_OutlineWidth"),
            idIsOut = Shader.PropertyToID("_IsOutline"), idOutOn = Shader.PropertyToID("_OutlineOn"),
            idScale = Shader.PropertyToID("_Scale"), idOffset = Shader.PropertyToID("_Offset"),
            idUseLat = Shader.PropertyToID("_UseLattice"), idLatMin = Shader.PropertyToID("_LatMin"),
            idLatSize = Shader.PropertyToID("_LatSize"), idLatDim = Shader.PropertyToID("_LatDim"),
            idLat = Shader.PropertyToID("_Lattice"),
            idVertTex = Shader.PropertyToID("_VertTex"), idVertW = Shader.PropertyToID("_VertTexW"),
            idUseVert = Shader.PropertyToID("_UseVertEdit");

        readonly MaterialPropertyBlock mpb = new MaterialPropertyBlock();
        readonly Vector4[] latBuf = new Vector4[LatticeMath.MaxDim * LatticeMath.MaxDim];
        Vector4[] vertBuf;
        Material outlineMat;
        Texture2D vertexTex;

        /// <summary>Назначает материалы: [заливка, обводка], если в меше два сабмеша.</summary>
        public void BindMaterials(MeshRenderer r, Mesh mesh, Material fill)
        {
            if (r == null || mesh == null || fill == null) return;
            if (mesh.subMeshCount >= 2)
            {
                if (outlineMat == null || outlineMat.shader != fill.shader)
                {
                    DestroySafe(outlineMat);
                    outlineMat = new Material(fill) { name = "ArtMesh_Outline", hideFlags = HideFlags.HideAndDontSave };
                    outlineMat.SetFloat(idIsOut, 1f);
                }
                r.sharedMaterials = new[] { fill, outlineMat };
            }
            else r.sharedMaterials = new[] { fill };
        }

        public void Apply(MeshRenderer r, ArtMeshAsset asset, ArtMeshState s, ArtMeshAppearance look)
        {
            if (outlineMat != null) outlineMat.SetFloat(idOutOn, look.outline ? 1f : 0f);

            int n = Mathf.Min(asset.LatticeCount, latBuf.Length);
            for (int i = 0; i < n; i++)
                latBuf[i] = new Vector4(s.lattice[i].x, s.lattice[i].y, s.lattice[i].z, 0f);
            bool lat = asset.useLattice && asset.latticeSize.x > 1e-4f && asset.latticeSize.y > 1e-4f;

            r.GetPropertyBlock(mpb);
            mpb.SetColor(idTint, look.fillColor);
            mpb.SetColor(idOutC, look.outlineColor);
            mpb.SetFloat(idOutW, look.outlineWidth);
            mpb.SetVector(idScale, s.scale);
            mpb.SetVector(idOffset, s.offset);
            mpb.SetFloat(idUseLat, lat ? 1f : 0f);
            mpb.SetVector(idLatMin, asset.latticeMin);
            mpb.SetVector(idLatSize, asset.latticeSize);
            mpb.SetVector(idLatDim, new Vector4(asset.latticeCols, asset.latticeRows, 0f, 0f));
            mpb.SetVectorArray(idLat, latBuf);

            int vc = s.vertexDelta.Length;
            bool useVert = s.hasVertexDelta && vc > 0;
            if (useVert)
            {
                if (vertexTex == null || vertexTex.width != vc)
                {
                    DestroySafe(vertexTex);
                    vertexTex = new Texture2D(vc, 1, TextureFormat.RGBAFloat, false, true)
                    {
                        filterMode = FilterMode.Point,
                        wrapMode = TextureWrapMode.Clamp,
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    vertBuf = new Vector4[vc];
                }
                for (int i = 0; i < vc; i++)
                    vertBuf[i] = new Vector4(s.vertexDelta[i].x, s.vertexDelta[i].y, s.vertexDelta[i].z, 0f);
                vertexTex.SetPixelData(vertBuf, 0);
                vertexTex.Apply(false);
                mpb.SetTexture(idVertTex, vertexTex);
                mpb.SetFloat(idVertW, vc);
            }
            mpb.SetFloat(idUseVert, useVert ? 1f : 0f);
            r.SetPropertyBlock(mpb);
        }

        public void Dispose()
        {
            DestroySafe(outlineMat); outlineMat = null;
            DestroySafe(vertexTex); vertexTex = null;
        }

        static void DestroySafe(UnityEngine.Object o)
        {
            if (o == null) return;
            if (Application.isPlaying) UnityEngine.Object.Destroy(o); else UnityEngine.Object.DestroyImmediate(o);
        }
    }
}
