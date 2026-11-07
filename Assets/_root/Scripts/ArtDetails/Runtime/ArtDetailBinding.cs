using System;
using UnityEngine;

namespace ArtDetails
{
    /// <summary>
    /// Привязка детали к месту на меше ArtMesh: треугольник и барицентрические координаты. Точка привязки
    /// следует за деформацией меша. basePosition (позиция в базовой форме) позволяет перепривязать деталь
    /// после перепекания меша.
    /// </summary>
    [Serializable]
    public sealed class ArtDetailBinding
    {
        public bool isBound;
        public int triangle;                     // индекс треугольника (в списке треугольников сабмеша 0)
        public Vector3 barycentric = new Vector3(1f, 0f, 0f);
        public Vector2 basePosition;             // точка привязки в базовой форме меша
        public int vertexCount;                  // число вершин меша на момент привязки (проверка актуальности)
    }
}
