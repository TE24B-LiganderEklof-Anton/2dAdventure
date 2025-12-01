using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using Raylib_cs;

//movement
World world = new();

KeyboardKey upKey = KeyboardKey.Up;
KeyboardKey downKey = KeyboardKey.Down;
KeyboardKey rightKey = KeyboardKey.Right;
KeyboardKey leftKey = KeyboardKey.Left;

Raylib.InitWindow(800,600, "Game");
Raylib.SetTargetFPS(60);
Vector2 pos = new(0,0);
float moveCooldown = 0.1f;

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    world.DrawFOV((int)pos.Y, (int)pos.X, 5,5);


    if (Raylib.IsKeyDown(upKey))
    {
        pos.Y--;
        Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(downKey))
    {
        pos.Y++;
        Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(rightKey))
    {
        pos.X++;
        Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(leftKey))
    {
        pos.X--;
        Raylib.WaitTime(moveCooldown);
    }
    
    // Console.WriteLine(pos);
    Raylib.EndDrawing();
}

class Tile
{
    public Terrain terrain;
    // public Structure structure;
}
enum Structure
{ 
    hut,
    ruins,
    campfire,
}

class Terrain
{
    public Color tileColor = Color.Pink;
    // public Structure[] validStructures;
    public bool traversable = true;
}

class World
{
    //terrain classes
    Terrain debug = new();
    Terrain plains = new();
    Terrain water = new();
    Terrain mountains = new();

    public World()
    {
        debug.tileColor = Color.Purple;

        plains.tileColor = new(0, 255, 0);

        water.tileColor = Color.Blue;
        water.traversable = false;

        mountains.tileColor = Color.Gray; 
    }
    

    //first coordinate is Y second X, easier for me to visualize
    public Dictionary<int, Dictionary<int, Tile>> yAxis = new();
    int tileSize = 30;
    int tileGap = 10;
    int padding = 30;
    
    (int, int) GetTileGraphicsPos(int yPos, int xPos)
    {
        int graphicsYPos = padding + ((tileSize+tileGap)*yPos);
        int graphicsXPos = padding + ((tileSize+tileGap)*xPos);
        return (graphicsYPos, graphicsXPos);
    }
    void DrawTile(int yPos, int xPos, Tile tile)
    {
        int randomRGB = 10;
        (int graphicsYPos, int graphicsXPos) = GetTileGraphicsPos(yPos, xPos);
        Color color = Color.Pink;
        if (tile.terrain != null)
        {
            // Color randomizedColor = new(color.R + Random.Shared.Next(-randomRGB, randomRGB), color.G + Random.Shared.Next(-randomRGB, randomRGB), color.B + Random.Shared.Next(-randomRGB, randomRGB));
            color = tile.terrain.tileColor;
        }

        Raylib.DrawRectangle(graphicsXPos, graphicsYPos, tileSize, tileSize, color);
    }
    public void DrawFOV(int yCenter, int xCenter, int yRadius, int xRadius)
    {
        
        int yMin = yCenter - yRadius;
        int yMax = yCenter + yRadius;
        int xMin = xCenter - xRadius;
        int xMax = xCenter + xRadius;

        int relativeY = 0;
        for (int y = yMin; y <= yMax; y++)
        {
            int relativeX = 0;
            for (int x = xMin; x <= xMax; x++)
            {
                Tile tile = GetTileFromCoords(y, x);
                DrawTile(relativeY, relativeX, tile);
                relativeX++;
            }
            relativeY++;
        }
    }
    Dictionary<int, Tile> GetXAxisDict(int y)
    {
        if (yAxis.ContainsKey(y))
        {
            return yAxis[y];
        }
        else
        {
            Dictionary<int, Tile> xAxis = new();
            yAxis.Add(y, xAxis);
            return xAxis;
        }
    }
    public Tile GetTileFromCoords(int y, int x)
    {
        return GetTileFromCoords(y,x, true);
    }
    public Tile GetTileFromCoords(int y, int x, bool generateIfMissing)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        if (xAxis.ContainsKey(x))
        {
            return xAxis[x];
        }
        else if(generateIfMissing)
        {
            Tile newTile = GenerateTileAtCoords(y, x);
            return newTile;
        }
        else
        {
            return null;
        }
        
    }

    public List<Tile> GetNeighboringTiles(int yPos, int xPos, int radius)
    {
        List<Tile> tiles = new();
        for (int y = yPos-radius; y <= yPos+radius; y++)
        {
            for (int x = xPos-radius; x <= xPos+radius; x++)
            {
                if(y == yPos && x == xPos)
                {
                    continue;
                }
                Tile tile = GetTileFromCoords(y,x, false);
                if (tile != null) tiles.Add(tile);
            }
        }
        return tiles;
    }
    public Tile GenerateTileAtCoords(int y, int x)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        Tile newTile = new();
        

        Dictionary<Terrain, float> terrainValues = new()
        {
            {water, 0.1f},
            {plains, 0.5f},
            {mountains, 0.7f},
            {debug, 0}
        };
        Dictionary<Terrain, float> neighboringBonus = new()
        {
            {water, 0.75f},
            {plains, 0.5f},
            {mountains, -0.2f},
            {debug, 1}
        };

        List<Tile> neighboringTiles = GetNeighboringTiles(y, x, 1);

        foreach (Tile tile in neighboringTiles)
        {
            if (tile == null) continue;

            Terrain terrain = tile.terrain;
            terrainValues[terrain] += neighboringBonus[terrain];
            if (terrain == water)
            {
                terrainValues[mountains] *= 0.1f;
            }
            if (terrain == mountains)
            {
                terrainValues[water] *= 0.1f;
            }
        }

        float totalTerrainValue = 0;
        foreach (KeyValuePair<Terrain,float> pair in terrainValues)
        {
            // Console.WriteLine(pair.Value);
            totalTerrainValue += pair.Value;
        }
        // Console.WriteLine(totalTerrainValue);
        // Console.WriteLine("////////////////////////////////////////////////////");

        float selectionValue = (float)Random.Shared.NextDouble() * totalTerrainValue;
        // Console.WriteLine(selectionValue);

        Terrain selectedTerrain = plains;

        foreach (KeyValuePair<Terrain,float> pair in terrainValues)
        {
            totalTerrainValue -= pair.Value;
            if (totalTerrainValue < selectionValue)
            {
                Console.WriteLine(pair.Key);
                selectedTerrain = pair.Key; //issue is here
                // Console.WriteLine("break");
                break;
            }
        }
        Console.WriteLine("==========================================");
        
        //  if (selectedTerrain == water)
        // {
        //     Console.WriteLine("water");
        // }
        // else if (selectedTerrain == debug)
        // {
        //     Console.WriteLine("debug");
        // }
        // else if (selectedTerrain == plains)
        // {
        //     Console.WriteLine("plains");
        // }
        // else if (selectedTerrain == null)
        // {
        //     Console.WriteLine("null");
        // }
        // else
        // {
        //     Console.WriteLine("none");
        // }
        

        newTile.terrain = selectedTerrain;
        // Console.WriteLine(neighboringTiles.Count);
        // Console.WriteLine(selectedTerrain);



        // // //will be replaced with a proper algoritm, random for now
        // int terrainType = Random.Shared.Next(3);

        // if (terrainType == 0)
        // {
        //     newTile.terrain = water;
        // }
        // else if (terrainType == 1)
        // {
        //     newTile.terrain = plains;
        // }
        // else if (terrainType == 2)
        // {
        //     newTile.terrain = mountains;
        // }

        xAxis.Add(x, newTile);

        return newTile;
    }
}