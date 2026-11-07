using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Вкладка Setup: цель, ассет, маска, привязка к месту на меше и проверки.</summary>
    public sealed class DetailSetupTab : IDetailTab
    {
        public string Title => "Setup";

        public void OnGUI(ArtDetailEditorContext ctx)
        {
            var d = ctx.Detail;

            using (var so = new SerializedObject(d))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("target"), new GUIContent("Цель (ArtMeshRenderer)"));
                EditorGUILayout.PropertyField(so.FindProperty("asset"), new GUIContent("Ассет видов"));
                if (so.ApplyModifiedProperties()) d.RefreshRenderers();
            }

            if (d.Asset == null && GUILayout.Button("Создать ArtDetailAsset…"))
                DetailObjectCreator.CreateAndAssign(d);

            // ---- проверки
            var issues = DetailValidator.Validate(d);
            foreach (var issue in issues)
                EditorGUILayout.HelpBox(issue.text, issue.isError ? MessageType.Error : MessageType.Warning);
            if (d.Target != null && !d.IsChildOfTarget && GUILayout.Button("Сделать дочерним к цели"))
            {
                Undo.SetTransformParent(d.transform, d.Target.transform, "Parent detail to target");
                d.Refresh();
            }

            // ---- привязка
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Привязка к месту на меше", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Поставь деталь на нужное место (в Scene View, при нужном направлении) и нажми «Привязать». Точка привязки " +
                "лежит на поверхности меша и едет вместе с деформацией. Красная сфера = точка привязки, оранжевый треугольник = " +
                "привязанный треугольник.", MessageType.None);

            var b = d.Binding;
            EditorGUILayout.LabelField(
                b.isBound ? $"Привязана: треугольник {b.triangle}, барицентр ({b.barycentric.x:0.00}, {b.barycentric.y:0.00}, {b.barycentric.z:0.00})"
                          : "Не привязана", EditorStyles.miniLabel);

            using (new EditorGUI.DisabledScope(d.Target == null || ctx.IsEditing))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Привязать к позиции детали"))
                        ctx.Message = DetailBinder.BindAtDetailPosition(d, out string e1) ? "Привязано." : e1;
                    if (GUILayout.Button("Перепривязать (после Bake)"))
                        ctx.Message = DetailBinder.Rebind(d, out string e2) ? "Перепривязано." : e2;
                    if (GUILayout.Button("Сбросить"))
                    {
                        DetailBinder.Unbind(d);
                        ctx.Message = "Привязка сброшена.";
                    }
                }
            }
            if (ctx.IsEditing)
                EditorGUILayout.LabelField("Во время редактирования вида привязка недоступна.", EditorStyles.miniLabel);

            // ---- маска
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Маска силуэта", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Деталь не рисуется за границами силуэта меша-цели (внешнее кольцо, пересчитывается каждый кадр). Работает с " +
                "материалами на шейдере ArtDetails/MaskedUnlit. Отступ сдвигает границу внутрь.", MessageType.None);
            using (var so = new SerializedObject(d))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("mask"), true);
                if (so.ApplyModifiedProperties()) d.Refresh();
            }
            if (GUILayout.Button("Обновить рендереры детали")) d.RefreshRenderers();
        }
    }
}
