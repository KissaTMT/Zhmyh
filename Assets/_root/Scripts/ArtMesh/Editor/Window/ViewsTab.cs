using System;
using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Вкладка Views: список видов ассета, параметры вида, предпросмотр направления и операции над видами.</summary>
    public sealed class ViewsTab : IArtMeshTab
    {
        public string Title => "Views";

        public void OnGUI(ArtMeshEditorContext ctx)
        {
            var asset = ctx.Asset;
            if (asset == null) { EditorGUILayout.HelpBox("Сначала создай ArtMeshAsset на вкладке Mesh.", MessageType.Info); return; }
            if (asset.mesh == null) { EditorGUILayout.HelpBox("Сначала запеки меш (вкладка Mesh → Bake).", MessageType.Info); return; }

            EditorGUILayout.HelpBox(
                "Вид = направление камеры (yaw 0–360, pitch) + scale + offset (offset.z = глубина) + смещения сетки и вершин. " +
                "Каждый вид настраивается вручную; виды смешиваются по направлению камеры. Нужен вид Base с yaw 0 и без смещений. " +
                "Противоположную сторону можно создать кнопкой «Зеркало»: это обычный вид, который затем правится вручную.",
                MessageType.None);

            PreviewGUI(ctx);

            Action pending = null;   // операции над списком выполняем после отрисовки, чтобы не менять список в цикле

            using (var so = new SerializedObject(asset))
            {
                so.Update();
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
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("scale"));
                        EditorGUILayout.PropertyField(el.FindPropertyRelative("offset"));
                        if (EditorGUI.EndChangeCheck())
                        {
                            so.ApplyModifiedProperties();
                            ctx.Target.Refresh();
                            ctx.RepositionProxies();
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
                                pending = () => ViewOperations.SetDirection(asset, index, ctx.Target.Yaw, ctx.Target.Pitch);
                            if (GUILayout.Button("Зеркало"))
                                pending = () => MirrorView(ctx, index);
                            if (GUILayout.Button("Сброс"))
                                pending = () => { ctx.DestroyProxies(); ViewOperations.Reset(asset, index); ctx.Target.Refresh(); };
                            if (GUILayout.Button("X", GUILayout.Width(26)))
                                pending = () => { ctx.EndEdit(); ViewOperations.Remove(asset, index); ctx.Target.Refresh(); };
                        }
                    }
                }
                so.ApplyModifiedProperties();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("+ Base"))
                    pending = () => { ViewOperations.AddBase(asset); ctx.Target.Refresh(); };
                if (GUILayout.Button("+ Вид из текущего"))
                    pending = () =>
                    {
                        ctx.EndEdit();
                        var r = ctx.Target;
                        ViewOperations.AddFromState(asset, r.State, r.Yaw, r.Pitch);
                        ctx.BeginEdit(asset.views.Count - 1);
                    };
                using (new EditorGUI.DisabledScope(!ctx.IsEditing))
                {
                    if (GUILayout.Button("+ Зеркало выбранного"))
                        pending = () => MirrorView(ctx, ctx.ViewIndex);
                }
            }
            if (!ctx.IsEditing)
                EditorGUILayout.LabelField("«+ Зеркало выбранного» работает для вида в режиме «Редактировать»; у каждого вида есть и своя кнопка «Зеркало».",
                    EditorStyles.miniLabel);

            if (pending != null)
            {
                pending();
                GUIUtility.ExitGUI();
            }
        }

        // Предпросмотр направления без камеры: источник вида подменяется через слой IArtMeshViewSource.
        static void PreviewGUI(ArtMeshEditorContext ctx)
        {
            using (new EditorGUILayout.VerticalScope("box"))
            {
                EditorGUI.BeginChangeCheck();
                bool on = EditorGUILayout.ToggleLeft("Предпросмотр направления без камеры", ctx.ManualPreview);
                using (new EditorGUI.DisabledScope(!on))
                {
                    float yaw = EditorGUILayout.Slider("Yaw (0–360)", ctx.ManualYaw, 0f, 360f);
                    float pitch = EditorGUILayout.Slider("Pitch (−90…90)", ctx.ManualPitch, -90f, 90f);
                    if (EditorGUI.EndChangeCheck()) ctx.SetManualPreview(on, yaw, pitch);
                }
                if (on && ctx.IsEditing)
                    EditorGUILayout.LabelField("Пока редактируется вид, показывается он сам.", EditorStyles.miniLabel);
            }
        }

        static void MirrorView(ArtMeshEditorContext ctx, int index)
        {
            var nv = ViewMirrorService.CreateMirrored(ctx.Asset, index, out string error);
            if (nv == null) { ctx.Message = error; return; }
            ctx.Message = $"Создан вид «{nv.name}» (yaw {nv.yaw:0.#}°). Это обычный вид, его можно править вручную.";
            ctx.EndEdit();
            ctx.BeginEdit(ctx.Asset.views.Count - 1);
        }
    }
}
