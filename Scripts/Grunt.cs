using System;
using System.Collections;
using System.Collections.Generic;

namespace GruntPackage {

    public class TileBattleModifiers {
        //a flat modifier (Can't go past 100%) to courage (Can be negative) for standing on this tile
        public int percentCourageModifier;
        //a flat modifier (Can't go past 100%) to morale (Can be negative) for standing on this tile
        public int percentMoraleModifier;
        //a flat modifier that ruduces Damage EG: Boulder, Fence, Mud
        public int percentCoverModifier;

        public TileBattleModifiers(int flatMorale, int flatCourage, int flatCover) {

            this.percentMoraleModifier = flatMorale;
            this.percentCourageModifier = flatCourage;
            this.percentCoverModifier = flatCover;
        }

        private static readonly TileBattleModifiers mudMods = new TileBattleModifiers(0, -10, 10);
        private static readonly TileBattleModifiers grassLandMods = new TileBattleModifiers(-5, 20, 0);
        private static readonly TileBattleModifiers highlandsMods = new TileBattleModifiers(20, -10, 5);
        private static readonly TileBattleModifiers marshMods = new TileBattleModifiers (-10, -25, 30);
        private static readonly TileBattleModifiers crackedDesertMods = new TileBattleModifiers (-20, 50, -10);
        public static readonly TileBattleModifiers[] tileMods = {null, grassLandMods, mudMods, highlandsMods, marshMods, crackedDesertMods};
    }

    public class Grunt {

        public int percentMorale;
        public int percentCourage;
        public int percentCover;
        public int healthPoints;

        private int tempPercentMoraleModifier;
        private int tempPercentCourageModifier;
        private int tempPercentCoverModifier;

        public int baseMorale;
        public int baseCourage;
        public int baseCover;

        public int baseHealthPoints;
        public float baseDamageMultiplier; //Automatically does casting
        public bool isDead;

        public bool isHeadedDownwards; //Used for direction specific move modifiers.

        public static readonly Random moveWeightsRandomizer = new Random();

        //can be null
        private Grunt shootingAt;

        public Grunt(bool isHeadedDownwards) {
            baseMorale = 60;
            baseCourage = 40;
            baseCover = 10;

            baseHealthPoints = 20000;
            healthPoints = baseHealthPoints;
            baseDamageMultiplier = 5f;
            isDead = false;

            this.isHeadedDownwards = isHeadedDownwards;
        }

        //things done at the end of a turn
        public void processEndTurn(TileBattleModifiers tileBattleMods) {
            calcStats(tileBattleMods);
        }

        //things done at the start of a turn
        public void processStartTurn() {
            tempPercentMoraleModifier = 0;
            tempPercentCourageModifier = 0;
            tempPercentCoverModifier = 0;

            tempMoveWeights = new int[9];
        }

        // Gets the precent values of Morale, Courage, and Cover and
        //   calculates the stats of the grunt unit.
        public void calcStats(TileBattleModifiers tileBattleMods) {
            percentMorale = baseMorale + tileBattleMods.percentMoraleModifier;
            percentCourage = baseCourage + tileBattleMods.percentCourageModifier;
            percentCover = baseCover + tileBattleMods.percentCoverModifier;

            percentMorale += tempPercentMoraleModifier;
            percentCourage += tempPercentCourageModifier;
            percentCover += tempPercentCoverModifier;

            //Signals No Furter Modifiers to Cover
            if (percentCover > 100) percentCover = 100;
            if (percentCover < 0) percentCover = 0;

            //Troops in high cover get a tiered morale boost
            if (percentCover > 20) percentMorale += 10;
            if (percentCover > 50) percentMorale += 15;

            //units can lose up to 50 morale for being weak
            percentMorale -= (int) (50 * ((double) baseHealthPoints - healthPoints) / baseHealthPoints);

            //units can gain up to 50 courage for being weak
            percentCourage += (int) (50 * ((double) baseHealthPoints - healthPoints) / baseHealthPoints);


            //Signals No Further Modifiers to Morale or Courage
            if (percentMorale > 100) percentMorale = 100;
            if (percentMorale < 0) percentMorale = 0;
            if (percentCourage > 100) percentCourage = 100;
            if (percentCourage < 0) percentCourage = 0;

        }

        public void addTempMorale(int percentMorale) {
            tempPercentMoraleModifier += percentMorale;
        }

        public void addTempCourage(int percentCourage) {
            tempPercentCourageModifier += percentCourage;
        }

        public void addTempCover(int percentCover) {
            tempPercentCoverModifier += percentCover;
        }

        //only called in debugging, grunts move like a king in chess
        private readonly string[] movecodex = {"Still", "UpLeft", "Up", "UpRight", "Right", "DownRight", "Down", "DownLeft", "Left"};
        //Morale and orders effect tempMoveWeights before each grunt moves. Chosen randomly from probabilities of each direction.
        public int[] tempMoveWeights = new int[9];

