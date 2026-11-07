using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>
    /// Данные авторинга арт-меша (только редактор): спрайты, якорь и параметры построения. Лежит рядом с ArtMeshAsset
    /// отдельным файлом «Имя.authoring.asset» и не связан с рантаймом: ни ссылок на спрайты, ни текстур в билде.
    /// </summary>
    public sealed class ArtMeshAuthoring : ScriptableObject
    {
        public ArtMeshAsset target;
        public ArtMeshBuildSettings build = new ArtMeshBuildSettings();
        public Sprite sourceSprite;                           // по нему строится меш
        public Sprite contourSprite;                          // образец контура для подгонки вершин
        public bool autoAnchor = true;
        public Vector2 anchor = new Vector2(.5f, .5f);        // якорь радиальной сетки, 0..1 в прямоугольнике спрайта
    }
}
