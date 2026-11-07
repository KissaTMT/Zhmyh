using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Главное окно ArtDetails. Единственная ответственность: хостить вкладки, следить за выбором детали и каждый
    /// кадр отдавать контексту тик синхронизации. Логика живёт в функциональных классах и контексте.
    /// </summary>
    public sealed class ArtDetailEditorWindow : EditorWindow
    {
        [SerializeField] ArtDetail targetDetail;
        [SerializeField] int tabIndex;
        [SerializeField] bool lockTarget;

        ArtDetailEditorContext ctx;
        IDetailTab[] tabs;
        string[] titles;
        Vector2 scroll;

        [MenuItem("Tools/ArtDetails/Editor")]
        public static void Open()
        {
            var w = GetWindow<ArtDetailEditorWindow>("ArtDetails");
            w.minSize = new Vector2(380f, 420f);
            w.Show();
        }

        public static void OpenFor(ArtDetail d)
        {
            Open();
            GetWindow<ArtDetailEditorWindow>().ChangeTarget(d);
        }

        void OnEnable()
        {
            ctx = new ArtDetailEditorContext();
            tabs = new IDetailTab[] { new DetailSetupTab(), new DetailViewsTab() };
            titles = new string[tabs.Length];
            for (int i = 0; i < tabs.Length; i++) titles[i] = tabs[i].Title;

            if (targetDetail == null) targetDetail = SelectedDetail();
            ctx.SetDetail(targetDetail);

            EditorApplication.update += OnUpdate;
            Undo.undoRedoPerformed += OnUndoRedo;
            Selection.selectionChanged += OnSelectionChanged;
        }

        void OnDisable()
        {
            EditorApplication.update -= OnUpdate;
            Undo.undoRedoPerformed -= OnUndoRedo;
            Selection.selectionChanged -= OnSelectionChanged;
            if (ctx != null) ctx.Release();
        }

        static ArtDetail SelectedDetail()
        {
            var go = Selection.activeGameObject;
            return go != null ? go.GetComponentInParent<ArtDetail>() : null;
        }

        void ChangeTarget(ArtDetail d)
        {
            targetDetail = d;
            ctx.SetDetail(d);
            Repaint();
        }

        void OnSelectionChanged()
        {
            if (lockTarget) return;
            var d = SelectedDetail();
            if (d != null && d != ctx.Detail) ChangeTarget(d);
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

        void OnGUI()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var d = (ArtDetail)EditorGUILayout.ObjectField(targetDetail, typeof(ArtDetail), true);
                lockTarget = GUILayout.Toggle(lockTarget, "Lock", EditorStyles.miniButton, GUILayout.Width(44));
                if (d != targetDetail) ChangeTarget(d);
            }

            if (ctx.Detail == null)
            {
                EditorGUILayout.HelpBox(
                    "Выбери объект с ArtDetail или создай: GameObject → ArtDetails → Art Detail (Quad) (выбрав ArtMesh-деталь).",
                    MessageType.Info);
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
