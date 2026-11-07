using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Создаёт зеркальную копию вида (быстрый способ получить противоположную сторону). Это инструмент редактора:
    /// в рантайме понятия зеркала нет, результат — обычный вид с собственными данными, которые можно править вручную.
    ///
    /// Новый вид: yaw' = 360 − yaw, тот же pitch, offset.x' = −offset.x, сетка отражена (столбцы развёрнуты, x инвертирован),
    /// а правки вершин подобраны так, чтобы итоговая форма была ТОЧНЫМ зеркальным образом исходной: вершина i занимает
    /// отражённую позицию своей ближайшей зеркальной соседки (перестановка без выворота меша).
    /// </summary>
    public static class ViewMirrorService
    {
        public static ArtMeshView CreateMirrored(ArtMeshAsset a, int index, out string error)
        {
            error = null;
            if (a == null || index < 0 || index >= a.views.Count) { error = "Не выбран вид."; return null; }

            var partner = MirrorPermutation.Build(a, out error);
            if (partner == null) return null;

            a.EnsureViewData();
            var g = a.Geometry;
            var src = a.views[index];
            int C = a.latticeCols, R = a.latticeRows, n = a.LatticeCount, vc = g.VertexCount;

            // 1) заготовка нового вида: отражённые трансформ и сетка
            var nv = new ArtMeshView
            {
                name = src.name + " (зеркало)",
                yaw = Mathf.Repeat(360f - src.yaw, 360f),
                pitch = src.pitch,
                scale = src.scale,
                offset = new Vector3(-src.offset.x, src.offset.y, src.offset.z),
                latticeDelta = new Vector3[n],
                vertexDelta = new Vector3[vc]
            };
            for (int j = 0; j < R; j++)
                for (int i = 0; i < C; i++)
                {
                    Vector3 d = src.latticeDelta[j * C + (C - 1 - i)];
                    nv.latticeDelta[j * C + i] = new Vector3(-d.x, d.y, d.z);
                }

            // 2) итоговая форма исходного вида и базовая форма нового (без правок вершин)
            var blender = new ArtMeshViewBlender();
            var srcState = new ArtMeshState(); srcState.Resize(n, vc); blender.FromView(src, srcState);
            var newState = new ArtMeshState(); newState.Resize(n, vc); blender.FromView(nv, newState);

            // 3) правки вершин: цель = отражённая позиция соседки в исходном виде
            for (int i = 0; i < vc; i++)
            {
                int j = partner[i];
                Vector3 pj = Vector3.Scale(ArtMeshEvaluator.ShapeFinal(a, g, srcState, j), src.scale) + src.offset;
                Vector3 target = new Vector3(-pj.x, pj.y, pj.z);

                Vector3 baseNew = ArtMeshEvaluator.ShapeBase(a, g, newState, i);
                Vector3 shape = baseNew;                              // защита от нулевого scale: правка = 0
                if (Mathf.Abs(nv.scale.x) > 1e-4f) shape.x = (target.x - nv.offset.x) / nv.scale.x;
                if (Mathf.Abs(nv.scale.y) > 1e-4f) shape.y = (target.y - nv.offset.y) / nv.scale.y;
                if (Mathf.Abs(nv.scale.z) > 1e-4f) shape.z = (target.z - nv.offset.z) / nv.scale.z;
                nv.vertexDelta[i] = shape - baseNew;
            }

            Undo.RecordObject(a, "Create mirrored view");
            a.views.Add(nv);
            EditorUtility.SetDirty(a);
            return nv;
        }
    }
}
