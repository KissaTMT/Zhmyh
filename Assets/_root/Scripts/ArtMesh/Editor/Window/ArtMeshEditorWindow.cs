using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Главное окно ArtMesh. Единственная ответственность: хостить вкладки, следить за выбором цели
    /// и каждый кадр отдавать контексту тик синхронизации. Логика живёт в функциональных классах и контексте.
    /// </summary>
    public sealed class ArtMeshEditorWindow : EditorWindow
    {
        [SerializeField] ArtMeshRenderer targetRenderer;
        [SerializeField] int tabIndex;
        [SerializeField] bool lockTarget;

        ArtMeshEditorContext ctx;
        IArtMeshTab[] tabs;
        string[] titles;
        Vector2 scroll;

        [MenuItem("Tools/ArtMesh/Editor")]
        public static void Open()
        {
            var w = GetWindow<ArtMeshEditorWindow>("ArtMesh");
            w.minSize = new Vector2(380f, 420f);
            w.Show();
        }

        public static void OpenFor(ArtMeshRenderer r)
        {
            Open();
            var w = GetWindow<ArtMeshEditorWindow>();
            w.ChangeTarget(r);
        }

        void OnEnable()
        {
            ctx = new ArtMeshEditorContext();
            tabs = new IArtMeshTab[] { new MeshTab(), new ViewsTab(), new ProxiesTab(), new ContourTab() };
            titles = new string[tabs.Length];
            for (int i = 0; i < tabs.Length; i++) titles[i] = tabs[i].Title;

            if (targetRenderer == null) targetRenderer = SelectedRenderer();
            ctx.SetTarget(targetRenderer);

            SceneView.duringSceneGui += OnSceneGUI;
            EditorApplication.update += OnUpdate;
            Undo.undoRedoPerformed += OnUndoRedo;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            EditorApplication.update -= OnUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Selection.selectionChanged -= OnSelectionChanged;
            if (ctx != null) ctx.EndEdit();
        }

        static ArtMeshRenderer SelectedRenderer()
        {
            var go = Selection.activeGameObject;
            return go != null ? go.GetComponent<ArtMeshRenderer>() : null;
        }

        void ChangeTarget(ArtMeshRenderer r)
        {
            targetRenderer = r;
            ctx.SetTarget(r);
            Repaint();
        }

        void OnSelectionChanged()
        {
            if (lockTarget) return;
            var r = SelectedRenderer();
            if (r != null && r != ctx.Target) ChangeTarget(r);
        }

        void OnUpdate()
        {
            if (ctx != null && ctx.Tick()) Repaint();
        }

        void OnUndoRedo()
        {
            if (ctx == null) return;
            ctx.OnUndoRedo();
            Repaint();
        }

        void OnSceneGUI(SceneView sv)
        {
            if (ctx == null || ctx.Target == null) return;
            if (ctx.IsEditing)
            {
                // контур виден при редактировании вида в любом режиме: и сетка, и вершины, и без прокси
                ProxyOverlay.DrawContour(ctx.Target, ctx.EditedView, ctx.OverlayContour());
                ProxyOverlay.DrawLattice(ctx.Session);
            }
            else AnchorHandleTool.Draw(ctx.Target, ctx.Authoring(false));
        }

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var t = (ArtMeshRenderer)EditorGUILayout.ObjectField(targetRenderer, typeof(ArtMeshRenderer), true);
                lockTarget = GUILayout.Toggle(lockTarget, "Lock", EditorStyles.miniButton, GUILayout.Width(44));
                if (t != targetRenderer) ChangeTarget(t);
            }

            if (ctx.Target == null)
            {
                EditorGUILayout.HelpBox(
                    "Выбери объект с ArtMeshRenderer (или создай: GameObject → ArtMesh → Art Mesh Part).", MessageType.Info);
                return;
            }

            tabIndex = GUILayout.Toolbar(Mathf.Clamp(tabIndex, 0, tabs.Length - 1), titles);

            scroll = EditorGUILayout.BeginScrollView(scroll);
            tabs[tabIndex].OnGUI(ctx);
            EditorGUILayout.EndScrollView();

            if (!string.IsNullOrEmpty(ctx.Message))
                EditorGUILayout.HelpBox(ctx.Message, MessageType.Info);
        }
    }
}
