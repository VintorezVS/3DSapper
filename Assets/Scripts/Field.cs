using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Field : MonoBehaviour
{
    private (int, int, int) playerPosition = (0, 0, 0);
    [SerializeField] private GameObject emptyCellPrefab;
    [SerializeField] private List<GameObject> rockCellPrefabs;
    [SerializeField] private List<GameObject> explosiveCellPrefabs;
    private int size = 10;
    private int layers = 2;
    private int explosiveCount = 20;
    private Dictionary<(int, int, int), GameObject> grid = new Dictionary<(int, int, int), GameObject>();

    public Vector3 PlayerPosition
    {
        get => Utils.TupleToVector3(playerPosition);
        set => playerPosition = Utils.Vector3ToTuple(value);
    }

    private void FixedUpdate()
    {
        if (isAllEmptyCellsRevealed() || isAllExplosivesMarked())
        {
            GameManager.Instance.Win();
        }
    }

    public Cell GetPlayerCell()
    {
        return GetCell(playerPosition);
    }

    public Cell GetCell((int, int, int) coordinates)
    {
        if (!grid.ContainsKey(coordinates)) return null;
        return grid[coordinates].GetComponent<Cell>();
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

        OpenRockCell(PlayerPosition, Projection.Front);
        GetSiblingCells(playerPosition, Projection.Front).ForEach(cell => cell.IsInteractive = true);
    }

    public void OnProjectionChange(Projection currentProjection)
    {
        EmptyCell[] emptyCells = FindObjectsOfType<EmptyCell>();
        bool isZProjection = currentProjection == Projection.Front || currentProjection == Projection.Back;

        foreach (var cell in emptyCells)
        {
            (int, int, int) cellCoordinates = Utils.Vector3ToTuple(cell.transform.position);
            bool isXAlignedWithPlayer = cellCoordinates.Item1 == playerPosition.Item1;
            bool isZAlignedWithPlayer = cellCoordinates.Item3 == playerPosition.Item3;
            bool isHidden = isZProjection ? !isZAlignedWithPlayer : !isXAlignedWithPlayer;

            int explosives = GetNearExplosives(cellCoordinates, currentProjection);
            cell.Text = isHidden ? "" : explosives.ToString();
            cell.transform.rotation = Quaternion.AngleAxis(((int)currentProjection), Vector3.up);

            if (explosives == 0 && !isHidden)
            {
                OpenSiblingRockCells(cellCoordinates, currentProjection);
            }
        }
    }

    public void MovePlayerTo(Vector3 roundedCellPosition, Projection currentProjection)
    {
        GetSiblingCells(playerPosition, currentProjection).ForEach(cell => cell.IsInteractive = false);
        PlayerPosition = roundedCellPosition;
        Cell cell = GetCell(playerPosition);

        if (cell is RockCell) OpenRockCell(PlayerPosition, currentProjection);

        cell.OnHit();
        GetSiblingCells(playerPosition, currentProjection).ForEach(cell => cell.IsInteractive = true);
    }

    public bool IsCellMarked(Vector3 cellPosition)
    {
        return GetCell(Utils.Vector3ToTuple(cellPosition)).IsMarked;
    }

    public bool IsCellPosition(Vector3 cellPosition)
    {
        return grid.ContainsKey(Utils.Vector3ToTuple(cellPosition));
    }

    public void ToggleCellMark(Vector3 cellPosition)
    {
        GetCell(Utils.Vector3ToTuple(cellPosition)).ToggleMark();
    }

    private bool isAllEmptyCellsRevealed()
    {
        return !grid.Values.Any(cell => cell.GetComponent<Cell>() is RockCell);
    }

    private bool isAllExplosivesMarked()
    {
        return grid.Values.Select(obj => obj.GetComponent<Cell>()).Where(cell => cell is ExplosiveCell).All(cell => cell.IsMarked);
    }

    private int GetNearExplosives((int, int, int) value, Projection projection)
    {
        return GetSiblingCells(value, projection).Count(cell => cell.IsExplosive);
    }

    private List<Cell> GetSiblingCells((int, int, int) value, Projection projection)
    {
        return GetSiblingsCoordinates(value, projection)
            .Where(coords => grid.ContainsKey(coords))
            .Select(coords => grid[coords].GetComponent<Cell>())
            .ToList();
    }

    private List<(int, int, int)> GetSiblingsCoordinates((int, int, int) value, Projection projection)
    {
        List<(int, int, int)> result = new List<(int, int, int)>();
        bool isByXAxis = projection == Projection.Front || projection == Projection.Back;
        int x = isByXAxis ? value.Item1 : value.Item3;
        int y = value.Item2;

        for (int i = -1; i <= 1; i++)
        {
            int localX = x + i;
            for (int j = -1; j <= 1; j++)
            {
                int localY = y + j;
                if (i == 0 && j == 0) continue;
                result.Add(isByXAxis ? (localX, localY, value.Item3) : (value.Item1, localY, localX));
            }
        }

        return result;
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

    EmptyCell OpenRockCell(Vector3 position, Projection currentProjection)
    {
        (int, int, int) cellCoordinates = Utils.Vector3ToTuple(position);
        if (grid.ContainsKey(cellCoordinates)) Destroy(grid[cellCoordinates].gameObject);
        grid[cellCoordinates] = CreateCell(position, emptyCellPrefab);

        EmptyCell cell = grid[cellCoordinates].GetComponent<EmptyCell>();
        int explosives = GetNearExplosives(cellCoordinates, currentProjection);
        cell.SetNearExplosiveCount(explosives);
        cell.transform.rotation = Quaternion.AngleAxis(((int)currentProjection), Vector3.up);

        if (explosives == 0)
        {
            OpenSiblingRockCells(cellCoordinates, currentProjection);
        }

        return cell;
    }

    List<EmptyCell> OpenSiblingRockCells((int, int, int) cellCoordinates, Projection currentProjection)
    {
        return GetSiblingCells(cellCoordinates, currentProjection)
            .Where(cell => cell is RockCell)
            .Select(cell => OpenRockCell(cell.transform.position, currentProjection))
            .ToList();
    }

    GameObject CreateCell(Vector3 position, bool isExplosive)
    {
        int layer = (int)position.z;
        GameObject prefab = isExplosive
            ? explosiveCellPrefabs[layer % explosiveCellPrefabs.Count]
            : rockCellPrefabs[layer % rockCellPrefabs.Count];
        GameObject cell = Instantiate(prefab);
        cell.transform.position = position;
        cell.transform.SetParent(transform);
        return cell;
    }

    GameObject CreateCell(Vector3 position, GameObject prefab)
    {
        int layer = (int)position.z;
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
        int from = -size / 2;
        int to = (size + 1) / 2;

        IterateOverFieldLayer(
            layer,
            (coords) => coordinates = new Vector3(Random.Range(from, to), Random.Range(from, to), layer),
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
        int from = -size / 2;
        int to = (size + 1) / 2;

        for (int i = from; i < to; i++)
        {
            for (int j = from; j < to; j++)
            {
                coordinates = new Vector3(i, j, layer);
                if (new Vector3(i, j, layer) == PlayerPosition) continue;
                if (shouldBreak != null && shouldBreak(coordinates)) break;
                action(coordinates);
            }
        }
    }
}