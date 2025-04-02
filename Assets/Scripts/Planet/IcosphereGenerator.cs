using System.Collections.Generic;
using UnityEngine;

public static class IcosphereGenerator
{
    public static Mesh GenerateIcosphere(float radius, int subdivisions)
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        // Золотое сечение
        float t = (1f + Mathf.Sqrt(5f)) / 2f;

        // Инициализация 12 вершин икосаэдра
        vertices.AddRange(new Vector3[]
        {
            new Vector3(-1,  t,  0),
            new Vector3( 1,  t,  0),
            new Vector3(-1, -t,  0),
            new Vector3( 1, -t,  0),
            new Vector3( 0, -1,  t),
            new Vector3( 0,  1,  t),
            new Vector3( 0, -1, -t),
            new Vector3( 0,  1, -t),
            new Vector3( t,  0, -1),
            new Vector3( t,  0,  1),
            new Vector3(-t,  0, -1),
            new Vector3(-t,  0,  1)
        });

        // Нормализуем вершины и умножаем на радиус
        for (int i = 0; i < vertices.Count; i++)
            vertices[i] = vertices[i].normalized * radius;

        // Базовые треугольники икосаэдра
        int[] baseTriangles = new int[]
        {
            0, 11, 5,
            0, 5, 1,
            0, 1, 7,
            0, 7, 10,
            0, 10, 11,
            1, 5, 9,
            5, 11, 4,
            11, 10, 2,
            10, 7, 6,
            7, 1, 8,
            3, 9, 4,
            3, 4, 2,
            3, 2, 6,
            3, 6, 8,
            3, 8, 9,
            4, 9, 5,
            2, 4, 11,
            6, 2, 10,
            8, 6, 7,
            9, 8, 1
        };
        triangles.AddRange(baseTriangles);

        // Подразделение каждого треугольника
        for (int i = 0; i < subdivisions; i++)
        {
            // Используем кеш для избежания дублирования вычислений
            Dictionary<long, int> middlePointIndexCache = new Dictionary<long, int>();
            List<int> newTriangles = new List<int>();

            for (int j = 0; j < triangles.Count; j += 3)
            {
                int v1 = triangles[j];
                int v2 = triangles[j + 1];
                int v3 = triangles[j + 2];

                int a = GetMiddlePoint(v1, v2, ref vertices, ref middlePointIndexCache, radius);
                int b = GetMiddlePoint(v2, v3, ref vertices, ref middlePointIndexCache, radius);
                int c = GetMiddlePoint(v3, v1, ref vertices, ref middlePointIndexCache, radius);

                newTriangles.AddRange(new int[] { v1, a, c });
                newTriangles.AddRange(new int[] { v2, b, a });
                newTriangles.AddRange(new int[] { v3, c, b });
                newTriangles.AddRange(new int[] { a, b, c });
            }
            triangles = newTriangles;
        }

        Mesh mesh = new Mesh();
        mesh.indexFormat = vertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : mesh.indexFormat;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    static int GetMiddlePoint(int indexA, int indexB, ref List<Vector3> vertices,
                              ref Dictionary<long, int> cache, float radius)
    {
        // Формируем уникальный ключ независимо от порядка индексов
        long smallerIndex = Mathf.Min(indexA, indexB);
        long greaterIndex = Mathf.Max(indexA, indexB);
        long key = (smallerIndex << 32) + greaterIndex;

        if (cache.TryGetValue(key, out int ret))
            return ret;

        Vector3 point1 = vertices[indexA];
        Vector3 point2 = vertices[indexB];
        Vector3 middle = ((point1 + point2) * 0.5f).normalized * radius;

        vertices.Add(middle);
        int newIndex = vertices.Count - 1;
        cache.Add(key, newIndex);
        return newIndex;
    }
}
