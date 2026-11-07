namespace ArtMesh.Editor
{
    /// <summary>
    /// Настройки мягкого влияния при перемещении прокси (состояние окна). Радиус измеряется в ШАГАХ ТОПОЛОГИИ,
    /// а не в мировых единицах: так результат не зависит от плотности вершин. 0 = выключено (двигаются только выбранные).
    /// </summary>
    public sealed class ProxyOptions
    {
        public float latticeBrush = 0f;    // в ячейках сетки
        public float vertexBrush = 0f;     // в шагах по кольцам и лучам меша
    }
}
