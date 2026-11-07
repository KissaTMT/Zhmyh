using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Состояние редактирования, общее для окна и вкладок: цель, данные авторинга, редактируемый вид, источник вида,
    /// сессия прокси, прокрутка кольца, настройки. Без отрисовки: окно и вкладки только читают и вызывают его методы.
    /// Рантайм-компонентом управляет только через публичный слой ViewSource (IArtMeshViewSource).
    /// </summary>
    public sealed class ArtMeshEditorContext
    {
        public ArtMeshRenderer Target { get; private set; }
        public ProxySession Session { get; private set; }
        public int ViewIndex { get; private set; } = -1;
        public RimSpinSession Spin { get; private set; }
        public float SpinValue { get; private set; }

        // предпросмотр направления без камеры
        public bool ManualPreview { get; private set; }
        public float ManualYaw { get; private set; }
        public float ManualPitch { get; private set; }

        public readonly ProxyOptions ProxyOptions = new ProxyOptions();
        public readonly ContourFitOptions ContourOptions = new ContourFitOptions();
        public readonly ContourProvider Contours = new ContourProvider();
        public string Message = "";

        ArtMeshAuthoring authoring;

        public ArtMeshAsset Asset => Target != null ? Target.Asset : null;
        public bool IsEditing => Asset != null && ViewIndex >= 0 && ViewIndex < Asset.views.Count;
        public ArtMeshView EditedView => IsEditing ? Asset.views[ViewIndex] : null;
        public bool HasVertexSession => Session != null && Session.IsAlive && Session.kind == ProxyKind.Vertices;

        /// <summary>Данные авторинга текущего ассета (спрайты, параметры построения). create = создать файл, если его нет.</summary>
        public ArtMeshAuthoring Authoring(bool create)
        {
            var a = Asset;
            if (a == null) return null;
            if (authoring != null && authoring.target == a) return authoring;
            authoring = create ? ArtMeshAuthoringRepository.GetOrCreate(a) : ArtMeshAuthoringRepository.Find(a);
            return authoring;
        }

        public void SetTarget(ArtMeshRenderer r)
        {
            Release();
            Target = r;
            authoring = null;
            Message = "";
            Contours.Invalidate();
            if (r != null) ProxyFactory.CleanupOrphans(r);
        }

        /// <summary>Снимает всё, что редактор навесил на цель (прокси, источник вида, каркас).</summary>
        public void Release()
        {
            EndEdit();
            ManualPreview = false;
            ApplyViewSource();
        }

        // ------------------------------------------------------------ источник вида

        /// <summary>Единственное место, где редактор влияет на то, что показывает компонент.</summary>
        void ApplyViewSource()
        {
            if (Target == null) return;
            if (IsEditing) Target.ViewSource = new FixedViewSource(EditedView);
            else if (ManualPreview) Target.ViewSource = new ManualViewSource(ManualYaw, ManualPitch);
            else Target.ViewSource = null;
            Target.Refresh();
            SceneView.RepaintAll();
        }

        public void SetManualPreview(bool on, float yaw, float pitch)
        {
            ManualPreview = on;
            ManualYaw = yaw;
            ManualPitch = pitch;
            ApplyViewSource();
        }

        // ------------------------------------------------------------ редактирование вида

        /// <summary>Показывает ровно один вид (превью) и включает каркас.</summary>
        public void BeginEdit(int index)
        {
            if (Target == null || Asset == null || index < 0 || index >= Asset.views.Count) return;
            DestroyProxies();
            ViewIndex = index;
            ApplyViewSource();
            WireframeGizmo.Enabled = true;
        }

        public void EndEdit()
        {
            DestroyProxies();
            ViewIndex = -1;
            ApplyViewSource();
            WireframeGizmo.Enabled = false;
        }

        public void CreateProxies(ProxyKind kind)
        {
            if (!IsEditing) return;
            DestroyProxies();
            Session = ProxyFactory.Create(Target, ViewIndex, kind, ProxyOptions, out string error);
            if (Session == null) { Message = error; return; }
            Message = "";
            Selection.activeGameObject = Session.root;
            SceneViewAligner.AlignToFront(Target.transform);
            SceneView.RepaintAll();
        }

        public void DestroyProxies()
        {
            Spin = null;                      // прокрутка без сессии прокси не имеет смысла; данные остаются как есть
            ProxyFactory.Destroy(Session);
            Session = null;
        }

        /// <summary>Расставляет прокси по данным вида (после изменения параметров вида или Undo).</summary>
        public void RepositionProxies()
        {
            if (Session != null) ProxySynchronizer.Reposition(Session);
        }

        /// <summary>После Undo/Redo список видов мог поменять объекты: перепривязываем источник вида и прокси.</summary>
        public void OnUndoRedo()
        {
            if (Target == null) return;
            if (!IsEditing) { if (ViewIndex >= 0) EndEdit(); return; }
            ApplyViewSource();
            if (Session != null) ProxySynchronizer.Reposition(Session);
        }

        /// <summary>Вызывается каждый кадр: читает прокси и пишет в данные. true, если данные изменились.</summary>
        public bool Tick()
        {
            if (Session == null) return false;
            if (!Session.IsAlive) { DestroyProxies(); return true; }
            return ProxySynchronizer.Sync(Session);
        }

        // ------------------------------------------------------------ контур

        /// <summary>Контур для отрисовки в Scene View (из кэша). Виден при редактировании вида в любом режиме.</summary>
        public Contour OverlayContour()
        {
            if (!IsEditing || !ContourOptions.showContour) return null;
            var au = Authoring(false);
            if (au == null || au.contourSprite == null) return null;
            return Contours.Get(au.contourSprite, ContourOptions.mirrorContour, au.build.alphaThreshold, false, out _);
        }

        // ------------------------------------------------------------ блокировки

        /// <summary>Блокирует (или разблокирует) выбранные вершины внешнего кольца. Остальные выбранные игнорируются.</summary>
        public string LockSelected(bool lockIt)
        {
            if (!HasVertexSession) return "Нужны прокси вершин.";
            var s = Session;
            int rings = ProxySelectionTools.Rings(s);
            if (rings == 0) return "Топология меша не радиальная.";

            int n = ProxySelectionTools.Rays(s), first = 1 + (rings - 1) * n;
            int count = 0;
            foreach (int i in ProxySelectionTools.SelectedIndices(s))
            {
                if (i < first || i >= first + n) continue;
                if (lockIt) s.lockedVertices.Add(i); else s.lockedVertices.Remove(i);
                count++;
            }
            SceneView.RepaintAll();
            if (count == 0) return "Выбери вершины внешнего кольца.";
            return (lockIt ? "Заблокировано: " : "Разблокировано: ") + count + ". Всего заблокировано: " + s.lockedVertices.Count + ".";
        }

        public string ClearLocks()
        {
            if (Session == null) return "";
            Session.lockedVertices.Clear();
            SceneView.RepaintAll();
            return "Блокировки сняты.";
        }

        // ------------------------------------------------------------ прокрутка кольца по контуру

        public string BeginSpin()
        {
            if (!HasVertexSession) return "Нужны прокси вершин.";
            var au = Authoring(true);
            if (au == null) return "Нет данных авторинга.";
            var c = Contours.Get(au.contourSprite, ContourOptions.mirrorContour, au.build.alphaThreshold, true, out string error);
            if (c == null) return error;
            Spin = RimSpinSession.Begin(Session, c, ContourOptions, out error);
            SpinValue = 0f;
            return Spin == null ? error : "";
        }

        public void ApplySpin(float value)
        {
            if (Spin == null) return;
            SpinValue = value;
            Spin.Apply(value);
            SceneView.RepaintAll();
        }

        /// <summary>keep = true оставляет результат, false возвращает правки к моменту начала прокрутки.</summary>
        public void EndSpin(bool keep)
        {
            if (Spin == null) return;
            if (!keep) Spin.Cancel();
            Spin = null;
            SpinValue = 0f;
            SceneView.RepaintAll();
        }
    }
}
