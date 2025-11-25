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
    public Structure structure;
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
    public Structure[] validStructures;
    public bool traversable = true;
}

class World
{
    //terrain classes
    Terrain plains = new();
    Terrain water = new();
    Terrain mountains = new();

    public World()
    {
        plains.tileColor = Color.Green;

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
        (int graphicsYPos, int graphicsXPos) = GetTileGraphicsPos(yPos, xPos);
        Color color = tile.terrain.tileColor;

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

    public List<Tile> GetNeighboringTiles(int yPos, int xPos)
    {
        List<Tile> tiles = new();
        for (int y = yPos-1; y <= yPos+1; y++)
        {
            for (int x = xPos-1; x <= xPos+1; x++)
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

        Dictionary<Terrain, int> terrainValues = new()
        {
            {water, 1},
            {plains, 3},
            {mountains, 1},
        };

        List<Tile> neighboringTiles = GetNeighboringTiles(y, x);

        foreach (Tile tile in neighboringTiles)
        {
            Terrain terrain = tile.terrain;
            terrainValues[terrain] += 1;
        }

        
        // Console.WriteLine(neighboringTiles.Count);




        //will be replaced with a proper algoritm, random for now
        int terrainType = Random.Shared.Next(3);

        if (terrainType == 0)
        {
            newTile.terrain = water;
        }
        else if (terrainType == 1)
        {
            newTile.terrain = plains;
        }
        else if (terrainType == 2)
        {
            newTile.terrain = mountains;
        }

        xAxis.Add(x, newTile);

        return newTile;
    }
}