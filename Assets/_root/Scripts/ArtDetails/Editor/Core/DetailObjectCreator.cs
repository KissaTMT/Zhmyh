using ArtMesh;
using UnityEditor;
using UnityEngine;

namespace ArtDetails.Editor
{
    /// <summary>Создание объектов и ассетов: деталь-квад с маской, материал по умолчанию, ArtDetailAsset.</summary>
    public static class DetailObjectCreator
    {
        const string Folder = "Assets/ArtDetails";
        const string MaterialPath = Folder + "/ArtDetailDefault.mat";

        /// <summary>Материал на шейдере ArtDetails/MaskedUnlit; создаётся один раз в Assets/ArtDetails.</summary>
        public static Material EnsureDefaultMaterial()
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
            if (mat != null) return mat;

            var shader = Shader.Find("ArtDetails/MaskedUnlit");
            if (shader == null) { Debug.LogError("ArtDetails: шейдер ArtDetails/MaskedUnlit не найден."); return null; }
            if (!AssetDatabase.IsValidFolder(Folder)) AssetDatabase.CreateFolder("Assets", "ArtDetails");
            mat = new Material(shader) { name = "ArtDetailDefault", color = Color.black };
            AssetDatabase.CreateAsset(mat, MaterialPath);
            AssetDatabase.SaveAssets();
            return mat;
        }

        public static ArtDetailAsset CreateAndAssign(ArtDetail d)
        {
            string path = EditorUtility.SaveFilePanelInProject("Create ArtDetail Asset", d.name + "_Detail", "asset", "");
            if (string.IsNullOrEmpty(path)) return null;

            var a = ScriptableObject.CreateInstance<ArtDetailAsset>();
            AssetDatabase.CreateAsset(a, path);
            DetailViewOperations.AddIdentity(a);
            AssetDatabase.SaveAssets();

            Undo.RecordObject(d, "Assign ArtDetail asset");
            d.Asset = a;
            EditorUtility.SetDirty(d);
            return a;
        }

        [MenuItem("GameObject/ArtDetails/Art Detail (Quad)", false, 10)]
        static void CreateQuad(MenuCommand cmd)
        {
            var contextGo = cmd.context as GameObject != null ? (GameObject)cmd.context : Selection.activeGameObject;
            ArtMeshRenderer target = contextGo != null ? contextGo.GetComponentInParent<ArtMeshRenderer>() : null;

            var go = new GameObject("Art Detail");
            Undo.RegisterCreatedObjectUndo(go, "Create Art Detail");
            if (target != null) go.transform.SetParent(target.transform, false);
            else GameObjectUtility.SetParentAndAlign(go, contextGo);

            var detail = go.AddComponent<ArtDetail>();
            if (target != null) detail.Target = target;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Quad);
            visual.name = "Visual";
            var collider = visual.GetComponent<Collider>();
            if (collider != null) UnityEngine.Object.DestroyImmediate(collider);
            visual.transform.SetParent(go.transform, false);
            visual.transform.localScale = Vector3.one * 0.2f;
            var mat = EnsureDefaultMaterial();
            if (mat != null) visual.GetComponent<MeshRenderer>().sharedMaterial = mat;
            detail.RefreshRenderers();

            Selection.activeGameObject = go;
            ArtDetailEditorWindow.OpenFor(detail);
        }
    }
}
