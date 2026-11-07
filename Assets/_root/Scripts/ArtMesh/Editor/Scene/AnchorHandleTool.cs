using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Хэндл якоря радиальной сетки. Двигает authoring.anchor; меш пересобирается кнопкой Bake.</summary>
    public static class AnchorHandleTool
    {
        public static void Draw(ArtMeshRenderer r, ArtMeshAuthoring au)
        {
            if (r == null || au == null || au.sourceSprite == null) return;

            var sp = au.sourceSprite;
            Rect rc = sp.textureRect;
            Vector2 local = (new Vector2(au.anchor.x * rc.width, au.anchor.y * rc.height) - sp.pivot) / sp.pixelsPerUnit;

            var prev = Handles.matrix;
            Handles.matrix = r.transform.localToWorldMatrix;
            Vector3 lp = new Vector3(local.x, local.y, 0f);
            float size = .05f * HandleUtility.GetHandleSize(r.transform.TransformPoint(lp));

            Handles.color = new Color(.2f, .6f, 1f);
            EditorGUI.BeginChangeCheck();
            Vector3 np = Handles.FreeMoveHandle(lp, size, Vector3.zero, Handles.DotHandleCap);
            Handles.Label(lp, "  якорь меша (затем Bake)");
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(au, "Move ArtMesh anchor");
                Vector2 p = new Vector2(np.x, np.y) * sp.pixelsPerUnit + sp.pivot;
                au.anchor = new Vector2(p.x / rc.width, p.y / rc.height);
                au.autoAnchor = false;
                EditorUtility.SetDirty(au);
            }
            Handles.matrix = prev;
        }
    }
}
