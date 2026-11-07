using System.Collections.Generic;
using ArtMesh;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Кэш силуэта на кадр: несколько деталей на одном ArtMesh считают его один раз. В редакторе кэша нет
    /// (кадры редактора и изменения данных не совпадают), силуэт считается при каждом запросе.
    /// </summary>
    public static class SilhouetteCache
    {
        sealed class Entry
        {
            public int frame = -1;
            public int count;
            public readonly Vector2[] points = new Vector2[SilhouetteCalculator.MaxPoints];
        }

        static readonly SilhouetteCalculator calculator = new SilhouetteCalculator();
        static readonly Dictionary<int, Entry> entries = new Dictionary<int, Entry>();
        static readonly Entry scratch = new Entry();

        /// <summary>Возвращает число точек силуэта; массив points общий, его нужно использовать сразу.</summary>
        public static int Get(ArtMeshRenderer target, out Vector2[] points)
        {
            points = null;
            if (target == null) return 0;

            Entry e;
            if (Application.isPlaying)
            {
                int id = target.GetInstanceID();
                if (!entries.TryGetValue(id, out e))
                {
                    if (entries.Count > 256) entries.Clear();
                    e = new Entry();
                    entries[id] = e;
                }
                if (e.frame == Time.frameCount) { points = e.points; return e.count; }
                e.frame = Time.frameCount;
            }
            else e = scratch;

            e.count = calculator.Compute(target, e.points);
            points = e.points;
            return e.count;
        }
    }
}
