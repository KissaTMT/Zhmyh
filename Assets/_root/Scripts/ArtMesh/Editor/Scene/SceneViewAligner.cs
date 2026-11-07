using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Ставит Scene-камеру напротив детали (на её оси +Z), чтобы инструменты двигали точки в плоскости детали.</summary>
    public static class SceneViewAligner
    {
        public static void AlignToFront(Transform part)
        {
            var sv = SceneView.lastActiveSceneView;
            if (sv == null || part == null) return;
            sv.LookAt(part.position, Quaternion.LookRotation(-part.forward, part.up), sv.size);
            sv.Repaint();
        }
    }
}
