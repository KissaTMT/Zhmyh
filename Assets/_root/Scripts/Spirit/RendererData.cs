using System;
using UnityEngine;

namespace Spirit
{
    [Serializable]
    public struct RendererData
    {
        public Sprite Sprite;
        public Vector2 GridSize;
        public bool FlipX;
        public bool FlipY;

        public override int GetHashCode()
        {
            return Sprite.GetHashCode() + GridSize.GetHashCode() + FlipX.GetHashCode() + FlipY.GetHashCode();
        }
    }
}
