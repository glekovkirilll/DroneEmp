using UnityEngine;

public static class TerrainGenerator
{
    public static Mesh ApplyNoise(Mesh mesh, float noiseScale, float heightMultiplier)
    {
        Vector3[] vertices = mesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 vertex = vertices[i];
            // Используем координаты X и Z для получения шумового значения
            float noise = Mathf.PerlinNoise(vertex.x * noiseScale, vertex.z * noiseScale);
            // Определяем смещение вдоль нормали (направление от центра сферы)
            Vector3 normal = vertex.normalized;
            vertices[i] += normal * noise * heightMultiplier;
        }
        mesh.vertices = vertices;
        mesh.RecalculateNormals();
        return mesh;
    }
}
