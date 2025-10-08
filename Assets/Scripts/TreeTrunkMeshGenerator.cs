using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter), typeof(MeshCollider))]
public class TreeTrunkMeshGenerator : MonoBehaviour
{
    [Header("Sprite Settings")]
    public Sprite trunkSprite;
    public float pixelsPerUnit = 100f;

    [Header("3D Settings")]
    public float depth = 1f;
    public int verticalSegments = 8;
    public bool generateCollider = true;

    [Header("Optimization")]
    public float simplificationTolerance = 0.1f;

    private Mesh generatedMesh;

    void Start()
    {
        GenerateMeshFromSprite();
    }

    [ContextMenu("Generate Mesh from Sprite")]
    public void GenerateMeshFromSprite()
    {
        if (trunkSprite == null)
        {
            Debug.LogError("No trunk sprite assigned!");
            return;
        }

        generatedMesh = CreateMeshFromSprite(trunkSprite);
        GetComponent<MeshFilter>().mesh = generatedMesh;

        if (generateCollider)
        {
            MeshCollider collider = GetComponent<MeshCollider>();
            collider.sharedMesh = generatedMesh;
            collider.convex = false;
        }

        Debug.Log($"Mesh generated: {generatedMesh.vertices.Length} vertices, {generatedMesh.triangles.Length / 3} triangles");
    }

