using System;
using System.Collections;
using System.Collections.Generic;
using GruntPackage;
using LieutenantPackage;
using CommanderPackage;

namespace ArmyPackage {

    public class GruntsAtSpotDataContainer {
        public int num;

        public int totalMorale;
        public int netMorale;
        public int totalCourage;
        public int netCourage;
        public int totalHealtPoints;
        public int netHealthPoints;

        public GruntsAtSpotDataContainer(int num, int totalMorale, int netMorale, int totalCourage, int netCourage, int totalHealtPoints, int netHealthPoints) {
            this.num = num;

            this.totalMorale = totalMorale;
            this.netMorale = netMorale;
            this.totalCourage = totalCourage;
            this.netCourage = netCourage;
            this.totalHealtPoints = totalHealtPoints;
            this.netHealthPoints = netHealthPoints;
        }
    }

    public class Army {

        public bool controlledByAI;
        public int faction;
        private static readonly double populateChance = 0.3;
        private static readonly double lieutenantChance = 0.05;
        public bool isTop;
        public int maxRange;

        //null at empty square, delete list every time an item is removed and size == 0
        //used to save time when searching for targets. Memory is not an issue because most values are null and
        public List<Grunt>[,] gruntMatrix;
        public LieutenantTemplate[] lieutenantTemplates;
        public List<Lieutenant> lieutenantsList = new List<Lieutenant>();
        public Commander commander;

        public static Random random = new Random();

        public Army(int sizeX, int sizeY, int faction, int numGrunts, int maxRange, bool isTop, LieutenantTemplate[] lieutenantTemplates) {

            this.faction = faction;
            this.isTop = isTop;
            controlledByAI = isTop;
            this.maxRange = maxRange;

            this.lieutenantTemplates = lieutenantTemplates;

            gruntMatrix = new List<Grunt>[sizeX,sizeY];

            populateGrunts(numGrunts, isTop, lieutenantTemplates.Length);
        }

        public GruntsAtSpotDataContainer numGruntsAtSpot(int row, int col) {
            if (gruntMatrix[row, col] == null) return null;

            GruntsAtSpotDataContainer wrapper = new GruntsAtSpotDataContainer(gruntMatrix[row, col].Count, 0, 0, 0, 0, 0, 0);

            foreach (Grunt grunt in gruntMatrix[row, col]) {
                wrapper.totalMorale += 100;
                wrapper.netMorale += grunt.percentMorale;

                wrapper.totalCourage += 100;
                wrapper.netCourage += grunt.percentCourage;

                wrapper.totalHealtPoints += grunt.baseHealthPoints;
                wrapper.netHealthPoints += grunt.healthPoints;
            }

            return wrapper;
        }

        public bool containsLieutenantsAtSpot(int row, int col) {
            foreach (Lieutenant lieutenant in lieutenantsList) {
                if (lieutenant.row == row && lieutenant.col == col) {
                    return true;
                }
            }
            return false;
        }

