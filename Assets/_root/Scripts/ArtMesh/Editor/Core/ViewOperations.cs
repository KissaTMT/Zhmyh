using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Операции над списком видов ассета. Каждая записывается в Undo.</summary>
    public static class ViewOperations
    {
        static void Begin(ArtMeshAsset a, string label) { Undo.RecordObject(a, label); }
        static void End(ArtMeshAsset a) { a.EnsureViewData(); EditorUtility.SetDirty(a); }

        /// <summary>Вид без смещений на yaw 0. Нужен как «нулевая» точка смешивания.</summary>
        public static ArtMeshView AddBase(ArtMeshAsset a)
        {
            Begin(a, "Add Base view");
            var v = new ArtMeshView { name = "Base" };
            a.views.Add(v);
            End(a);
            return v;
        }

        /// <summary>Новый вид из текущего состояния (копия смешанных смещений) в заданном направлении камеры.</summary>
        public static ArtMeshView AddFromState(ArtMeshAsset a, ArtMeshState s, float yaw, float pitch)
        {
            Begin(a, "Add view from current");
            var v = new ArtMeshView
            {
                name = "view " + a.views.Count,
                yaw = Mathf.Repeat(yaw, 360f),
                pitch = Mathf.Clamp(pitch, -90f, 90f),
                scale = s.scale,
                offset = s.offset,
                latticeDelta = (Vector3[])s.lattice.Clone(),
                vertexDelta = s.hasVertexDelta ? (Vector3[])s.vertexDelta.Clone() : null
            };
            a.views.Add(v);
            End(a);
            return v;
        }

        public static void Reset(ArtMeshAsset a, int index)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Reset view");
            var v = a.views[index];
            v.latticeDelta = null; v.vertexDelta = null;
            v.scale = Vector3.one; v.offset = Vector3.zero;
            End(a);
        }

        public static void Remove(ArtMeshAsset a, int index)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Remove view");
            a.views.RemoveAt(index);
            End(a);
        }

        public static void SetDirection(ArtMeshAsset a, int index, float yaw, float pitch)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Set view direction");
            a.views[index].yaw = Mathf.Repeat(yaw, 360f);
            a.views[index].pitch = Mathf.Clamp(pitch, -90f, 90f);
            End(a);
        }
    }
}
