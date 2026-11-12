namespace ArtDetails
{
    /// <summary>
    /// Как смешиваются виды детали по направлению камеры.
    /// Local: между двумя соседними видами интерполяция идёт ТОЛЬКО между ними (остальные виды не влияют).
    /// Shepard: все виды влияют на любой угол (1/d^power), старое поведение.
    /// </summary>
    public enum ArtDetailBlendMode
    {
        Local = 0,
        Shepard = 1
    }
}
