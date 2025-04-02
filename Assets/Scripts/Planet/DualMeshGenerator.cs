using System.Collections.Generic;
using UnityEngine;

public static class DualMeshGenerator
{
    public static Mesh GenerateDualMesh(Mesh originalMesh)
    {
        Vector3[] origVertices = originalMesh.vertices;
        int[] origTriangles = originalMesh.triangles;

        // Словарь: для каждой вершины исходной сетки сохраняем список центроидов треугольников, в которых она участвует.
        Dictionary<int, List<Vector3>> vertexToCentroids = new Dictionary<int, List<Vector3>>();

        // Вычисляем центроиды треугольников
        for (int i = 0; i < origTriangles.Length; i += 3)
        {
            int i1 = origTriangles[i];
            int i2 = origTriangles[i + 1];
            int i3 = origTriangles[i + 2];
            Vector3 v1 = origVertices[i1];
            Vector3 v2 = origVertices[i2];
            Vector3 v3 = origVertices[i3];
            Vector3 centroid = (v1 + v2 + v3) / 3f;
            // Чтобы центроиды точно лежали на сфере, нормализуем их
            centroid = centroid.normalized * v1.magnitude;

            AddCentroid(vertexToCentroids, i1, centroid);
            AddCentroid(vertexToCentroids, i2, centroid);
            AddCentroid(vertexToCentroids, i3, centroid);
        }

        List<Vector3> dualVertices = new List<Vector3>();
        List<int> dualTriangles = new List<int>();

        // Для каждой вершины исходной сетки формируем многоугольную ячейку
        foreach (var kvp in vertexToCentroids)
        {
            List<Vector3> cellPoints = kvp.Value;
            Vector3 center = origVertices[kvp.Key];

            // Для корректной сортировки учитываем плоскость, проходящую через центр ячейки.
            // Выбираем вектор, ортогональный центру, как опорный (например, Vector3.forward, если не параллелен центру)
            Vector3 reference = Vector3.Cross(center, Vector3.up);
            if (reference == Vector3.zero)
                reference = Vector3.Cross(center, Vector3.forward);

            // Сортируем точки по углу относительно центра
            cellPoints.Sort((a, b) =>
            {
                float angleA = Mathf.Atan2(Vector3.Dot(Vector3.Cross(reference, a - center), center), Vector3.Dot(reference, a - center));
                float angleB = Mathf.Atan2(Vector3.Dot(Vector3.Cross(reference, b - center), center), Vector3.Dot(reference, b - center));
                return angleA.CompareTo(angleB);
            });

            // Запоминаем начальный индекс для вершин ячейки
            int startIndex = dualVertices.Count;
            dualVertices.AddRange(cellPoints);
            // Вычисляем центр полигона как среднее значение его вершин
            Vector3 polyCenter = Vector3.zero;
            foreach (var pt in cellPoints)
                polyCenter += pt;
            polyCenter /= cellPoints.Count;
            dualVertices.Add(polyCenter);
            int centerIndex = dualVertices.Count - 1;

            // Формируем треугольники ячейки (сверху центр, затем последовательные пары вершин)
            for (int i = 0; i < cellPoints.Count; i++)
            {
                int current = startIndex + i;
                int next = startIndex + ((i + 1) % cellPoints.Count);
                dualTriangles.Add(current);
                dualTriangles.Add(next);
                dualTriangles.Add(centerIndex);
            }
        }

        Mesh dualMesh = new Mesh();
        dualMesh.indexFormat = dualVertices.Count > 65000 ? UnityEngine.Rendering.IndexFormat.UInt32 : dualMesh.indexFormat;
        dualMesh.vertices = dualVertices.ToArray();
        dualMesh.triangles = dualTriangles.ToArray();
        dualMesh.RecalculateNormals();
        return dualMesh;
    }

    static void AddCentroid(Dictionary<int, List<Vector3>> dict, int key, Vector3 centroid)
    {
        if (!dict.ContainsKey(key))
            dict[key] = new List<Vector3>();
        dict[key].Add(centroid);
    }
}
