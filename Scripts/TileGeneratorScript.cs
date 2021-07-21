using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;

public class TileGeneratorScript : MonoBehaviour {
    public const int startX = -12;
    public const int startY = -15;
    public const int numTilesX = 24;
    public const int numTilesY = 30;

    public const int seed = 2;
    public static readonly System.Random terrainRandom = new System.Random(seed);
    private static int[,] lanscapeGenNums = new int[numTilesY, numTilesX];

    public static readonly String[] codex = {"Empty", "GrassLandTile", "MudTile", "HighlandsTile", "MarshTile", "CrackedDesertTile"};
    public static readonly int[] weights = {490, 12, 4, 6, 1, 3};
    private static Tile[] tilesForConversion;

    public Tilemap landscape;

    public int[,] GetTerrainGenArray() {
        return lanscapeGenNums;
    }

    private static String arrToString2D(int[,] a) {
        var s = "{";
        var i = 0;
        foreach (var item in a) s += (i++ > 0 ? i % a.GetLength(1) == 1 ? "},\n{" : "," : "") + item;
        s += '}';

        return s;
    }

    private static void generateLandscapeGenNumsFromWeights() {

        //The following two loops turns integer weights into probabilitis less than one that can be used to run the generator.
        float[] probsFromWeights = new float[weights.Length];
        float totalWeight = 0;
        for (int index = 0; index < weights.Length; index++) {
            totalWeight += weights[index];
        }
        for (int index = 0; index < weights.Length; index++) {
            probsFromWeights[index] = weights[index] / totalWeight;
        }

        //handles any edge case due to rounding
        bool found;
        //the probablity of each specific element representing a tile,
        //will be converted into an integer representing that element
        float psubi;

        //the following nested loop runs the randomizer and seeds lanscapeGenNums
        for (int row = 0; row < lanscapeGenNums.GetLength(0); row++) {
            for (int col = 0; col < lanscapeGenNums.GetLength(1); col++) {

                psubi = (float) terrainRandom.NextDouble();

                found = false;
                for (int index = 0; index < probsFromWeights.Length; index++) {
                    psubi -= probsFromWeights[index];
                    if (psubi <= 0) {
                        found = true;
                        lanscapeGenNums[row, col] = index;
                        break;
                    }
                }
                if (found == false) {
                    lanscapeGenNums[row, col] = probsFromWeights.Length - 1;
                }
            }
        }
    }

    //returns if detected good seed and sucessful completion
    private static bool populateEmptiesLandscapeGenNums() {

        int diversityCount = 6;

        //this loop checks for diversity in the generated landscape
        for (int row = 0; row < lanscapeGenNums.GetLength(0); row++) {
            for (int col = 0; col < lanscapeGenNums.GetLength(1); col++) {
                if (lanscapeGenNums[row, col] != 0) diversityCount--;
            }
        }
        //bad seed
        if (diversityCount > 0) {
          Console.WriteLine("Bad seed!!!");
          return false;
        }

        //List of columns to skip on the next row to prevent infinite spreading
        List<int> skipCol = new List<int>();
        //List of values of neighbouring cells. A non-zero candadite will be randomly picked to fill a selected cell.
        List<int> candidates = new List<int>();
        //if there is any empty left in the List, and the process should continue
        int foundEmpty = 1;
        int prevEmpty = 0;
        //debug if issue
        int somethingWentWrongCount = 0;

        while (foundEmpty > 0) {

            //The next few lines protect against infinite loops and possible errors
            if (prevEmpty == foundEmpty) throw new Exception("No change in empty cells");
            prevEmpty = foundEmpty;
            foundEmpty = 0;
            somethingWentWrongCount++;

            if (somethingWentWrongCount >= 20) throw new Exception("Issue processing generated landscape from Seed: "
                + seed + "Diversity: " + diversityCount + arrToString2D(lanscapeGenNums));

            //We don't need to skip random elements in the top row just because we filled in the bottom one
            skipCol.Clear();
            //Iterate through each element of the matrix looking for empty cells
            for (int row = 0; row < lanscapeGenNums.GetLength(0); row++) {
                for (int col = 0; col < lanscapeGenNums.GetLength(1); col++) {
                    //skips processing the element at this column
                    if (skipCol.Contains(col)) {
                        skipCol.Remove(col);
                    }
                    //processes the element at this column
                    else {
                        //checks if empty
                        if (lanscapeGenNums[row, col] == 0) {
                            foundEmpty++;

                            //generates candadites, accounts for edges
                            if (row > 0) candidates.Add(lanscapeGenNums[row - 1, col]);
                            if (col > 0) candidates.Add(lanscapeGenNums[row, col - 1]);
                            if (row < lanscapeGenNums.GetLength(0) - 1) candidates.Add(lanscapeGenNums[row + 1, col]);
                            if (col < lanscapeGenNums.GetLength(1) - 1) candidates.Add(lanscapeGenNums[row, col + 1]);

                            //keeps only nonzero candidates
                            while (candidates.Contains(0)) {
                                candidates.Remove(0);
                            }
                            if (candidates.Count > 0) {
                                //assigns random candidate
                                lanscapeGenNums[row, col] = candidates[terrainRandom.Next(candidates.Count)];
                                //clears candidates
                                candidates.Clear();
                                //skips cell below this
                                skipCol.Add(col);
                                //skips cell after this
                                col++;
                            }
                        }
                    }
                }
            }
            Console.WriteLine(arrToString2D(lanscapeGenNums) + "\n\n\n");
        }
        return true;
    }

    public static Tile getMudTile() {
        return getTileByName("MudTile");
    }

    private static Tile getTileByName(string name) {
            Tile tile = Resources.Load<Tile>("Tiles/" + name);
//            Tile tile = (Tile) null;

            if (tile is null) {
            //this will be a throw eventually
                print("Unable to find requested Asset in tile folder:" + name);
                return null;
            }
            return tile;
    }

    private static void populateTiles() {
        tilesForConversion = new Tile[codex.Length];
        tilesForConversion[0] = null;
        for (int codexIndex = 1; codexIndex < codex.Length; codexIndex++) {
            tilesForConversion[codexIndex] = getTileByName(codex[codexIndex]);
        }
    }

    private void replaceTiles() {
        if (tilesForConversion == null) throw new Exception("You must call populateTiles() first");
        for (int row = 0; row < lanscapeGenNums.GetLength(0); row++) {
            for (int col = 0; col < lanscapeGenNums.GetLength(1); col++) {
                Vector3Int offset = new Vector3Int(row + startX, col + startY, 0);
                landscape.SetTile(offset, tilesForConversion[lanscapeGenNums[row, col]]);
            }
        }
    }

    void awake() {
        //tries to generate a new landscape until one works
        generateLandscapeGenNumsFromWeights();
        int somethingWentWrongCount = 0;
        while (!populateEmptiesLandscapeGenNums()) {
            somethingWentWrongCount++;
            if (somethingWentWrongCount >= 4) throw new Exception("Error Populating Seed" + seed);
            generateLandscapeGenNumsFromWeights();
        }
    }

    // Start is called before the first frame update
    void Start() {
        //renders the tiles by getting loading the game objects,
        populateTiles();
        //then drawing them
        replaceTiles();
    }

//    private int count = 0;

    //Update is called once per frame
//    void Update() {
//        count++;
//        if (count >= 60) {
//            print("Game is still running");
//
//        }
//    }
}