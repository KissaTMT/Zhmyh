namespace ArtMesh.Editor
{
    /// <summary>Настройки подгонки по контуру (состояние окна, не данные ассета).</summary>
    public sealed class ContourFitOptions
    {
        public bool mirrorContour;            // отразить контур по X
        public bool snapPinned = true;        // притягивать выбранные вершины к контуру
        public bool redistribute = true;      // остальные вершины кольца расставить по свободному контуру
        public bool propagateInterior = true; // внутренние кольца следуют за внешним
        public bool showContour = true;
    }
}
