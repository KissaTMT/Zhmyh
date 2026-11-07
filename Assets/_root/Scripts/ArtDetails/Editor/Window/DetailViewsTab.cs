using System;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Вкладка Views: виды детали (только Transform), предпросмотр направления и редактирование вида.</summary>
    public sealed class DetailViewsTab : IDetailTab
    {
        public string Title => "Views";

        public void OnGUI(ArtDetailEditorContext ctx)
        {
            var d = ctx.Detail;
            var asset = ctx.Asset;
            if (asset == null) { EditorGUILayout.HelpBox("Сначала создай ArtDetailAsset на вкладке Setup.", MessageType.Info); return; }
            if (ctx.Target == null) { EditorGUILayout.HelpBox("Не задана цель (вкладка Setup).", MessageType.Info); return; }

            EditorGUILayout.HelpBox(
                "Вид детали хранит только Transform (position, euler, scale) для направления yaw 0–360 / pitch. Позиция " +
                "относительно точки привязки, поэтому деталь едет вместе с деформацией меша. «Редактировать»: цель покажет " +
                "направление вида, а деталь можно двигать обычными инструментами Unity, изменения пишутся в вид сразу.",
                MessageType.None);

            PreviewGUI(ctx);

            Action pending = null;   // операции над списком выполняем после отрисовки

            using (var so = new SerializedObject(asset))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("weightPower"));
                var list = so.FindProperty("views");
                for (int i = 0; i < list.arraySize; i++)
                {
                    int index = i;
                    var el = list.GetArrayElementAtIndex(i);
                    bool editing = ctx.IsEditing && ctx.ViewIndex == i;

                    using (new EditorGUILayout.VerticalScope("box"))
                    {
                        EditorGUI.BeginChangeCheck();
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("name"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("yaw"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("pitch"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("position"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("euler"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("scale"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            so.ApplyModifiedProperties();
                            if (editing) ctx.Session.ApplyViewToTransform();
                            d.Refresh();
                            SceneView.RepaintAll();
                        }

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var prev = GUI.backgroundColor;
                            if (editing) GUI.backgroundColor = new Color(1f, .9f, .3f);
                            if (GUILayout.Button(editing ? "Стоп" : "Редактировать"))
                                pending = () => { if (editing) ctx.EndEdit(); else ctx.BeginEdit(index); };
                            GUI.backgroundColor = prev;

                            if (GUILayout.Button("Взять направление"))
                                pending = () => DetailViewOperations.SetDirection(asset, index, ctx.Target.Yaw, ctx.Target.Pitch);
                            using (new EditorGUI.DisabledScope(!d.HasAnchor || ctx.IsEditing))
                            {
                                if (GUILayout.Button("Записать трансформ"))
                                    pending = () => DetailViewOperations.Capture(asset, index, d);
                            }
                            if (GUILayout.Button("Сброс"))
                                pending = () => { ctx.EndEdit(); DetailViewOperations.Reset(asset, index); d.Refresh(); };
                            if (GUILayout.Button("X", GUILayout.Width(26)))
                                pending = () => { ctx.EndEdit(); DetailViewOperations.Remove(asset, index); d.Refresh(); };
                        }
                    }
                }
                so.ApplyModifiedProperties();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Base"))
                    pending = () => { DetailViewOperations.AddIdentity(asset); d.Refresh(); };
                using (new EditorGUI.DisabledScope(!d.HasAnchor || ctx.IsEditing))
                {
                    if (GUILayout.Button("+ Вид из текущего"))
                        pending = () =>
                        {
                            var t = ctx.Target;
                            DetailViewOperations.AddFromCurrent(asset, d, t.Yaw, t.Pitch);
                            ctx.BeginEdit(asset.views.Count - 1);
                        };
                }
            }
            if (!d.HasAnchor)
                EditorGUILayout.LabelField("Нет точки привязки: привяжи деталь на вкладке Setup.", EditorStyles.miniLabel);

            if (pending != null)
            {
                pending();
                GUIUtility.ExitGUI();
            }
        }

        // Предпросмотр направления без камеры: источник вида цели подменяется через слой IArtMeshViewSource.
        static void PreviewGUI(ArtDetailEditorContext ctx)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                using (new EditorGUI.DisabledScope(ctx.IsEditing))
                {
                    EditorGUI.BeginChangeCheck();
                    bool on = EditorGUILayout.ToggleLeft("Предпросмотр направления без камеры", ctx.ManualPreview);
                    using (new EditorGUI.DisabledScope(!on))
                    {
                        float yaw = EditorGUILayout.Slider("Yaw (0–360)", ctx.PreviewYaw, 0f, 360f);
                        float pitch = EditorGUILayout.Slider("Pitch (−90…90)", ctx.PreviewPitch, -90f, 90f);
                        if (EditorGUI.EndChangeCheck()) ctx.SetPreview(on, yaw, pitch);
                    }
                }
                if (ctx.IsEditing)
                    EditorGUILayout.LabelField("Пока редактируется вид, направление задаёт он.", EditorStyles.miniLabel);
            }
        }
    }
}
