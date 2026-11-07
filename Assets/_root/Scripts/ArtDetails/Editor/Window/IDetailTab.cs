namespace ArtDetails.Editor
{
    /// <summary>Вкладка окна ArtDetails Editor. Только отрисовка GUI и вызов функциональных классов.</summary>
    public interface IDetailTab
    {
        string Title { get; }
        void OnGUI(ArtDetailEditorContext ctx);
    }
}
