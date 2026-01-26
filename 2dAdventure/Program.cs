using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using Raylib_cs;

//movement

KeyboardKey upKey = KeyboardKey.Up;
KeyboardKey downKey = KeyboardKey.Down;
KeyboardKey rightKey = KeyboardKey.Right;
KeyboardKey leftKey = KeyboardKey.Left;

int fovRadius = 20;

int windowSize = (2 * World.padding) + ((fovRadius * 2 + 1) * World.tileGap) + ((fovRadius * 2 + 1) * World.tileSize);
Raylib.InitWindow(windowSize, windowSize, "Game");
World world = new();
Raylib.SetTargetFPS(60);
Vector2 pos = new(0, 0);
float moveCooldown = 0.1f;

// Texture2D tx = Raylib.LoadTexture("Assets/tree1.png");
Texture2D textureTest = Raylib.LoadTexture("Assets/tree1.png");

while (!Raylib.WindowShouldClose())
{
    Raylib.BeginDrawing();
    Raylib.ClearBackground(Color.Black);

    world.DrawFOV((int)pos.Y, (int)pos.X, fovRadius, fovRadius);

    Vector2 targetPos = pos;

    if (Raylib.IsKeyDown(upKey))
    {
        targetPos.Y--;
    }
    if (Raylib.IsKeyDown(downKey))
    {
        targetPos.Y++;
    }
    if (Raylib.IsKeyDown(rightKey))
    {
        targetPos.X++;
    }
    if (Raylib.IsKeyDown(leftKey))
    {
        targetPos.X--;
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

class TerrainDecoration
{
    public Texture2D texture;
    public TerrainDecoration(string textureToLoad)
    {
        texture = Raylib.LoadTexture(textureToLoad);
    }
    public Vector2 posOffset = new(0.5f, 0.5f);
    public float scale = 1;
}
class Tile
{
    public Terrain terrain;
    public int colorVariation = 0;

    public TerrainDecoration decoration = null;
    public Vector2 decorationOffset = new(0f,0f);

    // public Structure structure;
}
class Terrain
{
    public Color tileColor = Color.Pink;
    public int colorVariation = 0;
    // public Structure[] validStructures;
    public bool traversable = true;
    public Dictionary<TerrainDecoration, float> terrainDecorations;
}
class Water : Terrain
{
    public Water()
    {
        // tileColor = Color.Blue;
        tileColor = new(50, 150, 235);
        traversable = true;
        colorVariation = 15;
    }
}
class HeightNoise : FastNoiseLite
{
    public void Setup(int seed)
    {
        SetNoiseType(FastNoiseLite.NoiseType.Perlin);
        SetFrequency(0.025f);
        SetSeed(seed);
        SetFractalType(FastNoiseLite.FractalType.FBm);
    }
}
class World
{
    //noise
    HeightNoise heightNoise = new HeightNoise();
    FastNoiseLite biomeNoise = new();

    //height based terrain classes
    Terrain debug = new();
    Terrain biome = new(); //biome will be replaced depending on biome noise
    Terrain water = new Water();
    Terrain mountains = new();
    Terrain snowyMountains = new();
    Terrain beach = new();

    //biome based terrain classes
    Terrain plains = new();
    Terrain desert = new();
    Terrain forest = new();
    Terrain flowerField = new();

    //terrain decorations
    TerrainDecoration tree1 = new("Assets/tree1.png");
    TerrainDecoration tree2 = new("Assets/tree2.png");
    TerrainDecoration flower1 = new("Assets/flower1.png");
    TerrainDecoration flower2 = new("Assets/flower2.png");
    TerrainDecoration flower3 = new("Assets/flower3.png");
    TerrainDecoration flower4 = new("Assets/flower4.png");
    TerrainDecoration flower5 = new("Assets/flower5.png");

    Dictionary<Terrain,float> biomeValues;
    Dictionary<Terrain,float> heightValues;
    public World()
    {
        //terrainDecorations
        tree1.scale = 2f;
        tree2.scale = 2f;

        //noise
        int seed = Random.Shared.Next(10000000);

        heightNoise.Setup(seed);

        biomeNoise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
        biomeNoise.SetFrequency(0.02f);
        biomeNoise.SetSeed(seed + 12345);
        biomeNoise.SetFractalType(FastNoiseLite.FractalType.DomainWarpIndependent);
        biomeNoise.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);
        biomeNoise.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Hybrid);
        biomeNoise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2Reduced);
        biomeNoise.SetCellularJitter(1.5f);
        biomeNoise.SetDomainWarpAmp(2f);

        //terrain
        debug.tileColor = Color.Purple;

        mountains.tileColor = Color.Gray;
        mountains.traversable = true;
        mountains.colorVariation = 20;

        snowyMountains.tileColor = Color.White;
        snowyMountains.traversable = true;
        snowyMountains.colorVariation = 20;

        beach.tileColor = new(194, 178, 128);
        beach.colorVariation = 20;

        plains.tileColor = new(0, 145, 0);
        plains.colorVariation = 20;
        plains.terrainDecorations = new()
        {
            {tree1, 0.01f},
            {tree2, 0.01f},
            {flower1, 0.003f},
            {flower2, 0.003f},
            {flower3, 0.003f},
            {flower4, 0.003f},
            {flower5, 0.003f},
        };

        flowerField.tileColor = new(0,175,0);
        flowerField.colorVariation = 15;
        flowerField.terrainDecorations = new()
        {
            {flower1, 0.1f},
            {flower2, 0.1f},
            {flower3, 0.1f},
            {flower4, 0.1f},
            {flower5, 0.1f},
        };

        desert.tileColor = new(245, 245, 220);
        desert.colorVariation = 20;

        forest.tileColor = Color.DarkGreen;
        forest.colorVariation = 15;
        forest.terrainDecorations = new()
        {
            {tree1,0.1f},
            {tree2,0.1f},
        };

        //Generation values
        heightValues = new()
        {
            {water, 1f},
            {beach, 0.1f},
            {biome, 0.6f},
            {mountains, 0.3f},
            {snowyMountains, 0.7f}
        };
        biomeValues = new()
        {
            {desert, 0.5f},
            {plains, 0.5f},
            {forest,0.5f},
            {flowerField, 0.2f}
        };
    }

    Dictionary<int, Dictionary<int, Tile>> yAxis = new();
    public static int tileSize = 30;
    public static int tileGap = 0;
    public static int padding = 30;
    static int GetTerrainFrequency(List<Tile> tileList, Terrain terrain)
    {
        int amount = 0;
        foreach (Tile tile in tileList)
        {
            if (tile.terrain == terrain) amount++;
        }
        return amount;
    }
    (int, int) GetGraphicsPos(int yPos, int xPos)
    {
        int graphicsYPos = padding + ((tileSize + tileGap) * yPos);
        int graphicsXPos = padding + ((tileSize + tileGap) * xPos);
        return (graphicsYPos, graphicsXPos);
    }
    void DrawTileTerrainDecoration(Tile tile, int tileGraphicsY, int tileGraphicsX)
    {

        TerrainDecoration deco = tile.decoration;
        float scale = deco.scale * (float)tileSize / 5f;
        // Console.WriteLine(deco.texture.Height);
        int decoGraphicsY = tileGraphicsY - (int)((deco.texture.Height * scale * 0.6f) + (tile.decorationOffset.Y*scale));
        int decoGraphicsX = tileGraphicsX - (int)((deco.texture.Width * scale * 0.31f) + (tile.decorationOffset.X*scale));
        Raylib.DrawTextureEx(deco.texture, new(decoGraphicsX, decoGraphicsY), 0, scale, Color.White);
        // Console.WriteLine(deco.textureDirectory);
    }
    void DrawTile(int yPos, int xPos, Tile tile, bool hightlight)
    {
        (int graphicsYPos, int graphicsXPos) = GetGraphicsPos(yPos, xPos);

        Color color = tile.terrain.tileColor;
        color = AdjustColor(color, tile.colorVariation);

        if (hightlight) Raylib.DrawRectangle(graphicsXPos - tileGap / 2, graphicsYPos - tileGap / 2, tileSize + tileGap, tileSize + tileGap, Color.White);

        Raylib.DrawRectangle(graphicsXPos, graphicsYPos, tileSize, tileSize, color);
        // if (tile.decoration != null) DrawTileTerrainDecoration(tile, graphicsYPos, graphicsXPos);

    }
    public void DrawFOV(int yCenter, int xCenter, int yRadius, int xRadius)
    {
        Dictionary<Tile, Vector2> tilesWithDeco = new();
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
                if (tile.decoration != null)
                {
                    tilesWithDeco.Add(tile, new(screenX, screenY));
                }
            }
            foreach (KeyValuePair<Tile, Vector2> pair in tilesWithDeco)
            {
                (int graphicsY, int graphicsX) = GetGraphicsPos((int)pair.Value.Y, (int)pair.Value.X);
                DrawTileTerrainDecoration(pair.Key, graphicsY, graphicsX);
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
    static int AdjustRGBValue(int value, int change)
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
    float GetTotalDictionaryValue<T>(Dictionary<T, float> dictValues)
    {
        float totalValue = 0;
        foreach (KeyValuePair<T, float> pair in dictValues)
        {
            totalValue += pair.Value;
        }
        return totalValue;
    }
    T SelectKeyFromValueDict<T>(Dictionary<T, float> dict, float selectionValue)
    {
        float totalValue = GetTotalDictionaryValue(dict);

        T selected = default;

        float value = 0;

        foreach (KeyValuePair<T, float> pair in dict)
        {
            value += pair.Value;
            if (value > selectionValue)
            {
                // Console.WriteLine(pair.Key);
                selected = pair.Key;
                break;
            }
        }
        return selected;
    }
    float AdjustNoiseValue(float value, float totalValue)
    {
        return ((value + 1) / 2) * totalValue;
    }
    Terrain SelectTerrainFromNoise(int yPos, int xPos)
    {



        float totalHeightValue = GetTotalDictionaryValue(heightValues);
        float totalBiomeValue = GetTotalDictionaryValue(biomeValues);


        //choose from noise
        float heightSelectionValue = AdjustNoiseValue(heightNoise.GetNoise(xPos, yPos), totalHeightValue);
        Terrain selectedTerrain = SelectKeyFromValueDict(heightValues, heightSelectionValue);

        if (selectedTerrain == biome)
        {
            float biomeSelectionValue = AdjustNoiseValue(biomeNoise.GetNoise(xPos, yPos), totalBiomeValue);
            // Console.WriteLine(biomeSelectionValue);
            selectedTerrain = SelectKeyFromValueDict(biomeValues, biomeSelectionValue);
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
    Tile GenerateTileAtCoords(int y, int x)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        Tile newTile = new();

        // Terrain selectedTerrain = SelectTerrain(y, x);
        Terrain selectedTerrain = SelectTerrainFromNoise(y, x);
        newTile.terrain = selectedTerrain;
        int colorVariation = newTile.terrain.colorVariation;
        newTile.colorVariation = Random.Shared.Next(-colorVariation, colorVariation);

        if (selectedTerrain.terrainDecorations != null)
        {
            // Console.WriteLine(selectedTerrain.tileColor);
            // float totalTerrainDecoValue = GetTotalDictionaryValue(selectedTerrain.terrainDecorations);
            // float totalTerrainDecoValue = GetTotalDictionaryValue(selectedTerrain.terrainDecorations);
            float terrainDecoSelectionValue = (float)Random.Shared.NextDouble();
            TerrainDecoration selectedTerrainDeco = SelectKeyFromValueDict(selectedTerrain.terrainDecorations, terrainDecoSelectionValue);
            newTile.decorationOffset.Y = Random.Shared.Next(-50,50)/25;
            newTile.decorationOffset.X = Random.Shared.Next(-50,50)/25;
            newTile.decoration = selectedTerrainDeco;
        }

        xAxis.Add(x, newTile);

        return newTile;
    }
}