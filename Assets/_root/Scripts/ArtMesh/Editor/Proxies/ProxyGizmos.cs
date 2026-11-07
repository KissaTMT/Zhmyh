using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Рисует кликабельные кубики на прокси-точках. Используется DrawGizmo по Transform с проверкой в реестре,
    /// поэтому на самих объектах нет никаких компонентов (Pickable делает их выбираемыми в Scene View).
    /// Белый = обычная точка, оранжевый = выбрана, красный = заблокирована.
    /// </summary>
    public static class ProxyGizmos
    {
        static readonly Color normal = new Color(1f, 1f, 1f, .9f);
        static readonly Color selected = new Color(1f, .45f, .1f);
        static readonly Color locked = new Color(1f, .15f, .15f);

        [DrawGizmo(GizmoType.Pickable | GizmoType.NonSelected | GizmoType.Selected, typeof(Transform))]
        static void Draw(Transform t, GizmoType type)
        {
            if (!ProxyRegistry.TryGet(t, out ProxySession s, out int index)) return;

            bool sel = Selection.Contains(t.gameObject);
            bool lck = s.lockedVertices.Contains(index);
            float size = HandleUtility.GetHandleSize(t.position) * 0.025f * (sel ? 1.6f : 1f);
            Vector3 pos = t.position;
            if (Camera.current != null) pos -= Camera.current.transform.forward * size * 2f;   // чтобы меш не закрывал точку

            Gizmos.color = sel ? selected : (lck ? locked : normal);
            Gizmos.DrawCube(pos, Vector3.one * size);
            if (sel && lck)
            {
                Gizmos.color = locked;
                Gizmos.DrawWireCube(pos, Vector3.one * size * 1.5f);
            }
        }
    }
}
