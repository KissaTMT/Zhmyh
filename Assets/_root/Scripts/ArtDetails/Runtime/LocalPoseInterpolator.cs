using System.Collections.Generic;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Локальная интерполяция видов детали (как ключи параметров в Live2D). Чистая функция без выделения памяти.
    ///
    /// Виды группируются в слои по pitch (допуск PitchTolerance). Внутри слоя yaw замкнут в круг 0…360:
    /// поза берётся интерполяцией ровно между двумя СОСЕДНИМИ видами (предыдущим и следующим по кругу), остальные виды слоя
    /// не влияют. Если слоёв несколько, поза считается в двух соседних слоях и линейно смешивается по pitch.
    /// Поэтому между двумя видами с нулевым Scale.X деталь остаётся строго невидимой, а в самих видах поза точная.
    /// </summary>
    public static class LocalPoseInterpolator
    {
        const float PitchTolerance = 0.5f;

        public static ArtDetailPose Evaluate(List<ArtDetailView> views, float yaw, float pitch)
        {
            if (views == null || views.Count == 0) return ArtDetailPose.Identity;
            yaw = Mathf.Repeat(yaw, 360f);

            // ближайшие слои по pitch снизу и сверху от запроса
            float below = float.NegativeInfinity, above = float.PositiveInfinity;
            float minP = float.PositiveInfinity, maxP = float.NegativeInfinity;
            for (int i = 0; i < views.Count; i++)
            {
                float p = views[i].pitch;
                if (p < minP) minP = p;
                if (p > maxP) maxP = p;
                if (p <= pitch && p > below) below = p;
                if (p >= pitch && p < above) above = p;
            }
            if (float.IsNegativeInfinity(below)) below = minP;     // запрос ниже всех слоёв
            if (float.IsPositiveInfinity(above)) above = maxP;     // запрос выше всех слоёв

            if (Mathf.Abs(above - below) < PitchTolerance) return Layer(views, below, yaw);

            float t = Mathf.Clamp01((pitch - below) / (above - below));
            return ArtDetailPose.Lerp(Layer(views, below, yaw), Layer(views, above, yaw), t);
        }

        // Поза слоя одного pitch на заданном yaw: интерполяция между соседними видами по кругу.
        static ArtDetailPose Layer(List<ArtDetailView> views, float layerPitch, float yaw)
        {
            int prev = -1, next = -1, first = -1, last = -1;
            float prevYaw = float.NegativeInfinity, nextYaw = float.PositiveInfinity;
            float firstYaw = float.PositiveInfinity, lastYaw = float.NegativeInfinity;

            for (int i = 0; i < views.Count; i++)
            {
                var v = views[i];
                if (Mathf.Abs(v.pitch - layerPitch) >= PitchTolerance) continue;

                float y = Mathf.Repeat(v.yaw, 360f);
                if (y < firstYaw) { firstYaw = y; first = i; }
                if (y > lastYaw) { lastYaw = y; last = i; }
                if (y <= yaw && y > prevYaw) { prevYaw = y; prev = i; }
                if (y > yaw && y < nextYaw) { nextYaw = y; next = i; }
            }

            if (first < 0) return ArtDetailPose.Identity;
            if (lastYaw - firstYaw < 1e-4f) return views[first].ToPose();          // в слое один вид

            if (prev < 0) { prev = last; prevYaw = lastYaw - 360f; }               // левее первого вида: предыдущий = последний по кругу
            if (next < 0) { next = first; nextYaw = firstYaw + 360f; }             // правее последнего: следующий = первый по кругу

            float span = nextYaw - prevYaw;
            float k = span > 1e-5f ? Mathf.Clamp01((yaw - prevYaw) / span) : 0f;
            return ArtDetailPose.Lerp(views[prev].ToPose(), views[next].ToPose(), k);
        }
    }
}
