using ArtMesh;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Живое редактирование вида детали обычными инструментами Unity. На время сессии:
    ///   • цель показывает направление вида (источник вида ArtMesh подменяется через публичный слой ViewSource);
    ///   • деталь перестаёт управляться компонентом (Driving = false), её двигает пользователь;
    ///   • каждый тик поза детали записывается в вид (абсолютная синхронизация: Undo и любые инструменты работают сами).
    /// </summary>
    public sealed class DetailEditSession
    {
        public ArtDetail Detail { get; }
        public int ViewIndex { get; }

        readonly ArtMeshRenderer target;
        readonly IArtMeshViewSource previousSource;

        ArtDetailView View => Detail.Asset.views[ViewIndex];

        DetailEditSession(ArtDetail detail, int viewIndex)
        {
            Detail = detail;
            ViewIndex = viewIndex;
            target = detail.Target;
            previousSource = target.ViewSource;
        }

        public static DetailEditSession Begin(ArtDetail d, int index, out string error)
        {
            error = null;
            if (d == null || d.Target == null) { error = "У детали нет цели (ArtMeshRenderer)."; return null; }
            var asset = d.Asset;
            if (asset == null || index < 0 || index >= asset.views.Count) { error = "Не выбран вид."; return null; }
            if (!d.Binding.isBound) { error = "Сначала привяжи деталь к меш (вкладка Setup)."; return null; }

            var s = new DetailEditSession(d, index);
            var view = s.View;
            s.target.ViewSource = new ManualViewSource(view.yaw, view.pitch);
            s.target.Refresh();
            d.Driving = false;
            d.Refresh();

            if (!d.HasAnchor)
            {
                s.End();
                error = "Привязка недействительна (меш перепечён?). Нажми «Перепривязать».";
                return null;
            }
            s.ApplyViewToTransform();
            return s;
        }

        /// <summary>Переносит данные вида в Transform детали (старт сессии, после Undo/Redo).</summary>
        public void ApplyViewToTransform()
        {
            target.Refresh();
            Detail.Refresh();
            if (Detail.HasAnchor && ViewIndex < Detail.Asset.views.Count) Detail.ApplyPose(View.ToPose());
        }

        /// <summary>Читает Transform детали и записывает в вид. true, если вид изменился.</summary>
        public bool Tick()
        {
            if (Detail == null || Detail.Asset == null || ViewIndex >= Detail.Asset.views.Count) return false;
            target.Refresh();
            Detail.Refresh();                       // свежая точка привязки; поза не пишется, пока Driving = false
            if (!Detail.HasAnchor) return false;

            var pose = Detail.ReadPose();
            var v = View;
            bool changed = (pose.position - v.position).sqrMagnitude > 1e-10f
                        || (pose.scale - v.scale).sqrMagnitude > 1e-10f
                        || Quaternion.Angle(pose.rotation, Quaternion.Euler(v.euler)) > 1e-3f;
            if (!changed) return false;

            Undo.RecordObject(Detail.Asset, "Edit detail view");
            v.Set(pose.position, Detail.transform.localEulerAngles, pose.scale);
            EditorUtility.SetDirty(Detail.Asset);
            return true;
        }

        public void End()
        {
            if (Detail != null)
            {
                Detail.Driving = true;
                target.ViewSource = previousSource;
                target.Refresh();
                Detail.Refresh();
            }
        }
    }
}
