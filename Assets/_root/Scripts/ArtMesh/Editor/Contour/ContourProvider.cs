using System;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Кэширует контур спрайта (извлечение дорогое), в том числе неудачу. Единственная ответственность: выдать контур по спрайту.</summary>
    public sealed class ContourProvider
    {
        Contour cached;
        int stamp;
        int failStamp = int.MinValue;
        string failError;

        public void Invalidate() { cached = null; failStamp = int.MinValue; }

        public Contour Get(Sprite sprite, bool mirror, float alphaThreshold, bool force, out string error)
        {
            error = null;
            if (sprite == null) { error = "Задай спрайт-образец контура."; return null; }

            int s = sprite.GetInstanceID() * 2 + (mirror ? 1 : 0);
            if (!force)
            {
                if (cached != null && stamp == s) return cached;
                if (failStamp == s) { error = failError; return null; }
            }

            Contour c;
            try { c = ContourExtractor.Extract(sprite, alphaThreshold); }
            catch (Exception ex) { failStamp = s; failError = ex.Message; error = failError; return null; }
            if (c == null) { failStamp = s; failError = "В спрайте не найден контур (пустая альфа?)."; error = failError; return null; }

            if (mirror) c = c.Mirrored();
            cached = c; stamp = s; failStamp = int.MinValue;
            return c;
        }
    }
}
