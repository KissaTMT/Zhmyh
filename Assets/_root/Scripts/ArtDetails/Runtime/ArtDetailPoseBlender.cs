using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Смешивает виды детали в позу по направлению. Режим задаётся в ассете (ArtDetailBlendMode):
    /// Local (по умолчанию) интерполирует только между соседними видами, Shepard смешивает все виды (1/d^power).
    /// В обоих режимах позиция и масштаб линейные, поворот по кватернионам.
    /// </summary>
    public sealed class ArtDetailPoseBlender
    {
        float[] weights = new float[0];

        public ArtDetailPose Blend(ArtDetailAsset asset, float yaw, float pitch)
        {
            if (asset == null || asset.views == null || asset.views.Count == 0) return ArtDetailPose.Identity;
            if (asset.blendMode == ArtDetailBlendMode.Local) return LocalPoseInterpolator.Evaluate(asset.views, yaw, pitch);
            return BlendShepard(asset, yaw, pitch);
        }

        // Все виды влияют на любой угол: вес = 1 / (угол до направления)^power. Поворот: взвешенное среднее кватернионов.
        ArtDetailPose BlendShepard(ArtDetailAsset asset, float yaw, float pitch)
        {
            var views = asset.views;
            Vector3 dir = ArtMeshViewDirection.FromAngles(yaw, pitch);
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

            Vector3 pos = Vector3.zero, scl = Vector3.zero;
            Vector4 q = Vector4.zero;
            Quaternion reference = Quaternion.identity;
            bool hasReference = false;

            for (int i = 0; i < views.Count; i++)
            {
                float w = weights[i] / sum;
                if (w <= 0f) continue;
                var v = views[i];
                pos += w * v.position;
                scl += w * v.scale;

                Quaternion r = Quaternion.Euler(v.euler);
                if (!hasReference) { reference = r; hasReference = true; }
                else if (Quaternion.Dot(reference, r) < 0f) r = new Quaternion(-r.x, -r.y, -r.z, -r.w);   // та же ориентация, другая полусфера
                q += w * new Vector4(r.x, r.y, r.z, r.w);
            }

            float len = Mathf.Sqrt(q.x * q.x + q.y * q.y + q.z * q.z + q.w * q.w);
            Quaternion rot = len > 1e-6f ? new Quaternion(q.x / len, q.y / len, q.z / len, q.w / len) : Quaternion.identity;
            return new ArtDetailPose(pos, rot, scl);
        }
    }
}
