using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Передаёт маску в материалы деталей: многоугольник силуэта, матрицу «мир → оси ArtMesh-детали», отступ.
    /// Единственная ответственность: запись в MaterialPropertyBlock рендереров. Шейдер: ArtDetails/MaskedUnlit.
    /// </summary>
    public sealed class ArtDetailMaskBinder
    {
        static readonly int idOn = Shader.PropertyToID("_MaskOn"),
            idCount = Shader.PropertyToID("_MaskCount"),
            idInset = Shader.PropertyToID("_MaskInset"),
            idMatrix = Shader.PropertyToID("_PartWorldToLocal"),
            idPoly = Shader.PropertyToID("_MaskPoly");

        readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        readonly Vector4[] packed = new Vector4[SilhouetteCalculator.MaxPoints / 2];

        public void Apply(Renderer[] renderers, Vector2[] points, int count, Transform part, ArtDetailMaskSettings settings)
        {
            if (renderers == null || renderers.Length == 0) return;

            bool on = settings != null && settings.enabled && points != null && count >= 3 && part != null;
            if (on)
            {
                // две точки в одном Vector4: чётная = xy, нечётная = zw
                for (int i = 0; i < count; i++)
                {
                    Vector4 v = packed[i >> 1];
                    if ((i & 1) == 0) { v.x = points[i].x; v.y = points[i].y; }
                    else { v.z = points[i].x; v.w = points[i].y; }
                    packed[i >> 1] = v;
                }
            }

            foreach (var r in renderers)
            {
                if (r == null) continue;
                r.GetPropertyBlock(block);
                block.SetFloat(idOn, on ? 1f : 0f);
                if (on)
                {
                    block.SetFloat(idCount, count);
                    block.SetFloat(idInset, settings.inset);
                    block.SetMatrix(idMatrix, part.worldToLocalMatrix);
                    block.SetVectorArray(idPoly, packed);
                }
                r.SetPropertyBlock(block);
            }
        }
    }
}
