using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Смешивает виды в итоговое состояние. Веса по угловому расстоянию на полной сфере видов (Shepard, 1/d^power).
    /// Сторон «лево/право» нет: каждый вид имеет собственное направление 0…360.
    /// </summary>
    public sealed class ArtMeshViewBlender
    {
        float[] weights = new float[0];

        /// <summary>Состояние ровно одного вида (превью, редактор).</summary>
        public void FromView(ArtMeshView view, ArtMeshState state)
        {
            state.Clear();
            state.scale = Vector3.zero;
            Accumulate(view, state, 1f);
        }

        /// <summary>Смесь видов для направления камеры l (локальные оси детали).</summary>
        public void Blend(ArtMeshAsset asset, Vector3 l, ArtMeshState state)
        {
            state.Clear();
            var views = asset.views;
            if (views.Count == 0) return;                      // видов нет: чистая базовая форма

            Vector3 dir = l.normalized;
            if (weights.Length != views.Count) weights = new float[views.Count];

            int exact = -1; float sum = 0f;
            for (int i = 0; i < views.Count; i++)
            {
                float ang = Mathf.Acos(Mathf.Clamp(Vector3.Dot(dir, views[i].Direction()), -1f, 1f));
                if (ang < 1e-3f) { exact = i; break; }
                weights[i] = 1f / Mathf.Pow(ang, asset.weightPower);
                sum += weights[i];
            }
            if (exact >= 0)
            {
                for (int i = 0; i < views.Count; i++) weights[i] = 0f;
                weights[exact] = 1f; sum = 1f;
            }

            state.scale = Vector3.zero;
            for (int i = 0; i < views.Count; i++)
            {
                float w = weights[i] / sum;
                if (w > 0f) Accumulate(views[i], state, w);
            }
        }

        static void Accumulate(ArtMeshView v, ArtMeshState s, float w)
        {
            s.scale += w * v.scale;
            s.offset += w * v.offset;
            if (v.latticeDelta != null && v.latticeDelta.Length == s.lattice.Length)
                for (int j = 0; j < s.lattice.Length; j++) s.lattice[j] += w * v.latticeDelta[j];
            if (v.vertexDelta != null && v.vertexDelta.Length == s.vertexDelta.Length)
            {
                s.hasVertexDelta = true;
                for (int j = 0; j < s.vertexDelta.Length; j++) s.vertexDelta[j] += w * v.vertexDelta[j];
            }
        }
    }
}
