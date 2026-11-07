using System.Collections.Generic;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Какие Transform сейчас являются прокси-точками и какому индексу соответствуют (нужно рисовщику гизмо,
    /// чтобы не вешать компоненты-маркеры). Одновременно существует одна сессия.
    /// </summary>
    public static class ProxyRegistry
    {
        static readonly Dictionary<int, int> indexById = new Dictionary<int, int>();
        static ProxySession current;

        public static void Register(ProxySession s)
        {
            indexById.Clear();
            current = s;
            for (int i = 0; i < s.points.Length; i++)
                if (s.points[i] != null) indexById[s.points[i].GetInstanceID()] = i;
        }

        public static void Unregister(ProxySession s)
        {
            if (current != s) return;
            indexById.Clear();
            current = null;
        }

        public static bool TryGet(Transform t, out ProxySession session, out int index)
        {
            session = current;
            index = -1;
            return current != null && indexById.Count > 0 && indexById.TryGetValue(t.GetInstanceID(), out index);
        }

        public static bool Contains(Transform t) => indexById.Count > 0 && indexById.ContainsKey(t.GetInstanceID());
    }
}
