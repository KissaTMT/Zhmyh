namespace ArtMesh.Editor
{
    /// <summary>Вкладка окна ArtMesh Editor. Только отрисовка GUI и вызов функциональных классов.</summary>
    public interface IArtMeshTab
    {
        string Title { get; }
        void OnGUI(ArtMeshEditorContext ctx);
    }
}