        public void recalculateStats(TileBattleModifiers tileBattleMods) {
            calcStats(tileBattleMods);
        }

        public int[] moveWeights;
        //tells its Army which direction it has chosen to move between 0 and 8 inclusive. 0 indicates no move
        public int move(int row, int col, int maxRow, int maxCol) {
            moveWeights = new int[9];

            //Higher courage makes units more likely to move.
            moveWeights[0] = 20000;
            moveWeights[0] -= 10000 * percentCourage / 100;
            //Weak units get movement penalties
            moveWeights[0] -= 5000 * (baseHealthPoints - healthPoints) / baseHealthPoints;

            for (int index = 1; index < moveWeights.Length; index++) {
                moveWeights[index] = 30 * percentCourage + 1;
            }

            int moraleScaleFactor = 3;
            //higher morale makes units more likely to advance and lower morale makes units more likely to retreat
            if (isHeadedDownwards) {
                moveWeights[1] -= moraleScaleFactor * (percentMorale - 30);
                moveWeights[2] -= moraleScaleFactor * (percentMorale - 30);
                moveWeights[3] -= moraleScaleFactor * (percentMorale - 30);

                moveWeights[5] += moraleScaleFactor * (percentMorale - 30);
                moveWeights[6] += moraleScaleFactor * (percentMorale - 30);
                moveWeights[7] += moraleScaleFactor * (percentMorale - 30);
            }
            else {
                moveWeights[1] += moraleScaleFactor * (percentMorale - 30);
                moveWeights[2] += moraleScaleFactor * (percentMorale - 30);
                moveWeights[3] += moraleScaleFactor * (percentMorale - 30);

                moveWeights[5] -= moraleScaleFactor * (percentMorale - 30);
                moveWeights[6] -= moraleScaleFactor * (percentMorale - 30);
                moveWeights[7] -= moraleScaleFactor * (percentMorale - 30);
            }


            for (int index = 0; index < moveWeights.Length; index++) {
                moveWeights[index] += tempMoveWeights[index];
                if (moveWeights[index] < 0) moveWeights[index] = 0;
            }

            if (row == 0) {
//                moveWeights[0] -= moveWeights[1] + moveWeights[2] + moveWeights[3];
                moveWeights[1] = 0;
                moveWeights[2] = 0;
                moveWeights[3] = 0;
            }
            else if (row == maxRow - 1) {
//                moveWeights[0] -= moveWeights[5] + moveWeights[6] + moveWeights[7];
                moveWeights[5] = 0;
                moveWeights[6] = 0;
                moveWeights[7] = 0;
            }
            if (col == 0) {
//                moveWeights[0] -= moveWeights[1] + moveWeights[7] + moveWeights[8];
                moveWeights[1] = 0;
                moveWeights[7] = 0;
                moveWeights[8] = 0;
            }
            else if (col == maxCol - 1) {
//                moveWeights[0] -= moveWeights[3] + moveWeights[4] + moveWeights[5];
                moveWeights[3] = 0;
                moveWeights[4] = 0;
                moveWeights[5] = 0;
            }

            //I would be in disbelief if someone manages to stack enough modifiers to make a /0 error
            //that would indicate running at an edge with a weight of like 10000
            //however, it is still theoretically possible, so this next line addresses it.
            if (moveWeights[0] <= 0) moveWeights[0] = 1;
            for (int index = 1; index < moveWeights.Length; index++) {
                if (moveWeights[index] < 0) moveWeights[index] = 0;
            }


            //ok if not exact, we are going for speed
            double[] probsFromWeights = new double[moveWeights.Length];
            double totalWeight = 0;
            for (int index = 0; index < moveWeights.Length; index++) {
                totalWeight += moveWeights[index];
            }
            for (int index = 0; index < moveWeights.Length; index++) {
                probsFromWeights[index] = moveWeights[index] / totalWeight;
            }

            double moveDecision = moveWeightsRandomizer.NextDouble();
            for (int i = 0; i < probsFromWeights.Length; i++) {
                moveDecision -= probsFromWeights[i];
                if (moveDecision < 0) {
                    return i;
                }
            }
            //If we messed up majorly and can't figure out which way to go, just stand still.
            return 0;
        }

        public void dealDamage() {
            if (shootingAt == null) return;
            shootingAt.takeDamage(baseDamageMultiplier * percentCourage * healthPoints / baseHealthPoints);
        }

        public void takeDamage(float damageToTake) {
            int damageToTakeInt = (int) (damageToTake * (-percentMorale / 100f + 2f) * percentCover / 100f);
            //multiply by ((-morale/100) + 2) and mutiply by cover/100
            //The morale equation gives
            healthPoints -= damageToTakeInt;
            if (healthPoints < 0) {
                isDead = true;
                healthPoints = 0;
            }

        }

        public void setShootingAt(Grunt grunt) {
            shootingAt = grunt;
        }
    }
}