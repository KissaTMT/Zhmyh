using UnityEditor;
using UnityEngine;

namespace ArtMesh.Editor
{
    /// <summary>Вкладка Mesh: ассет, материал, внешний вид, параметры построения (данные авторинга) и Bake.</summary>
    public sealed class MeshTab : IArtMeshTab
    {
        public string Title => "Mesh";

        public void OnGUI(ArtMeshEditorContext ctx)
        {
            var r = ctx.Target;

            using (var so = new SerializedObject(r))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("asset"));
                EditorGUILayout.PropertyField(so.FindProperty("material"));
                EditorGUILayout.PropertyField(so.FindProperty("appearance"), true);
                if (so.ApplyModifiedProperties()) { r.Rebind(); r.Refresh(); }
            }

            if (r.Material == null)
                EditorGUILayout.HelpBox("Назначь материал на шейдере ArtMesh/FlatDeform.", MessageType.Warning);

            var asset = r.Asset;
            if (asset == null)
            {
                EditorGUILayout.HelpBox("У детали нет ArtMeshAsset.", MessageType.Info);
                if (GUILayout.Button("Создать ArtMeshAsset…")) ArtMeshAssetCreator.CreateAndAssign(r);
                return;
            }

            var au = ctx.Authoring(true);
            if (au == null)
            {
                EditorGUILayout.HelpBox("ArtMeshAsset должен лежать в проекте (нужен файл авторинга рядом с ним).", MessageType.Warning);
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Построение меша (данные авторинга, в билд не попадают)", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Спрайт нужен только для построения меша (альфа-маска). Read/Write, Full Rect, без SpriteAtlas. " +
                "Якорь можно двигать в Scene View, затем нажать Bake. Данные лежат в файле «" +
                System.IO.Path.GetFileName(ArtMeshAuthoringRepository.PathFor(asset)) + "».", MessageType.None);

            using (var so = new SerializedObject(au))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("sourceSprite"));
                EditorGUILayout.PropertyField(so.FindProperty("autoAnchor"));
                using (new EditorGUI.DisabledScope(au.autoAnchor))
                    EditorGUILayout.PropertyField(so.FindProperty("anchor"));
                EditorGUILayout.PropertyField(so.FindProperty("build"), true);
                if (so.ApplyModifiedProperties()) EditorUtility.SetDirty(au);
            }

            EditorGUILayout.LabelField("Сетка деформации и смешивание", EditorStyles.boldLabel);
            using (var so = new SerializedObject(asset))
            {
                so.Update();
                EditorGUILayout.PropertyField(so.FindProperty("useLattice"));
                EditorGUILayout.PropertyField(so.FindProperty("latticeCols"));
                EditorGUILayout.PropertyField(so.FindProperty("latticeRows"));
                EditorGUILayout.PropertyField(so.FindProperty("weightPower"));
                if (so.ApplyModifiedProperties())
                {
                    asset.EnsureViewData();
                    EditorUtility.SetDirty(asset);
                    ctx.RepositionProxies();
                    r.Refresh();
                }
            }

            using (new EditorGUI.DisabledScope(au.sourceSprite == null))
            {
                if (GUILayout.Button("Bake mesh"))
                {
                    ctx.DestroyProxies();
                    if (ArtMeshBaker.Bake(asset, au, out string error))
                    {
                        r.Rebind();
                        r.Refresh();
                        ctx.Message = $"Меш запечён: вершин {asset.VertexCount}.";
                    }
                    else ctx.Message = error;
                }
            }

            EditorGUILayout.LabelField(
                asset.mesh != null
                    ? $"Меш: {asset.VertexCount} вершин, {asset.topology.rays} лучей × {asset.topology.rings} колец"
                    : "Меш не запечён",
                EditorStyles.miniLabel);
        }
    }
}