        //For initial debugging and AI VS Player Mathces. This fills an army in on either the top or bottom.
        public void populateGrunts(int numGrunts, bool isTop, int numLieutenants) {
            int lieutenantTemplateIndex = 0;

            for (int numAttempts = 0; numAttempts < 30; numAttempts++) {
                if(isTop) {
                    for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                        for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                            double psubi = random.NextDouble();
                            if (psubi <= lieutenantChance && numLieutenants > 0) {
                                numLieutenants--;
                                lieutenantsList.Add(new Lieutenant(lieutenantTemplates[lieutenantTemplateIndex], row, col, !controlledByAI));
                                lieutenantTemplateIndex++;
                            }
                            else if (populateChance <= psubi) {
                                if (numGrunts > 0) {
                                    placeGrunt(row, col, new Grunt(!isTop));
                                    numGrunts--;
                                }
                                else if (numLieutenants <= 0)
                                    return;
                            }
                        }
                    }
                }
                else {
                    for (int row = gruntMatrix.GetLength(0) - 1; row >= 0; row--) {
                        for (int col = gruntMatrix.GetLength(1) - 1; col >= 0; col--) {
                            double psubi = random.NextDouble();
                            if (psubi <= lieutenantChance && numLieutenants > 0) {
                                numLieutenants--;
                                lieutenantsList.Add(new Lieutenant(lieutenantTemplates[lieutenantTemplateIndex], row, col, !controlledByAI));
                                lieutenantTemplateIndex++;
                            }
                            else if (populateChance <= psubi) {
                                if (numGrunts > 0) {
                                    placeGrunt(row, col, new Grunt(!isTop));
                                    numGrunts--;
                                }
                                else if (numLieutenants <= 0)
                                    return;
                            }
                        }
                    }
                }
            }
            throw new Exception("Tried " + 30 + " times to populate grunts and lieutenants, but failed to place: "
                + numGrunts + " Grunts and " + numLieutenants + " Lieutenants.");
        }

        public int getFaction() {
            return faction;
        }

        public void forceCalcStats(int[,] terrainGenArray) {
            for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                    if (gruntMatrix[row, col] != null) {
                        foreach (Grunt grunt in gruntMatrix[row, col]) {
                            grunt.recalculateStats(getTBS(terrainGenArray, row, col));
                        }
                    }
                }
            }
        }

        public class ClosestCandidateWrapper {
            public int distance;
            public Grunt candidate;

            public ClosestCandidateWrapper(int distance, Grunt candidate) {
                this.distance = distance;
                this.candidate = candidate;
            }
        }

        public void processTick(int[,] terrainGenArray) {
            moveUnitsAndDealDamage(terrainGenArray);
            //For each grunt, tick grunt and eventually do some other things
        }

        public void processEndTurn(int[,] terrainGenArray) {
            for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                    if (gruntMatrix[row, col] != null) {
                        foreach (Grunt grunt in gruntMatrix[row, col]) {
                            grunt.processEndTurn(getTBS(terrainGenArray, row, col));
                        }
                    }
                }
            }
        }

        public void processStartTurn(int[,] terrainGenArray) {
            for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                    if (gruntMatrix[row, col] != null) {
                        foreach (Grunt grunt in gruntMatrix[row, col]) {
                            grunt.processStartTurn();
                            grunt.recalculateStats(getTBS(terrainGenArray, row, col));
                        }
                    }
                }
            }
            foreach (Lieutenant lieutenant in lieutenantsList) {
                lieutenant.turnTick();
            }
        }

        public ClosestCandidateWrapper getClosestUnit(int row, int col, int otherArmysMaxRange) {
            //check four directions for distance 1 then 2 then 3 ect. Stop at (otherArmysMaxRange)
            //wrap distance & candidate and return

            //for compiler, delete upon finishing method
            return null;
        }

        //only called in debugging, grunts move like a king in chess
        private static readonly string[] movecodex = {"Still", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft", "Left"};

        public void moveUnitsAndDealDamage(int[,] terrainGenArray) {
            for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                    if (gruntMatrix[row, col] != null) {
                        //The following loop is necessary because we are removing grunts from this index as the program runs.
                        int gruntsLeft = gruntMatrix[row, col].Count;
                        for (int gruntIndex = 0; gruntIndex < gruntsLeft; gruntIndex++) {
                            //If Index Out of Bounds, let Crash. That should be handled in the move weights assignment function.
                            Grunt grunt = gruntMatrix[row, col][gruntIndex];
                            grunt.dealDamage();

                            try {
                            switch(grunt.move(row, col, gruntMatrix.GetLength(0), gruntMatrix.GetLength(1))) {
                                case 0:
                                    break;
                                case 1:
                                    placeGrunt(row - 1, col - 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row - 1, col - 1));
                                    gruntsLeft--;
                                    break;
                                case 2:
                                    placeGrunt(row - 1, col, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row - 1, col));
                                    gruntsLeft--;
                                    break;
                                case 3:
                                    placeGrunt(row - 1, col + 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row - 1, col + 1));
                                    gruntsLeft--;
                                    break;
                                case 4:
                                    placeGrunt(row, col + 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row, col + 1));
                                    gruntsLeft--;
                                    break;
                                case 5:
                                    placeGrunt(row + 1, col + 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row + 1, col + 1));
                                    gruntsLeft--;
                                    break;
                                case 6:
                                    placeGrunt(row + 1, col, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row + 1, col));
                                    gruntsLeft--;
                                    break;
                                case 7:
                                    placeGrunt(row + 1, col - 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row + 1, col - 1));
                                    gruntsLeft--;
                                    break;
                                case 8:
                                    placeGrunt(row, col - 1, grunt);
                                    removeGrunt(row, col, grunt);
                                    grunt.recalculateStats(getTBS(terrainGenArray, row, col - 1));
                                    gruntsLeft--;
                                    break;
                                default:
                                    throw new Exception("Grunt.move() gave invalid return, not an element of [0,1,2,3,4,5,6,7]");
                            }
                            } catch (IndexOutOfRangeException ex) {
                                throw new IndexOutOfRangeException("Tried to call invalid move tile. Move weights may be in" +
                                    " an invalid state when grunt.move() was called: row: " + row + ", col: " + col + ", Move Weights: "
                                    + string.Join(", ", grunt.moveWeights + "; Exception:" + ex));
                            }
                        }
                    }
                }
            }
        }

        // let game crash if this throws an error.
        public void removeGrunt(int row, int col, Grunt grunt) {
            //let crash if index out of bounds exception
            List<Grunt> gruntsAtThisSpot = gruntMatrix[row, col];
            //we want this to crash because there is something very wrong with our game. If this is happening often, it is a crirical bug. Loosing track of a unit could make the game unplayable if we didnt force a crash
            if (gruntsAtThisSpot.Count == 0)
                throw new Exception("Lost Track Of Grunt? Can not find grunt at [" + row + "," + col + "] to remove. This location is null");
            foreach (Grunt gruntToCheck in gruntsAtThisSpot) {
                //only checks for reference equality, but these should be the same reference, not two different grunts.
                if (grunt == gruntToCheck) {
                    gruntsAtThisSpot.Remove(grunt);
                    if (gruntsAtThisSpot.Count == 0) {
                        gruntMatrix[row, col] = null;
                    }
                    return;
                }
            }
            throw new Exception("Lost Track Of Grunt? Can not find grunt at [" + row + "," + col + "] to remove. This location has grunts. Their references were not pointed to the exact same object in memory.");
        }

        public void placeGrunt(int row, int col, Grunt grunt) {
            //let crash if index out of bounds exception
            List<Grunt> gruntsAtThisSpot = gruntMatrix[row, col];
            if (gruntsAtThisSpot == null) {
                gruntsAtThisSpot = new List<Grunt>();
                gruntsAtThisSpot.Add(grunt);
                gruntMatrix[row, col] = gruntsAtThisSpot;
            }
            else {
                gruntsAtThisSpot.Add(grunt);
            }
        }

        public static TileBattleModifiers getTBS(int[,] terrainGenArray, int row, int col) {
            return TileBattleModifiers.tileMods[terrainGenArray[row, col]];
        }
    }
}