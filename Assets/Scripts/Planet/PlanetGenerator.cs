using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    // Start is called before the first frame update
    [Header("Основные параметры планеты")]
    [Tooltip("Число подразделений icosphere. Чем больше значение, тем более гладкая сфера.")]
    public int subdivisions = 3;
    [Tooltip("Радиус планеты")]
    public float radius = 10f;

    [Header("Параметры рельефа")]
    [Tooltip("Масштаб шума")]
    public float noiseScale = 0.1f;
    [Tooltip("Коэффициент высоты для рельефа")]
    public float heightMultiplier = 2f;

    [Header("Материалы")]
    [SerializeField] private Material planetMaterial;
    void Start()
    {
        // 1. Генерация базовой icosphere
        Mesh icosphereMesh = IcosphereGenerator.GenerateIcosphere(radius, subdivisions);

        // 2. Построение dual-сетки для получения шестиугольного разбиения
        Mesh dualMesh = DualMeshGenerator.GenerateDualMesh(icosphereMesh);

        // 3. Применение шума для создания рельефа
        Mesh terrainMesh = TerrainGenerator.ApplyNoise(dualMesh, noiseScale, heightMultiplier);

        // Назначаем сгенерированную сетку компоненту MeshFilter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = terrainMesh;

        // Присваиваем базовый материал для визуализации
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = planetMaterial;
    }
}
