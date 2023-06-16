using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class Field : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject emptyCellPrefab;
    [SerializeField] private List<GameObject> rockCellPrefabs;
    [SerializeField] private List<GameObject> explosiveCellPrefabs;
    [SerializeField] private TextMeshProUGUI rockCellsCounterText;
    // ENCAPSULATION
    private (int, int, int) playerPosition = (0, 0, 0);
    private int explosiveCount = 20;
    private Dictionary<(int, int, int), GameObject> grid = new Dictionary<(int, int, int), GameObject>();

    #region Events
    public event System.Action OnReady;
    #endregion

    public int Size { get; private set; } = 10;
    public int Layers { get; private set; } = 2;
    public Vector3Int PlayerPosition
    {
        get => Utils.TupleToVector3Int(playerPosition);
        private set => playerPosition = Utils.Vector3ToTuple(value);
    }

    private void OnEnable()
    {
        player.OnMove += HandlePlayerMove;
        player.OnProjectionChange += HandleProjectionChange;
    }

    private void OnDisable()
    {
        player.OnMove -= HandlePlayerMove;
        player.OnProjectionChange -= HandleProjectionChange;
    }

    private void FixedUpdate()
    {
        rockCellsCounterText.text = System.String.Format("Cells left: {0}", GetRockCellsCount());
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
        this.Size = size;
        this.Layers = layers;
        this.explosiveCount = explosiveCount;
    }

    public void Generate(int size, int layers, int explosiveCount)
    {
        SetFieldParameters(size, layers, explosiveCount);
        Generate();
    }

    public void Generate()
    {
        int explosivesPerLayer = explosiveCount / Layers;

        for (int layer = 0; layer < Layers; layer++)
        {
            GenerateCellsForLayer(layer, layer == Layers - 1 ? explosivesPerLayer + explosiveCount % Layers : explosivesPerLayer);
        }

        OpenRockCell(PlayerPosition);
        OnReady?.Invoke();
    }

    public bool IsCellMarked(Vector3Int cellPosition)
    {
        return GetCell(Utils.Vector3ToTuple(cellPosition)).IsMarked;
    }

    public bool IsCellPosition(Vector3Int cellPosition)
    {
        return grid.ContainsKey(Utils.Vector3ToTuple(cellPosition));
    }

    public void CheckCell()
    {
        if (IsCellMarked(PlayerPosition)) return;

        Cell cell = GetCell(playerPosition);
        if (cell is RockCell) OpenRockCell(PlayerPosition);
        cell.OnHit();
    }

    public void ToggleCellMark()
    {
        GetCell(playerPosition).ToggleMark();
    }

    public void ToggleCellMark(Vector3Int cellPosition)
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

    private int GetRockCellsCount()
    {
        return grid.Values.Where(cell => !(cell.GetComponent<Cell>() is EmptyCell)).Count();
    }

    private int GetNearExplosives((int, int, int) value)
    {
        return GetSiblingCells(value).Count(cell => cell.IsExplosive);
    }

    private List<Cell> GetSiblingCells((int, int, int) value)
    {
        return GetSiblingsCoordinates(value)
            .Where(coords => grid.ContainsKey(coords))
            .Select(coords => grid[coords].GetComponent<Cell>())
            .ToList();
    }

    private List<RockCell> GetSiblingRockCells((int, int, int) value)
    {
        return GetSiblingsCoordinates(value)
            .Where(coords => grid.ContainsKey(coords) && grid[coords].GetComponent<Cell>() is RockCell)
            .Select(coords => grid[coords].GetComponent<RockCell>())
            .ToList();
    }

    private List<(int, int, int)> GetSiblingsCoordinates((int, int, int) value)
    {
        List<(int, int, int)> result = new List<(int, int, int)>();
        Projection currentProjection = MainManager.Instance.currentProjection;
        Vector3Int projectionMask = currentProjection.RightwardDirection + currentProjection.UpwardDirection;

        int x = projectionMask.x == 0 ? value.Item3 : value.Item1;
        int y = projectionMask.y == 0 ? value.Item3 : value.Item2;

        for (int i = -1; i <= 1; i++)
        {
            int localX = x + i;
            for (int j = -1; j <= 1; j++)
            {
                int localY = y + j;
                if (i == 0 && j == 0) continue;
                result.Add((
                    projectionMask.x == 0 ? value.Item1 : localX,
                    projectionMask.y == 0 ? value.Item2 : localY,
                    projectionMask.x == 0 ? localX : projectionMask.y == 0 ? localY : value.Item3
                ));
            }
        }

        return result;
    }

    private void HandlePlayerMove(Vector3Int nextPosition)
    {
        PlayerPosition = nextPosition;
    }

    public void HandleProjectionChange(Projection currentProjection)
    {
        EmptyCell[] emptyCells = FindObjectsOfType<EmptyCell>();
        Vector3Int projectionMask = currentProjection.RightwardDirection + currentProjection.UpwardDirection;

        foreach (var cell in emptyCells)
        {
            (int, int, int) cellCoordinates = Utils.Vector3ToTuple(cell.transform.position);
            bool isXAlignedWithPlayer = cellCoordinates.Item1 == playerPosition.Item1;
            bool isYAlignedWithPlayer = cellCoordinates.Item2 == playerPosition.Item2;
            bool isZAlignedWithPlayer = cellCoordinates.Item3 == playerPosition.Item3;
            bool isVisible = projectionMask.x == 0 && isXAlignedWithPlayer
                || projectionMask.y == 0 && isYAlignedWithPlayer
                || projectionMask.z == 0 && isZAlignedWithPlayer;

            if (isVisible)
            {
                cell.SetNearExplosiveCount(GetNearExplosives(cellCoordinates));
                cell.transform.rotation = Quaternion.LookRotation(currentProjection.ForwardDirection, currentProjection.UpwardDirection);
                StartCoroutine(OpenSiblingRockCells(Utils.Vector3ToVector3Int(cell.transform.position)));
            }
            else
            {
                cell.Text = "";
            }
        }
    }

    void GenerateCellsForLayer(int layer, int explosiveCount)
    {
        List<Vector3Int> explosiveCellsCoordinates = new List<Vector3Int>();

        for (int i = 0; i < explosiveCount; i++)
        {
            Vector3Int coordinates = GenerateUniqueRandomCoordinateForLayer(layer, explosiveCellsCoordinates);
            explosiveCellsCoordinates.Add(coordinates);
        }

        IterateOverFieldLayer(
            layer,
            (coords) => grid[Utils.Vector3ToTuple(coords)] = CreateCell(coords, explosiveCellsCoordinates.Contains(coords))
        );
    }

    void OpenRockCell(Vector3Int position)
    {
        CreateEmptyCell(position);
        StartCoroutine(OpenSiblingRockCells(position));
    }

    IEnumerator OpenSiblingRockCells(Vector3Int position)
    {
        if (GetNearExplosives(Utils.Vector3ToTuple(position)) != 0) yield break;

        List<RockCell> cellsToOpen = GetSiblingRockCells(Utils.Vector3ToTuple(position)).ToList();
        List<RockCell> nextSiblingCells = new List<RockCell>();
        List<EmptyCell> openedCells = new List<EmptyCell>();
        int step = 0;

        while (cellsToOpen.Count != 0)
        {
            foreach (var cell in cellsToOpen)
            {
                EmptyCell newCell = CreateEmptyCell(Utils.Vector3ToVector3Int(cell.transform.position));
                (int, int, int) coordinates = Utils.Vector3ToTuple(newCell.transform.position);
                int explosives = GetNearExplosives(coordinates);
                if (explosives == 0) openedCells.Add(newCell);
            }

            foreach (var cell in openedCells)
            {
                List<RockCell> siblingCells = GetSiblingRockCells(Utils.Vector3ToTuple(cell.transform.position));
                foreach (var sibling in siblingCells)
                {
                    if (!nextSiblingCells.Contains(sibling))
                    {
                        nextSiblingCells.Add(sibling);
                    }
                }
            }

            yield return step++;

            cellsToOpen = nextSiblingCells.Where(cell => cell != null).ToList();
            nextSiblingCells = new List<RockCell>();
            openedCells = new List<EmptyCell>();
        }
    }

    GameObject CreateCell(Vector3Int position, bool isExplosive)
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

    GameObject CreateCell(Vector3Int position, GameObject prefab)
    {
        GameObject cell = Instantiate(prefab);
        cell.transform.position = position;
        cell.transform.SetParent(transform);
        return cell;
    }

    GameObject CreateExplosiveCell(Vector3Int coordinates)
    {
        return CreateCell(coordinates, true);
    }

    GameObject CreateRockCell(Vector3Int coordinates)
    {
        return CreateCell(coordinates, false);
    }

    EmptyCell CreateEmptyCell(Vector3Int position)
    {
        (int, int, int) cellCoordinates = Utils.Vector3ToTuple(position);
        if (grid.ContainsKey(cellCoordinates)) Destroy(grid[cellCoordinates].gameObject);
        grid[cellCoordinates] = CreateCell(position, emptyCellPrefab);

        EmptyCell cell = grid[cellCoordinates].GetComponent<EmptyCell>();
        int explosives = GetNearExplosives(cellCoordinates);
        cell.SetNearExplosiveCount(explosives);

        Projection currentProjection = MainManager.Instance.currentProjection;
        cell.transform.rotation = Quaternion.LookRotation(currentProjection.ForwardDirection, currentProjection.UpwardDirection);
        return cell;
    }

    Vector3Int GenerateUniqueRandomCoordinateForLayer(int layer, List<Vector3Int> usedCoordinates)
    {
        Vector3Int coordinates = PlayerPosition;
        System.Predicate<Vector3Int> isAvailable = (coords) => coordinates != PlayerPosition && !usedCoordinates.Any(used => used == coordinates);
        int from = -Size / 2;
        int to = (Size + 1) / 2;

        IterateOverFieldLayer(
            layer,
            (coords) => coordinates = new Vector3Int(Random.Range(from, to), Random.Range(from, to), layer),
            isAvailable
        );

        if (isAvailable(coordinates))
        {
            return coordinates;
        }

        return GetFirstEmptyCoordinateForLayer(layer, usedCoordinates);
    }

    Vector3Int GetFirstEmptyCoordinateForLayer(int layer, List<Vector3Int> usedCoordinates)
    {
        Vector3Int coordinates = PlayerPosition;
        System.Predicate<Vector3Int> isAvailable = (coords) => coordinates != PlayerPosition && !usedCoordinates.Any(used => used == coordinates);
        IterateOverFieldLayer(layer, (coords) => coordinates = coords, isAvailable);

        if (!isAvailable(coordinates))
        {
            throw new System.Exception("Cannot find empty coordinates on the field!");
        }

        return coordinates;
    }

    void IterateOverFieldLayer(int layer, System.Action<Vector3Int> action)
    {
        IterateOverFieldLayer(layer, action, null);
    }

    // ABSTRACTION
    void IterateOverFieldLayer(int layer, System.Action<Vector3Int> action, System.Predicate<Vector3Int> shouldBreak)
    {
        Vector3Int coordinates;
        int from = -Size / 2;
        int to = (Size + 1) / 2;

        for (int i = from; i < to; i++)
        {
            for (int j = from; j < to; j++)
            {
                coordinates = new Vector3Int(i, j, layer);
                if (new Vector3Int(i, j, layer) == PlayerPosition) continue;
                if (shouldBreak != null && shouldBreak(coordinates)) break;
                action(coordinates);
            }
        }
    }
}