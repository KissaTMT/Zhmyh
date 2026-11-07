using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Вкладка Proxies: создание GameObject-прокси для выбранного вида, помощники выбора точек, диагностика.</summary>
    public sealed class ProxiesTab : IArtMeshTab
    {
        int ring = 1, ray, row, col;

        public string Title => "Proxies";

        public void OnGUI(ArtMeshEditorContext ctx)
        {
            if (!ctx.IsEditing)
            {
                EditorGUILayout.HelpBox("Сначала выбери вид на вкладке Views и нажми «Редактировать».", MessageType.Info);
                return;
            }

            var view = ctx.EditedView;
            EditorGUILayout.LabelField("Редактируется вид: " + view.name, EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Прокси: по GameObject на каждый узел сетки или вершину. Выбирай их в Scene или Hierarchy и двигай " +
                "обычными инструментами W/E/R, со snapping и Undo. Z точки = её глубина. Корень прокси = трансформ вида: " +
                "Position это offset (Z = глубина вида), Scale это scale; поворот не поддерживается. " +
                "Закончил: «Удалить прокси», данные остаются в виде.", MessageType.None);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Прокси сетки")) ctx.CreateProxies(ProxyKind.Lattice);
                if (GUILayout.Button("Прокси вершин")) ctx.CreateProxies(ProxyKind.Vertices);
            }

            var s = ctx.Session;
            using (new EditorGUILayout.HorizontalScope())
            {
                using (new EditorGUI.DisabledScope(s == null))
                {
                    if (GUILayout.Button("Выбрать корень")) Selection.activeGameObject = s.root;
                    if (GUILayout.Button("Удалить прокси")) ctx.DestroyProxies();
                }
            }
            if (GUILayout.Button("Выровнять Scene View по детали"))
                SceneViewAligner.AlignToFront(ctx.Target.transform);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Мягкое влияние на соседние точки", EditorStyles.boldLabel);
            ctx.ProxyOptions.latticeBrush = EditorGUILayout.Slider("Узлы сетки (ячейки)", ctx.ProxyOptions.latticeBrush, 0f, 4f);
            ctx.ProxyOptions.vertexBrush = EditorGUILayout.Slider("Вершины (шаги по кольцам/лучам)", ctx.ProxyOptions.vertexBrush, 0f, 4f);
            if (ctx.ProxyOptions.latticeBrush > 0f || ctx.ProxyOptions.vertexBrush > 0f)
                EditorGUILayout.HelpBox(
                    "Пока радиус больше 0, перетаскивание точки двигает и соседние (они затухают по расстоянию в шагах топологии). " +
                    "Для точечной правки поставь 0.", MessageType.Warning);

            if (s == null || !s.IsAlive) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Выбор точек", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Все")) ProxySelectionTools.All(s);
                if (GUILayout.Button("Инвертировать")) ProxySelectionTools.Invert(s);
            }

            if (s.kind == ProxyKind.Lattice) LatticeSelectors(s);
            else VertexSelectors(s, ctx);

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Обновить позиции прокси")) ProxySynchronizer.Reposition(s);
                if (GUILayout.Button("Проверить совпадающие точки"))
                    ctx.Message = ProxyDiagnostics.SelectCoincident(s, Mathf.Max(1e-5f, ctx.Asset.latticeSize.y * 0.0005f));
            }
        }

        void LatticeSelectors(ProxySession s)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                row = EditorGUILayout.IntSlider("Строка", Mathf.Clamp(row, 0, s.rows - 1), 0, s.rows - 1);
                if (GUILayout.Button("Выбрать", GUILayout.Width(70))) ProxySelectionTools.LatticeRow(s, row);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                col = EditorGUILayout.IntSlider("Столбец", Mathf.Clamp(col, 0, s.cols - 1), 0, s.cols - 1);
                if (GUILayout.Button("Выбрать", GUILayout.Width(70))) ProxySelectionTools.LatticeColumn(s, col);
            }
            if (GUILayout.Button("Только контур сетки")) ProxySelectionTools.LatticeBorder(s);
        }

        void VertexSelectors(ProxySession s, ArtMeshEditorContext ctx)
        {
            int rays = ProxySelectionTools.Rays(s), rings = ProxySelectionTools.Rings(s);
            if (rings == 0) { EditorGUILayout.HelpBox("Топология меша не радиальная.", MessageType.Warning); return; }

            if (GUILayout.Button("Центральная вершина")) ProxySelectionTools.Center(s);
            using (new EditorGUILayout.HorizontalScope())
            {
                ring = EditorGUILayout.IntSlider("Кольцо", Mathf.Clamp(ring, 1, rings), 1, rings);
                if (GUILayout.Button("Выбрать", GUILayout.Width(70))) ProxySelectionTools.Ring(s, ring);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                ray = EditorGUILayout.IntSlider("Луч", Mathf.Clamp(ray, 0, rays - 1), 0, rays - 1);
                if (GUILayout.Button("Выбрать", GUILayout.Width(70))) ProxySelectionTools.Ray(s, ray);
            }
            if (GUILayout.Button("Внешнее кольцо (контур)")) ProxySelectionTools.OuterRim(s);
        }
    }
}
