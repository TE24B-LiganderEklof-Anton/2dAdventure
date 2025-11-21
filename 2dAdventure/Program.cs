using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Schema;
using Raylib_cs;


//testing generation
// World worldTest = new();
// int testX = 10;
// int testY = 10;
// while (true)
// {
//     worldTest.writeFOV(testY, testX, 5, 30);
//     // testX++;
//     testY++;
//     Console.ReadLine();
// }

//movement
World world = new();

KeyboardKey upKey = KeyboardKey.W;
KeyboardKey downKey = KeyboardKey.S;
KeyboardKey rightKey = KeyboardKey.D;
KeyboardKey leftKey = KeyboardKey.A;

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
    
    Console.WriteLine(pos);
    Raylib.EndDrawing();
}

class Tile
{
    public int content = 0;
}
class World
{
    //first coordinate is Y second X, easier for me to visualize
    public Dictionary<int, Dictionary<int, Tile>> yAxis = new();
    int tileSize = 30;
    int tileGap = 10;
    int padding = 30;

    Dictionary<int, Func<Action>> contentDrawMethods = new()
    {
        
    };
    (int, int) GetTileGraphicsPos(int yPos, int xPos)
    {
        int graphicsYPos = padding + ((tileSize+tileGap)*yPos);
        int graphicsXPos = padding + ((tileSize+tileGap)*xPos);
        return (graphicsYPos, graphicsXPos);
    }
    void DrawTile(int yPos, int xPos, Tile tile)
    {
        (int graphicsYPos, int graphicsXPos) = GetTileGraphicsPos(yPos, xPos);
        Color color = Color.Red;
        int content = tile.content;
        if (content == 0)
        {
            color = Color.Purple;
        }
        if (content == 1)
        {
            color = Color.Blue;
        }
        if (content == 2)
        {
            color = Color.Pink;
        }
        if (content == 3)
        {
            color = Color.Green;
        }
        if (content == 4)
        {
            color = Color.Yellow;
        }
        if (content == 5)
        {
            color = Color.Brown;
        }
        // Raylib.DrawCircle(graphicsXPos, graphicsYPos, tileSize/2, color);
        Raylib.DrawRectangle(graphicsXPos, graphicsYPos, tileSize, tileSize, color);
    }
    public void DrawFOV(int yCenter, int xCenter, int yRadius, int xRadius)
    {
        
        Console.Clear();
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
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        if (xAxis.ContainsKey(x))
        {
            return xAxis[x];
        }
        else
        {
            Tile newTile = GenerateTileAtCoords(y, x);
            return newTile;
        }
    }
    public Tile GenerateTileAtCoords(int y, int x)
    {
        Dictionary<int, Tile> xAxis = GetXAxisDict(y);
        Tile newTile = new();

        //will be replaced with a proper algoritm, random for now
        newTile.content = Random.Shared.Next(6);

        xAxis.Add(x, newTile);

        return newTile;
    }
}