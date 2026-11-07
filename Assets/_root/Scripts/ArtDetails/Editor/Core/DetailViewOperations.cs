using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Операции над списком видов детали. Каждая записывается в Undo.</summary>
    public static class DetailViewOperations
    {
        static void Begin(ArtDetailAsset a, string label) { a.EnsureData(); Undo.RecordObject(a, label); }
        static void End(ArtDetailAsset a) { EditorUtility.SetDirty(a); }

        /// <summary>Вид с нулевой позой на yaw 0 (нулевая точка смешивания).</summary>
        public static ArtDetailView AddIdentity(ArtDetailAsset a)
        {
            Begin(a, "Add detail view");
            var v = new ArtDetailView { name = "Base" };
            a.views.Add(v);
            End(a);
            return v;
        }

        /// <summary>Новый вид из текущей позы детали в заданном направлении.</summary>
        public static ArtDetailView AddFromCurrent(ArtDetailAsset a, ArtDetail d, float yaw, float pitch)
        {
            Begin(a, "Add detail view from current");
            var pose = d.ReadPose();
            var v = new ArtDetailView
            {
                name = "view " + a.views.Count,
                yaw = Mathf.Repeat(yaw, 360f),
                pitch = Mathf.Clamp(pitch, -90f, 90f)
            };
            v.Set(pose.position, d.transform.localEulerAngles, pose.scale);
            a.views.Add(v);
            End(a);
            return v;
        }

        /// <summary>Записывает текущую позу детали в существующий вид.</summary>
        public static void Capture(ArtDetailAsset a, int index, ArtDetail d)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Capture detail pose");
            var pose = d.ReadPose();
            a.views[index].Set(pose.position, d.transform.localEulerAngles, pose.scale);
            End(a);
        }

        public static void Reset(ArtDetailAsset a, int index)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Reset detail view");
            a.views[index].Set(Vector3.zero, Vector3.zero, Vector3.one);
            End(a);
        }

        public static void Remove(ArtDetailAsset a, int index)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Remove detail view");
            a.views.RemoveAt(index);
            End(a);
        }

        public static void SetDirection(ArtDetailAsset a, int index, float yaw, float pitch)
        {
            if (index < 0 || index >= a.views.Count) return;
            Begin(a, "Set detail view direction");
            a.views[index].yaw = Mathf.Repeat(yaw, 360f);
            a.views[index].pitch = Mathf.Clamp(pitch, -90f, 90f);
            End(a);
        }
    }
}
