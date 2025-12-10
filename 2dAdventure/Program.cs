using System.ComponentModel;
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

int fovRadius = 100;

int windowSize = (2 * world.padding) + ((fovRadius * 2 + 1) * world.tileGap) + ((fovRadius * 2 + 1) * world.tileSize);
Raylib.InitWindow(windowSize, windowSize, "Game");
Raylib.SetTargetFPS(60);
Vector2 pos = new(0, 0);
float moveCooldown = 0.1f;

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);




    world.DrawFOV((int)pos.Y, (int)pos.X, fovRadius, fovRadius);

    Vector2 targetPos = pos;

    if (Raylib.IsKeyDown(upKey))
    {
        targetPos.Y--;
        // Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(downKey))
    {
        targetPos.Y++;
        // Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(rightKey))
    {
        targetPos.X++;
        // Raylib.WaitTime(moveCooldown);
    }
    if (Raylib.IsKeyDown(leftKey))
    {
        targetPos.X--;
        // Raylib.WaitTime(moveCooldown);
    }

    if (pos != targetPos)
    {
        Tile targetTile = world.GetTileFromCoords((int)targetPos.Y, (int)targetPos.X);
        if (targetTile.terrain.traversable)
        {
            pos = targetPos;
            Raylib.WaitTime(moveCooldown);
        }
    }



    // Console.WriteLine(pos);
    Raylib.EndDrawing();
}

class Tile
{
    public Terrain terrain;
    public int colorVariation = 0;

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

    public int colorVariation = 0;
    // public Structure[] validStructures;
    public bool traversable = true;
}
class World
{
    //noise
    FastNoiseLite noise = new();

    //terrain classes
    Terrain debug = new();
    Terrain plains = new();
    Terrain water = new();
    Terrain mountains = new();
    Terrain snowyMountains = new();

    Terrain beach = new();
    public World()
    {
        //noise
        noise.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        noise.SetFrequency(0.05f);
        noise.SetSeed(Random.Shared.Next(100000));
        noise.SetFractalType(FastNoiseLite.FractalType.FBm);
        
        //terrain
        debug.tileColor = Color.Purple;

        plains.tileColor = new(0, 225, 0);
        plains.colorVariation = 20;

        water.tileColor = new(50, 150, 235);
        water.traversable = true;
        water.colorVariation = 15;

        mountains.tileColor = Color.Gray;
        mountains.traversable = true;
        mountains.colorVariation = 20;

        snowyMountains.tileColor = Color.White;
        snowyMountains.traversable = true;
        snowyMountains.colorVariation = 20;

        beach.tileColor = new(194, 178, 128);
        beach.colorVariation = 20;
    }

    //first coordinate is Y second X, easier for me to visualize
    public Dictionary<int, Dictionary<int, Tile>> yAxis = new();
    public int tileSize = 10;
    public int tileGap = 0;
    public int padding = 30;
    // int tileColorVariation = 30;
    static int GetTerrainFrequency(List<Tile> tileList, Terrain terrain)
    {
        int amount = 0;
        foreach (Tile tile in tileList)
        {
            if (tile.terrain == terrain) amount++;
        }
        return amount;
    }
    (int, int) GetTileGraphicsPos(int yPos, int xPos)
    {
        int graphicsYPos = padding + ((tileSize + tileGap) * yPos);
        int graphicsXPos = padding + ((tileSize + tileGap) * xPos);
        return (graphicsYPos, graphicsXPos);
    }
    void DrawTile(int yPos, int xPos, Tile tile, bool hightlight)
    {
        (int graphicsYPos, int graphicsXPos) = GetTileGraphicsPos(yPos, xPos);

        Color color = tile.terrain.tileColor;
        color = AdjustColor(color, tile.colorVariation);

        if (hightlight) Raylib.DrawRectangle(graphicsXPos - tileGap / 2, graphicsYPos - tileGap / 2, tileSize + tileGap, tileSize + tileGap, Color.White);

        Raylib.DrawRectangle(graphicsXPos, graphicsYPos, tileSize, tileSize, color);
    }
    public void DrawFOV(int yCenter, int xCenter, int yRadius, int xRadius)
    {
        Tile centerTile = GetTileFromCoords(yCenter, xCenter);
        int yMin = yCenter - yRadius;
        int yMax = yCenter + yRadius;
        int xMin = xCenter - xRadius;
        int xMax = xCenter + xRadius;

        int screenY = 0;
        for (int y = yMin; y <= yMax; y++, screenY++)
        {
            int screenX = 0;
            for (int x = xMin; x <= xMax; x++, screenX++)
            {
                Tile tile = GetTileFromCoords(y, x);

                bool shouldHightlight = false;
                if (tile == centerTile) shouldHightlight = true;

                DrawTile(screenY, screenX, tile, shouldHightlight);
            }
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
        return GetTileFromCoords(y, x, true);
    }
    public Tile GetTileFromCoords(int y, int x, bool generateIfMissing)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        if (xAxis.ContainsKey(x))
        {
            return xAxis[x];
        }
        else if (generateIfMissing)
        {
            Tile newTile = GenerateTileAtCoords(y, x);
            return newTile;
        }
        else
        {
            return null;
        }

    }
    List<Tile> GetNeighboringTiles(int yPos, int xPos, int radius)
    {
        List<Tile> tiles = new();
        for (int y = yPos - radius; y <= yPos + radius; y++)
        {
            for (int x = xPos - radius; x <= xPos + radius; x++)
            {
                if (y == yPos && x == xPos)
                {
                    continue;
                }
                Tile tile = GetTileFromCoords(y, x, false);
                if (tile != null) tiles.Add(tile);
            }
        }
        return tiles;
    }
    int AdjustRGBValue(int value, int change)
    {
        value += change;
        if (value > 255) value = 255;
        else if (value < 0) value = 0;

        return value;
    }
    Color AdjustColor(Color color, int amount)
    {
        Color colorAdjusted = new(
        AdjustRGBValue(color.R, amount),
        AdjustRGBValue(color.G, amount),
        AdjustRGBValue(color.B, amount)
    );
        return colorAdjusted;
    }

    Terrain SelectTerrainFromNoise(int yPos, int xPos)
    {
        Dictionary<Terrain, float> terrainValues = new()
        {
            {water, 1f},
            {beach, 0.1f},
            {plains, 0.6f},
            {mountains, 0.3f},
            {snowyMountains, 0.7f}
        };
        float totalTerrainValue = 0;
        foreach (float value in terrainValues.Values)
        {
            totalTerrainValue += value;
        }

        // foreach (KeyValuePair<Terrain, float> pair in terrainValues)
        // {
        //     terrainValues[pair.Key] /= totalTerrainValue;
        // }
        //choose from noise
        float selectionValue = ((noise.GetNoise(xPos, yPos)+1)/2)*totalTerrainValue;

        Terrain selectedTerrain = debug;

        foreach (KeyValuePair<Terrain, float> pair in terrainValues)
        {
            totalTerrainValue -= pair.Value;
            if (totalTerrainValue < selectionValue)
            {
                // Console.WriteLine(pair.Key);
                selectedTerrain = pair.Key;
                break;
            }
        }
        return selectedTerrain;
    }
    Terrain SelectTerrain(int yPos, int xPos)
    {
        Dictionary<Terrain, float> terrainValues = new()
        {
            {water, 0.05f},
            {plains, 0.5f},
            {mountains, 0.01f},
            {beach, 0},
            {debug, 0}
        };
        Dictionary<Terrain, float> neighboringBonus = new()
        {
            {water, 0.85f},
            {plains, 0.6f},
            {mountains, 1f},
            {beach, 0},
            {debug, 1}
        };

        List<Tile> neighboringTiles = GetNeighboringTiles(yPos, xPos, 1);

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
        foreach (KeyValuePair<Terrain, float> pair in terrainValues)
        {
            totalTerrainValue += pair.Value;
        }

        float selectionValue = (float)Random.Shared.NextDouble() * totalTerrainValue;

        Terrain selectedTerrain = debug;

        foreach (KeyValuePair<Terrain, float> pair in terrainValues)
        {
            totalTerrainValue -= pair.Value;
            if (totalTerrainValue < selectionValue)
            {
                // Console.WriteLine(pair.Key);
                selectedTerrain = pair.Key;
                break;
            }
        }

        if (selectedTerrain == plains)
        {
            int waterAmount = GetTerrainFrequency(neighboringTiles, water);
            if (waterAmount >= 1) selectedTerrain = beach;
        }

        return selectedTerrain;
    }
    public Tile GenerateTileAtCoords(int y, int x)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        Tile newTile = new();


        // Terrain selectedTerrain = SelectTerrain(y, x);
        Terrain selectedTerrain = SelectTerrainFromNoise(y, x);
        newTile.terrain = selectedTerrain;
        int colorVariation = newTile.terrain.colorVariation;
        newTile.colorVariation = Random.Shared.Next(-colorVariation, colorVariation);

        xAxis.Add(x, newTile);

        return newTile;
    }
}