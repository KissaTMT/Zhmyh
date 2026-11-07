using UnityEngine;

namespace ArtMesh
{
    /// <summary>
    /// Направление «деталь → камера» в локальных осях детали. Углы: yaw 0…360 (0 = камера на оси +Z детали,
    /// 90 = на +X, 180 = сзади, 270 = на −X), pitch −90…90 (&gt; 0 = камера выше).
    /// </summary>
    public static class ArtMeshViewDirection
    {
        public static Vector3 FromCamera(Camera cam, Transform self, Transform space)
        {
            Vector3 toCam = cam.transform.position - self.position;
            Transform frame = space != null ? space : self;
            Vector3 l = frame.InverseTransformDirection(toCam);
            return l.sqrMagnitude > 1e-12f ? l.normalized : Vector3.forward;
        }

        public static Vector3 FromAngles(float yawDeg, float pitchDeg)
        {
            float y = yawDeg * Mathf.Deg2Rad, p = pitchDeg * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(y) * Mathf.Cos(p), Mathf.Sin(p), Mathf.Cos(y) * Mathf.Cos(p));
        }

        public static void ToAngles(Vector3 l, out float yaw, out float pitch)
        {
            yaw = Mathf.Repeat(Mathf.Atan2(l.x, l.z) * Mathf.Rad2Deg, 360f);
            pitch = Mathf.Asin(Mathf.Clamp(l.y, -1f, 1f)) * Mathf.Rad2Deg;
        }
    }
}
