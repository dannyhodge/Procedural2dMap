using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateProceduralMap : MonoBehaviour
{
    public int _mapWidth = 100;
    public int _startPos = 14;
    public int _plainInclineChance = 3;
    public int _plainEvenChance = 2;
    public int _plainDeclineChance = -1;

    public int _mountainInclineChance = 10;
    public int _mountainEvenChance = 3;
    public int _mountainDeclineChance = -1;

    public int _trenchInclineChance = -10;
    public int _trenchDeclineChance = 1;

    public List<MapPoint> surfaceMapPoints = new List<MapPoint>();
    public List<Cave> caves = new List<Cave>();
    public GameObject tile;

    void AddTile(int xPos, int yPos)
    {
        MapPoint mapPoint = new MapPoint
        {
            X = xPos,
            Y = yPos,
            Type = TileType.Dirt
        };
        surfaceMapPoints.Add(mapPoint);
        Instantiate(tile, new Vector3(xPos, yPos, 0), Quaternion.identity);
    }

    void CreateCave(MapPoint potentialCaveSpawnPoint)
    {
        Cave cave = new Cave
        {
            MapPoints = new List<MapPoint>()
        };

        cave.MapPoints.Add(potentialCaveSpawnPoint);

        Instantiate(tile, new Vector3(potentialCaveSpawnPoint.X, potentialCaveSpawnPoint.Y, 0), Quaternion.identity);
        for (int i = 0; i < 24; i++)
        {
            BranchOutCave(cave);
        }
        caves.Add(cave);
    }

    void PotentialCavePoint(MapPoint potentialNewCavePosition, Cave currentCave)
    {
        bool spaceOccupied = false;
        float randomVal = Random.value;
        var allCaves = caves;
        allCaves.Add(currentCave);

        if (randomVal > 0.95)
        {
            foreach (Cave cave in caves)
            {

                if (SpaceOccupiedInCave(cave, potentialNewCavePosition))
                {
                    spaceOccupied = true;
                    break;
                }
            }

            if (spaceOccupied == false)
            {
                Instantiate(tile, new Vector3(potentialNewCavePosition.X, potentialNewCavePosition.Y, 0), Quaternion.identity);
                currentCave.MapPoints.Add(potentialNewCavePosition);
            }
        }
    }

    void BranchOutCave(Cave currentCave)
    {
        var tempList = new List<MapPoint>();
        tempList.AddRange(currentCave.MapPoints);

        foreach (var caveMapPoint in tempList)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0) continue;

                    MapPoint potentialNewCavePosition = new MapPoint
                    {
                        X = caveMapPoint.X + x,
                        Y = caveMapPoint.Y + y,
                        Type = TileType.Empty
                    };

                    PotentialCavePoint(potentialNewCavePosition, currentCave);
                }
            }
        }

    }

    bool SpaceOccupiedInCave(Cave cave, MapPoint potentialNewCavePosition)
    {
        foreach (MapPoint caveMapPoint in cave.MapPoints)
        {
            if (caveMapPoint.X == potentialNewCavePosition.X && caveMapPoint.Y == potentialNewCavePosition.Y)
            {
                return true;
            }
        }
        return false;
    }

    AreaType NewAreaType()
    {
        return (AreaType)Random.Range(0, 3);
    }

    void CreateSurfaceArea(AreaType areaType, out int yPos, int xPos, int lastYPos)
    {
        int yPosChance;

        switch (areaType)
        {
            case AreaType.Plain:
                yPosChance = Random.Range(_plainDeclineChance, _plainInclineChance);
                if (yPosChance < 0) yPos = lastYPos - 1;
                else if (yPosChance > 0 && yPosChance <= _plainEvenChance) yPos = lastYPos;
                else yPos = lastYPos + 1;
                break;

            case AreaType.Mountain:
                yPosChance = Random.Range(_mountainDeclineChance, _mountainInclineChance);
                if (yPosChance < 0) yPos = lastYPos - 1;
                else if (yPosChance > 0 && yPosChance <= _mountainEvenChance) yPos = lastYPos;
                else yPos = lastYPos + 1;

                break;
            case AreaType.Trench:
                yPosChance = Random.Range(_trenchDeclineChance, _trenchInclineChance);

                if (yPosChance < 0) yPos = lastYPos - 1;
                else yPos = lastYPos;

                break;
            default:
                yPos = lastYPos + Random.Range(-1, 2);

                break;
        }

        for (int i = 0; i < 3; i++)
        {
            AddTile(xPos, yPos - i);
        }

    }

    void AddCaveSpawnPoint()
    {
        bool badCaveLocation = false;

        int randomId = Random.Range(0, surfaceMapPoints.Count - 1);
        MapPoint randomValue = surfaceMapPoints[randomId];
        MapPoint potentialCaveSpawnPoint = new MapPoint
        {
            X = randomValue.X,
            Y = randomValue.Y - Random.Range(20, 100),
            Type = TileType.Empty
        };

        if (caves.Count > 0)
        {
            foreach (var cave in caves)
            {
                foreach (var mapPoint in cave.MapPoints)
                {
                    if (potentialCaveSpawnPoint.X == mapPoint.X && potentialCaveSpawnPoint.Y == mapPoint.Y)
                    {
                        badCaveLocation = true;
                        break;
                    }
                }
            }
        }
        if (badCaveLocation == false)
        {
            CreateCave(potentialCaveSpawnPoint);
        }
        else
        {
            AddCaveSpawnPoint();
        }


    }

    void Start()
    {
        int lastYPos = 0;

        int areaLength = Random.Range(50, 300);
        int areaCounter = 0;
        AreaType areaType = NewAreaType();

        for (var i = 0; i < _mapWidth; i++)
        {

            if (areaCounter >= areaLength)
            {
                areaLength = Random.Range(50, 300);
                areaCounter = 0;
                areaType = NewAreaType();
            }

            int yPos;
            int xPos = i + _startPos;

            CreateSurfaceArea(areaType, out yPos, xPos, lastYPos);

            lastYPos = yPos;
            areaCounter++;
        }

        AddCaveSpawnPoint();
        AddCaveSpawnPoint();
        AddCaveSpawnPoint();
        AddCaveSpawnPoint();
        AddCaveSpawnPoint();
        AddCaveSpawnPoint();
    }

    void Update()
    {

    }
}

public class MapPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public TileType Type { get; set; }
}

public class Cave
{
    public List<MapPoint> MapPoints { get; set; }
}

public enum TileType
{
    Dirt = 0,
    Stone = 1,
    Water = 2,
    Iron = 3,
    Empty = 4
}

public enum AreaType
{
    Plain = 0,
    Mountain = 1,
    Trench = 2
}