    private Mesh CreateMeshFromSprite(Sprite sprite)
    {
        Mesh mesh = new Mesh();
        mesh.name = $"{sprite.name}_TrunkMesh";

        // Получаем контуры спрайта
        Vector2[] spriteVertices = sprite.vertices;
        ushort[] spriteTriangles = sprite.triangles;

        // Если у спрайта нет данных о триангуляции, создаем простой прямоугольник
        if (spriteVertices == null || spriteVertices.Length == 0)
        {
            Debug.LogWarning("Sprite doesn't have mesh data. Using fallback rectangle.");
            return CreateFallbackMesh(sprite);
        }

        // Создаем 3D меш путем экструзии 2D формы
        List<Vector3> vertices3D = new List<Vector3>();
        List<int> triangles3D = new List<int>();
        List<Vector2> uv3D = new List<Vector2>();

        // Создаем слои по высоте
        for (int layer = 0; layer < verticalSegments; layer++)
        {
            float layerHeight = (float)layer / (verticalSegments - 1);
            float z = (layerHeight - 0.5f) * depth;

            // Добавляем вершины для этого слоя
            for (int i = 0; i < spriteVertices.Length; i++)
            {
                Vector2 spriteVert = spriteVertices[i];
                Vector3 vertex3D = new Vector3(
                    spriteVert.x,
                    spriteVert.y,
                    z
                );

                vertices3D.Add(vertex3D);

                // UV координаты
                Vector2 uv = new Vector2(
                    (spriteVert.x - sprite.bounds.min.x) / sprite.bounds.size.x,
                    (spriteVert.y - sprite.bounds.min.y) / sprite.bounds.size.y
                );
                uv3D.Add(uv);
            }
        }

        // Создаем треугольники для каждого слоя (фронтальная и задняя стороны)
        for (int layer = 0; layer < verticalSegments - 1; layer++)
        {
            int baseIndexFront = layer * spriteVertices.Length;
            int baseIndexBack = (layer + 1) * spriteVertices.Length;

            // Используем оригинальные треугольники спрайта для каждого слоя
            for (int i = 0; i < spriteTriangles.Length; i += 3)
            {
                // Фронтальная сторона
                triangles3D.Add(baseIndexFront + spriteTriangles[i]);
                triangles3D.Add(baseIndexFront + spriteTriangles[i + 1]);
                triangles3D.Add(baseIndexFront + spriteTriangles[i + 2]);

                // Задняя сторона (обратный порядок)
                triangles3D.Add(baseIndexBack + spriteTriangles[i + 2]);
                triangles3D.Add(baseIndexBack + spriteTriangles[i + 1]);
                triangles3D.Add(baseIndexBack + spriteTriangles[i]);
            }
        }

        // Создаем боковые поверхности
        CreateSideSurfaces(vertices3D, triangles3D, spriteVertices, verticalSegments);

        mesh.vertices = vertices3D.ToArray();
        mesh.triangles = triangles3D.ToArray();
        mesh.uv = uv3D.ToArray();

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private void CreateSideSurfaces(List<Vector3> vertices, List<int> triangles, Vector2[] spriteVerts, int layers)
    {
        // Находим контур спрайта (внешние края)
        List<Vector2> contour = ExtractSpriteContour(spriteVerts);

        for (int layer = 0; layer < layers - 1; layer++)
        {
            int currentLayerStart = layer * spriteVerts.Length;
            int nextLayerStart = (layer + 1) * spriteVerts.Length;

            for (int i = 0; i < contour.Count; i++)
            {
                int nextIndex = (i + 1) % contour.Count;

                // Находим индексы вершин в массиве
                int v1 = FindVertexIndex(vertices, contour[i], layer, spriteVerts.Length);
                int v2 = FindVertexIndex(vertices, contour[nextIndex], layer, spriteVerts.Length);
                int v3 = FindVertexIndex(vertices, contour[nextIndex], layer + 1, spriteVerts.Length);
                int v4 = FindVertexIndex(vertices, contour[i], layer + 1, spriteVerts.Length);

                if (v1 >= 0 && v2 >= 0 && v3 >= 0 && v4 >= 0)
                {
                    // Первый треугольник
                    triangles.Add(v1);
                    triangles.Add(v2);
                    triangles.Add(v3);

                    // Второй треугольник
                    triangles.Add(v3);
                    triangles.Add(v4);
                    triangles.Add(v1);
                }
            }
        }
    }

    private List<Vector2> ExtractSpriteContour(Vector2[] vertices)
    {
        // Упрощенный алгоритм поиска контура
        // В реальном проекте нужно использовать более сложный алгоритм
        List<Vector2> contour = new List<Vector2>();

        // Находим крайние точки
        float minX = float.MaxValue, maxX = float.MinValue;
        float minY = float.MaxValue, maxY = float.MinValue;

        foreach (Vector2 vert in vertices)
        {
            if (vert.x < minX) minX = vert.x;
            if (vert.x > maxX) maxX = vert.x;
            if (vert.y < minY) minY = vert.y;
            if (vert.y > maxY) maxY = vert.y;
        }

        // Создаем простой прямоугольный контур (заглушка)
        // В реальности нужно анализировать альфа-канал спрайта
        contour.Add(new Vector2(minX, minY));
        contour.Add(new Vector2(maxX, minY));
        contour.Add(new Vector2(maxX, maxY));
        contour.Add(new Vector2(minX, maxY));

        return contour;
    }

    private int FindVertexIndex(List<Vector3> vertices, Vector2 target, int layer, int vertsPerLayer)
    {
        for (int i = 0; i < vertsPerLayer; i++)
        {
            int index = layer * vertsPerLayer + i;
            Vector3 vert = vertices[index];
            if (Mathf.Abs(vert.x - target.x) < 0.01f && Mathf.Abs(vert.y - target.y) < 0.01f)
            {
                return index;
            }
        }
        return -1;
    }

    private Mesh CreateFallbackMesh(Sprite sprite)
    {
        // Запасной вариант - создаем меш на основе bounding box спрайта
        Mesh mesh = new Mesh();

        Vector3[] vertices = new Vector3[8];
        Vector2[] uv = new Vector2[8];
        int[] triangles = new int[36];

        Bounds bounds = sprite.bounds;

        // 8 вершин кубоида
        vertices[0] = new Vector3(bounds.min.x, bounds.min.y, -depth / 2);
        vertices[1] = new Vector3(bounds.max.x, bounds.min.y, -depth / 2);
        vertices[2] = new Vector3(bounds.max.x, bounds.max.y, -depth / 2);
        vertices[3] = new Vector3(bounds.min.x, bounds.max.y, -depth / 2);
        vertices[4] = new Vector3(bounds.min.x, bounds.min.y, depth / 2);
        vertices[5] = new Vector3(bounds.max.x, bounds.min.y, depth / 2);
        vertices[6] = new Vector3(bounds.max.x, bounds.max.y, depth / 2);
        vertices[7] = new Vector3(bounds.min.x, bounds.max.y, depth / 2);

        // UV
        for (int i = 0; i < 8; i++)
        {
            uv[i] = new Vector2(vertices[i].x, vertices[i].y);
        }

        // Триангуляция куба
        int[] cubeTriangles = new int[] {
            0, 2, 1, 0, 3, 2, // front
            4, 5, 6, 4, 6, 7, // back
            0, 1, 5, 0, 5, 4, // bottom
            3, 6, 2, 3, 7, 6, // top
            0, 4, 7, 0, 7, 3, // left
            1, 2, 6, 1, 6, 5  // right
        };

        mesh.vertices = vertices;
        mesh.triangles = cubeTriangles;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    public Vector3 GetSurfaceNormal(Vector3 worldPosition)
    {
        // Конвертируем мировую позицию в локальную
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);

        // Упрощенное вычисление нормали - в реальности нужно делать raycast к мешу
        Vector3 directionToCenter = new Vector3(-localPos.x, 0, 0).normalized;
        return transform.TransformDirection(directionToCenter);
    }

    public bool IsPointOnTrunk(Vector3 worldPosition, float tolerance = 0.5f)
    {
        // Проверяем попадание точки в bounding box меша
        Bounds meshBounds = generatedMesh.bounds;
        Vector3 localPos = transform.InverseTransformPoint(worldPosition);
        return meshBounds.Contains(localPos);
    }

    private void OnDrawGizmosSelected()
    {
        if (generatedMesh != null)
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireMesh(generatedMesh);
        }
    }
}