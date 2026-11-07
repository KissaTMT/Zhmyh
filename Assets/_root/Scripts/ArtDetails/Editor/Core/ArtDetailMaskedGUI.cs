using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>
    /// Поле Sprite в материале ArtDetails/MaskedUnlit.
    /// Прямоугольник спрайта пишется в текстуру и Tiling/Offset, шейдер читает их через TRANSFORM_TEX.
    /// </summary>
    public class ArtDetailMaskedGUI : ShaderGUI
    {
        public override void OnGUI(MaterialEditor editor, MaterialProperty[] properties)
        {
            var mainTex = FindProperty("_MainTex", properties);
            DrawSprite(editor, mainTex);
            editor.PropertiesDefaultGUI(properties);
        }

        static void DrawSprite(MaterialEditor editor, MaterialProperty mainTex)
        {
            var materials = editor.targets;
            Sprite current = null;
            bool mixed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                var sprite = Match((Material)materials[i]);
                if (i == 0) current = sprite;
                else if (sprite != current) mixed = true;
            }

            EditorGUI.showMixedValue = mixed;
            EditorGUI.BeginChangeCheck();
            var picked = (Sprite)EditorGUILayout.ObjectField("Sprite", current, typeof(Sprite), false);
            EditorGUI.showMixedValue = false;
            if (!EditorGUI.EndChangeCheck()) return;

            if (picked == null)
            {
                mainTex.textureScaleAndOffset = new Vector4(1f, 1f, 0f, 0f);
                return;
            }

            if (!TryRect(picked, out Texture2D tex, out Rect r))
            {
                Debug.LogWarning("ArtDetails: спрайт плотно упакован в атлас, прямоугольник недоступен. Нужен Full Rect без Sprite Atlas.");
                return;
            }
            mainTex.textureValue = tex;
            mainTex.textureScaleAndOffset = new Vector4(
                r.width / tex.width,
                r.height / tex.height,
                r.x / tex.width,
                r.y / tex.height);
        }

        static bool TryRect(Sprite sprite, out Texture2D tex, out Rect rect)
        {
            tex = null;
            rect = default;
            if (sprite == null) return false;
            if (sprite.packed && sprite.packingMode == SpritePackingMode.Tight) return false;
            tex = sprite.texture;
            if (tex == null || tex.width == 0 || tex.height == 0) return false;
            rect = sprite.textureRect;
            return true;
        }

        /// <summary>Спрайт, чей прямоугольник совпадает с текущими Tiling/Offset материала.</summary>
        static Sprite Match(Material mat)
        {
            var tex = mat.GetTexture("_MainTex") as Texture2D;
            if (tex == null || tex.width == 0 || tex.height == 0) return null;

            string path = AssetDatabase.GetAssetPath(tex);
            if (string.IsNullOrEmpty(path)) return null;

            Vector2 scale = mat.GetTextureScale("_MainTex");
            Vector2 offset = mat.GetTextureOffset("_MainTex");
            Sprite found = null;
            foreach (var asset in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
            {
                if (asset is not Sprite sprite) continue;
                if (!TryRect(sprite, out Texture2D spriteTex, out Rect r) || spriteTex != tex) continue;
                if (!SameRect(r, tex, scale, offset)) continue;
                found = sprite;
                break;
            }
            return found;
        }

        static bool SameRect(Rect r, Texture2D tex, Vector2 scale, Vector2 offset)
        {
            const float pixel = 0.5f;
            return Mathf.Abs(scale.x * tex.width - r.width) < pixel
                && Mathf.Abs(scale.y * tex.height - r.height) < pixel
                && Mathf.Abs(offset.x * tex.width - r.x) < pixel
                && Mathf.Abs(offset.y * tex.height - r.y) < pixel;
        }
    }
}
