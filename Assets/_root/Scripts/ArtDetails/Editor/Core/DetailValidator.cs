using System.Collections.Generic;
using ArtMesh;
using UnityEngine;

namespace ArtDetails.Editor
{
    public struct DetailIssue
    {
        public string text;
        public bool isError;
        public DetailIssue(string text, bool isError) { this.text = text; this.isError = isError; }
    }

    /// <summary>Проверяет, что деталь настроена правильно, и объясняет, чего не хватает. Только чтение.</summary>
    public static class DetailValidator
    {
        const string MaskShader = "ArtDetails/MaskedUnlit";

        public static List<DetailIssue> Validate(ArtDetail d)
        {
            var list = new List<DetailIssue>();
            if (d == null) return list;

            var target = d.Target;
            if (target == null)
            {
                list.Add(new DetailIssue("Не задана цель: ArtMeshRenderer, на котором стоит деталь.", true));
                return list;
            }

            var meshAsset = target.Asset;
            if (meshAsset == null || meshAsset.mesh == null)
                list.Add(new DetailIssue("У цели нет запечённого меша (ArtMesh → Mesh → Bake).", true));
            else if (meshAsset.Geometry == null || !meshAsset.Geometry.IsValid)
                list.Add(new DetailIssue("Меш цели нечитаем: включи Read/Write у меша (нужно для привязки и маски).", true));

            if (!d.IsChildOfTarget)
                list.Add(new DetailIssue("Деталь должна быть ПРЯМЫМ дочерним объектом цели: иначе локальный Transform не совпадает с осями ArtMesh.", false));

            if (d.Asset == null)
                list.Add(new DetailIssue("Нет ArtDetailAsset: некуда сохранять виды.", false));

            var b = d.Binding;
            if (!b.isBound)
                list.Add(new DetailIssue("Деталь не привязана к месту на меше. Поставь её на нужное место и нажми «Привязать».", false));
            else if (meshAsset != null && meshAsset.Geometry != null && b.vertexCount != meshAsset.Geometry.VertexCount)
                list.Add(new DetailIssue("Привязка устарела (меш перепечён). Нажми «Перепривязать».", true));

            if (d.Mask.enabled)
            {
                if (meshAsset != null && meshAsset.mesh != null && !meshAsset.topology.IsRadialFor(meshAsset.VertexCount))
                    list.Add(new DetailIssue("Меш цели не радиальный: силуэт для маски получить нельзя, маска отключена.", false));

                var renderers = d.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0)
                    list.Add(new DetailIssue("У детали нет рендереров (добавь меш, например Quad).", false));
                foreach (var r in renderers)
                {
                    var m = r.sharedMaterial;
                    if (m == null || m.shader == null || m.shader.name != MaskShader)
                    {
                        list.Add(new DetailIssue($"Рендерер «{r.name}»: для маски нужен шейдер {MaskShader}.", false));
                        break;
                    }
                }
            }
            return list;
        }
    }
}
