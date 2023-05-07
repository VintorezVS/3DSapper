using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    private (int, int, int) playerPosition = (0, 0, 0);
    [SerializeField] private List<GameObject> prefabs;
    [SerializeField] private List<GameObject> explosivePrefabs;
    private int size = 10;
    private int layers = 2;
    private int explosiveCount = 20;
    private Dictionary<(int, int, int), GameObject> grid = new Dictionary<(int, int, int), GameObject>();

    public Vector3 PlayerPosition
    {
        get => Utils.TupleToVector3(playerPosition);
        set
        {
            playerPosition = Utils.Vector3ToTuple(value);
        }
    }

    public void SetFieldParameters(int size, int layers, int explosiveCount)
    {
        this.size = size;
        this.layers = layers;
        this.explosiveCount = explosiveCount;
    }

    public void Generate(int size, int layers, int explosiveCount)
    {
        SetFieldParameters(size, layers, explosiveCount);
        Generate();
    }

    public void Generate()
    {
        int explosivesPerLayer = explosiveCount / layers;

        for (int layer = 0; layer < layers; layer++)
        {
            GenerateCellsForLayer(layer, layer == layers - 1 ? explosivesPerLayer + explosiveCount % layers : explosivesPerLayer);
        }
    }

    void GenerateCellsForLayer(int layer, int explosiveCount)
    {
        List<Vector3> explosiveCellsCoordinates = new List<Vector3>();

        for (int i = 0; i < explosiveCount; i++)
        {
            Vector3 coordinates = GenerateUniqueRandomCoordinateForLayer(layer, explosiveCellsCoordinates);
            explosiveCellsCoordinates.Add(coordinates);
        }

        IterateOverFieldLayer(
            layer,
            (coords) => grid[Utils.Vector3ToTuple(coords)] = CreateCell(coords, explosiveCellsCoordinates.Contains(coords))
        );
    }

    GameObject CreateCell(Vector3 position, bool isExplosive)
    {
        int layer = (int)position.z;
        GameObject prefab = isExplosive ? explosivePrefabs[layer] : prefabs[layer];
        GameObject cell = Instantiate(prefab);
        cell.transform.position = position;
        cell.transform.SetParent(transform);
        return cell;
    }

    GameObject CreateExplosiveCell(Vector3 coordinates)
    {
        return CreateCell(coordinates, true);
    }

    GameObject CreateSimpleCell(Vector3 coordinates)
    {
        return CreateCell(coordinates, false);
    }

    Vector3 GenerateUniqueRandomCoordinateForLayer(int layer, List<Vector3> usedCoordinates)
    {
        Vector3 coordinates = PlayerPosition;
        System.Predicate<Vector3> isAvailable = (coords) => coordinates != PlayerPosition && !usedCoordinates.Any(used => used == coordinates);
        IterateOverFieldLayer(
            layer,
            (coords) => coordinates = new Vector3(Random.Range(0, size), Random.Range(0, size), layer),
            isAvailable
        );

        if (isAvailable(coordinates))
        {
            return coordinates;
        }

        return GetFirstEmptyCoordinateForLayer(layer, usedCoordinates);
    }

    Vector3 GetFirstEmptyCoordinateForLayer(int layer, List<Vector3> usedCoordinates)
    {
        Vector3 coordinates = PlayerPosition;
        System.Predicate<Vector3> isAvailable = (coords) => coordinates != PlayerPosition && !usedCoordinates.Any(used => used == coordinates);
        IterateOverFieldLayer(layer, (coords) => coordinates = coords, isAvailable);

        if (!isAvailable(coordinates))
        {
            throw new System.Exception("Cannot find empty coordinates on the field!");
        }

        return coordinates;
    }

    void IterateOverFieldLayer(int layer, System.Action<Vector3> action)
    {
        IterateOverFieldLayer(layer, action, null);
    }

    void IterateOverFieldLayer(int layer, System.Action<Vector3> action, System.Predicate<Vector3> shouldBreak)
    {
        Vector3 coordinates;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                coordinates = new Vector3(i, j, layer);
                if (new Vector3(i, j, layer) == PlayerPosition) continue;
                if (shouldBreak != null && shouldBreak(coordinates)) break;
                action(coordinates);
            }
        }
    }
}