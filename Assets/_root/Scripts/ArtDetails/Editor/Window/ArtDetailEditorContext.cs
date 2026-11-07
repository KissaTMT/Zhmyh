using ArtMesh;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Состояние редактирования, общее для окна и вкладок: цель, сессия редактирования вида, предпросмотр направления.
    /// Без отрисовки. ArtMesh-цель меняется только через её публичный слой ViewSource.
    /// </summary>
    public sealed class ArtDetailEditorContext
    {
        public ArtDetail Detail { get; private set; }
        public DetailEditSession Session { get; private set; }

        public bool ManualPreview { get; private set; }
        public float PreviewYaw { get; private set; }
        public float PreviewPitch { get; private set; }
        public string Message = "";

        IArtMeshViewSource sourceBeforePreview;
        bool previewApplied;

        public ArtMeshRenderer Target => Detail != null ? Detail.Target : null;
        public ArtDetailAsset Asset => Detail != null ? Detail.Asset : null;
        public bool IsEditing => Session != null;
        public int ViewIndex => Session != null ? Session.ViewIndex : -1;

        public void SetDetail(ArtDetail d)
        {
            Release();
            Detail = d;
            Message = "";
        }

        /// <summary>Снимает всё, что редактор навесил (сессию, предпросмотр).</summary>
        public void Release()
        {
            EndEdit();
            ManualPreview = false;
            ApplyPreview();
        }

        // ------------------------------------------------------------ предпросмотр направления

        public void SetPreview(bool on, float yaw, float pitch)
        {
            if (IsEditing) return;                 // во время редактирования вида направление задаёт он
            ManualPreview = on;
            PreviewYaw = yaw;
            PreviewPitch = pitch;
            ApplyPreview();
        }

        void ApplyPreview()
        {
            var t = Target;
            if (t == null) { previewApplied = false; return; }

            if (ManualPreview)
            {
                if (!previewApplied) { sourceBeforePreview = t.ViewSource; previewApplied = true; }
                t.ViewSource = new ManualViewSource(PreviewYaw, PreviewPitch);
            }
            else if (previewApplied)
            {
                t.ViewSource = sourceBeforePreview;
                sourceBeforePreview = null;
                previewApplied = false;
            }
            t.Refresh();
            if (Detail != null) Detail.Refresh();
            SceneView.RepaintAll();
        }

        // ------------------------------------------------------------ редактирование вида

        public void BeginEdit(int index)
        {
            EndEdit();
            ManualPreview = false;
            ApplyPreview();

            Session = DetailEditSession.Begin(Detail, index, out string error);
            if (Session == null) Message = error;
            else { Message = ""; DetailGizmos.AlwaysVisible = true; }
            SceneView.RepaintAll();
        }

        public void EndEdit()
        {
            if (Session != null) Session.End();
            Session = null;
            DetailGizmos.AlwaysVisible = false;
            SceneView.RepaintAll();
        }

        /// <summary>Каждый кадр: читает Transform детали и пишет в вид. true, если данные изменились.</summary>
        public bool Tick()
        {
            if (Session == null) return false;
            if (Detail == null || Asset == null || Session.ViewIndex >= Asset.views.Count)
            {
                EndEdit();
                return true;
            }
            return Session.Tick();
        }

        public void OnUndoRedo()
        {
            if (Session == null) return;
            if (Asset == null || Session.ViewIndex >= Asset.views.Count) { EndEdit(); return; }
            Session.ApplyViewToTransform();
        }
    }
}
