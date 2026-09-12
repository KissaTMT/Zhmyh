using System.Collections.Generic;
using UnityEngine;

namespace Spirit
{
    public class ViewUvCalculator
    {
        public static Vector2[] CalculateUvsByView(RendererData data)
        {
            var uvs = new List<Vector2>();

            var sprite = data.Sprite;


            Rect uvRect = CalculateSpriteRect(sprite);
            bool flipX = data.FlipX;
            bool flipY = data.FlipY;

            var gridX = data.GridSize.x;
            var gridY = data.GridSize.y;

            for (int row = 0; row <= gridY; row++)
            {
                for (int col = 0; col <= gridX; col++)
                {
                    float tX = col / gridX;
                    float tY = row / gridY;

                    float u = Mathf.Lerp(uvRect.xMin, uvRect.xMax, tX);
                    float v = Mathf.Lerp(uvRect.yMin, uvRect.yMax, tY);

                    if (flipX) u = uvRect.xMax + uvRect.xMin - u;
                    if (flipY) v = uvRect.yMax + uvRect.yMin - v;

                    uvs.Add(new Vector2(u, v));
                }
            }
            return uvs.ToArray();
        }
        public static Rect CalculateSpriteRect(Sprite sprite)
        {
            var tex = sprite.texture;
            var r = sprite.textureRect;
            return new Rect(
                r.x / tex.width,
                r.y / tex.height,
                r.width / tex.width,
                r.height / tex.height
            );
        }
    }
}
