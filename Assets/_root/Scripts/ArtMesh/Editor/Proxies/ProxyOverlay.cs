using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Линии поверх Scene View: сетка деформации и контур спрайта-образца. Только отрисовка.</summary>
    public static class ProxyOverlay
    {
        public static void DrawLattice(ProxySession s)
        {
            if (s == null || !s.IsAlive || s.kind != ProxyKind.Lattice || s.cols * s.rows != s.points.Length) return;

            var prevZ = Handles.zTest;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = new Color(1f, .9f, .2f, .9f);
            for (int j = 0; j < s.rows; j++)
                for (int i = 0; i < s.cols; i++)
                {
                    var a = s.points[j * s.cols + i];
                    if (a == null) return;
                    if (i < s.cols - 1 && s.points[j * s.cols + i + 1] != null)
                        Handles.DrawLine(a.position, s.points[j * s.cols + i + 1].position);
                    if (j < s.rows - 1 && s.points[(j + 1) * s.cols + i] != null)
                        Handles.DrawLine(a.position, s.points[(j + 1) * s.cols + i].position);
                }
            Handles.zTest = prevZ;
        }

        /// <summary>Контур не зависит от сессии прокси: виден при редактировании вида в любом режиме (сетка или вершины).</summary>
        public static void DrawContour(ArtMeshRenderer r, ArtMeshView view, Contour c)
        {
            if (r == null || view == null || c == null) return;

            var prevM = Handles.matrix;
            var prevZ = Handles.zTest;
            Handles.matrix = r.transform.localToWorldMatrix;
            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            Handles.color = new Color(.2f, 1f, 1f, .95f);

            float z = view.offset.z + 0.01f;
            var pts = new Vector3[c.pts.Length + 1];
            for (int i = 0; i < c.pts.Length; i++) pts[i] = new Vector3(c.pts[i].x, c.pts[i].y, z);
            pts[pts.Length - 1] = pts[0];
            Handles.DrawAAPolyLine(2f, pts);

            Handles.zTest = prevZ;
            Handles.matrix = prevM;
        }
    }
}
