using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Вкладка Contour: подгонка вершин по контуру спрайта-образца, блокировка участка кольца, прокрутка по контуру.</summary>
    public sealed class ContourTab : IArtMeshTab
    {
        int arcFrom, arcTo;

        public string Title => "Contour";

        public void OnGUI(ArtMeshEditorContext ctx)
        {
            if (!ctx.IsEditing)
            {
                EditorGUILayout.HelpBox("Сначала выбери вид на вкладке Views и нажми «Редактировать».", MessageType.Info);
                return;
            }

            var asset = ctx.Asset;
            var au = ctx.Authoring(true);
            if (au == null)
            {
                EditorGUILayout.HelpBox("ArtMeshAsset должен лежать в проекте (нужен файл авторинга рядом с ним).", MessageType.Warning);
                return;
            }

            EditorGUILayout.HelpBox(
                "Спрайт-образец нужен только как источник контура: берётся граница его альфа-маски (голубая линия в Scene, " +
                "видна и в режиме сетки, и в режиме вершин). Пивот спрайта должен совпадать с пивотом спрайта, по которому " +
                "построен меш. Сравнение идёт с формой после трансформа вида; глубина Z вершин не меняется.", MessageType.None);

            using (var so = new SerializedObject(au))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("contourSprite"));
                if (so.ApplyModifiedProperties())
                {
                    EditorUtility.SetDirty(au);
                    ctx.Contours.Invalidate();
                    SceneView.RepaintAll();
                }
            }

            var o = ctx.ContourOptions;
            EditorGUI.BeginChangeCheck();
            o.mirrorContour = EditorGUILayout.Toggle("Отразить контур по X", o.mirrorContour);
            o.snapPinned = EditorGUILayout.Toggle("Притягивать выбранные", o.snapPinned);
            o.redistribute = EditorGUILayout.Toggle("Расставить остальные по свободному контуру", o.redistribute);
            o.propagateInterior = EditorGUILayout.Toggle("Внутренние кольца следуют за внешним", o.propagateInterior);
            o.showContour = EditorGUILayout.Toggle("Показывать контур", o.showContour);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();

            if (!ctx.HasVertexSession || au.contourSprite == null)
            {
                EditorGUILayout.HelpBox(
                    au.contourSprite == null ? "Задай спрайт-образец контура." : "Для подгонки создай прокси вершин (вкладка Proxies).",
                    MessageType.Info);
                return;
            }

            var s = ctx.Session;
            bool spinning = ctx.Spin != null;

            using (new EditorGUI.DisabledScope(spinning))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Подгонка", EditorStyles.boldLabel);
                if (GUILayout.Button("Подогнать внешнее кольцо по контуру")) Run(ctx, c => ContourFitService.FitRim(s, c, o));
                if (GUILayout.Button("Привязать выбранные к контуру")) Run(ctx, c => ContourFitService.SnapSelected(s, c));
                if (GUILayout.Button("Выбрать вершины кольца вне контура"))
                    Run(ctx, c => ContourFitService.SelectOffContour(s, c, Mathf.Max(0.001f, asset.latticeSize.y * 0.01f)));
            }

            LocksGUI(ctx, s);
            SpinGUI(ctx, s, spinning);
        }

        // ---- блокировка участка кольца
        void LocksGUI(ArtMeshEditorContext ctx, ProxySession s)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Блокировка участка кольца (красные точки)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Заблокированные вершины стоят на месте при прокрутке. Остальные вершины распределяются по свободному " +
                "от них участку контура.", MessageType.None);

            int rays = ProxySelectionTools.Rays(s);
            using (new EditorGUILayout.HorizontalScope())
            {
                arcFrom = EditorGUILayout.IntField("Дуга: от луча", Mathf.Clamp(arcFrom, 0, rays - 1));
                arcTo = EditorGUILayout.IntField("до", Mathf.Clamp(arcTo, 0, rays - 1));
                if (GUILayout.Button("Выбрать дугу", GUILayout.Width(100))) ProxySelectionTools.RimArc(s, arcFrom, arcTo);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Заблокировать выбранные")) ctx.Message = ctx.LockSelected(true);
                if (GUILayout.Button("Разблокировать выбранные")) ctx.Message = ctx.LockSelected(false);
                if (GUILayout.Button("Снять все")) ctx.Message = ctx.ClearLocks();
            }
            EditorGUILayout.LabelField("Заблокировано вершин: " + s.lockedVertices.Count, EditorStyles.miniLabel);
        }

        // ---- прокрутка по контуру
        void SpinGUI(ArtMeshEditorContext ctx, ProxySession s, bool spinning)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Прокрутка вершин по контуру", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Без блокировок: кольцо целиком сдвигается вдоль контура, сохраняя текущее распределение. С блокировками: " +
                "заблокированные стоят, свободные вершины равномерно распределяются по свободному участку контура и " +
                "сдвигаются по нему (у заблокированных соседей прижимаются, не проходя сквозь них). Ползунок считается от " +
                "снимка, поэтому его можно крутить туда-обратно; «Применить» фиксирует результат, «Отмена» возвращает исходное.",
                MessageType.None);

            if (!spinning)
            {
                if (GUILayout.Button("Начать прокрутку")) ctx.Message = ctx.BeginSpin();
                return;
            }

            int half = Mathf.Max(1, ctx.Spin.RimCount / 2);
            EditorGUI.BeginChangeCheck();
            float v = EditorGUILayout.Slider("Сдвиг (слоты)", ctx.SpinValue, -half, half);
            if (EditorGUI.EndChangeCheck()) ctx.ApplySpin(v);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("−1 слот")) ctx.ApplySpin(Mathf.Round(ctx.SpinValue) - 1f);
                if (GUILayout.Button("+1 слот")) ctx.ApplySpin(Mathf.Round(ctx.SpinValue) + 1f);
                if (GUILayout.Button("0")) ctx.ApplySpin(0f);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                var prev = GUI.backgroundColor;
                GUI.backgroundColor = new Color(.6f, 1f, .6f);
                if (GUILayout.Button("Применить")) { ctx.EndSpin(true); ctx.Message = "Прокрутка применена."; }
                GUI.backgroundColor = prev;
                if (GUILayout.Button("Отмена")) { ctx.EndSpin(false); ctx.Message = "Прокрутка отменена."; }
            }
            EditorGUILayout.LabelField($"Заблокировано вершин: {ctx.Spin.LockedCount} из {ctx.Spin.RimCount} в кольце", EditorStyles.miniLabel);
        }

        static void Run(ArtMeshEditorContext ctx, System.Func<Contour, string> action)
        {
            var au = ctx.Authoring(true);
            if (au == null) { ctx.Message = "Нет данных авторинга."; return; }
            var c = ctx.Contours.Get(au.contourSprite, ctx.ContourOptions.mirrorContour, au.build.alphaThreshold, true, out string error);
            ctx.Message = c == null ? error : action(c);
            SceneView.RepaintAll();
        }
    }
}
