using System;
using System.Collections;
using System.Collections.Generic;
using ArmyPackage;
using GruntPackage;
using LieutenantPackage;

namespace BattleProcessorPackage {
    public class BattleProcessor {

        public bool isOnTurn;

        public List<Army> listOfArmies;

        public int[,] terrainGenArray;

        //called once
        public BattleProcessor(int[,] terrainGenArray) {
            isOnTurn = true;
            this.terrainGenArray = terrainGenArray;

            //One type of battle. Creates an army of 25 men
            Army playerArmy = new Army(terrainGenArray.GetLength(0), terrainGenArray.GetLength(1), 0, 25, 4, false, 
                new LieutenantTemplate[] {LieutenantsList.jeremy, LieutenantsList.daniel, LieutenantsList.sergi});
            Army computerArmy = new Army(terrainGenArray.GetLength(0), terrainGenArray.GetLength(1), 1, 25, 4, true, 
                new LieutenantTemplate[] {LieutenantsList.jeremy, LieutenantsList.jeremy, LieutenantsList.jeremy});

            listOfArmies = new List<Army>();
            listOfArmies.Add(playerArmy);
            listOfArmies.Add(computerArmy);
        }

        public int tickCount = 0;
        public static int ticksPerTurn = 5;

        public void processTick() {
            tickCount++;

            foreach (Army army in listOfArmies) {
                //Moves units and deals damage
                army.processTick(terrainGenArray);
            }

            //turn every 5 ticks
            if (tickCount >= ticksPerTurn) {
                foreach (Army army in listOfArmies) {
                    army.processStartTurn(terrainGenArray);
                }

                isOnTurn = true;
                findDamageTargets();

                aiTurn();
                playerTurn();
                tickCount = 0;
            }
        }

        public void forceCalcStats() {
            foreach (Army army in listOfArmies) {
                army.forceCalcStats(terrainGenArray);
            }
        }

        public void endTurn() {
            foreach (Army army in listOfArmies) {
                army.processEndTurn(terrainGenArray);
            }
            isOnTurn = false;
        }

        public void playerTurn() {
            //Display Options for the player, Ends when the player hits "End Turn"
        }

        public void aiTurn() {
            //AI moves its lieutenants and casts a series of commands.
        }

        //findDamageTargets(Army); for each grunt in army, get it's coordinates. call closestEnemy(**coordinates**) for each enemy army (faction). looks through a list of candidates and finds the closestEnemy and assigns it to the Grunt by calling grunt.setShootingAt(Grunt shootingAt); This is set once per turn, not on ticks or moves.
        public void findDamageTargets() {
            foreach (Army assigningTo in listOfArmies) {
                List<Grunt>[,] gruntMatrix = assigningTo.gruntMatrix;
                for (int row = 0; row < gruntMatrix.GetLength(0); row++) {
                    for (int col = 0; col < gruntMatrix.GetLength(1); col++) {
                        if (gruntMatrix[row, col] != null) {
                            foreach (Grunt grunt in gruntMatrix[row, col]) {
                                Army.ClosestCandidateWrapper bestCandidate = null;  //can be null
                                foreach (Army army in listOfArmies) {
                                    if (army != assigningTo) { //checks that they are not the exact same reference
                                        Army.ClosestCandidateWrapper proposal = army.getClosestUnit(row, col, assigningTo.maxRange);
                                        if (proposal != null) {
                                            if (proposal.distance < bestCandidate.distance) {
                                                bestCandidate = proposal;
                                            }
                                        }
                                    }
                                }
                                grunt.setShootingAt(bestCandidate != null ? bestCandidate.candidate : null); //can be null
                            }
                        }
                    }
                }
            }
        }
    }
}