using System;
using System.Collections;
using System.Collections.Generic;
using PowerPackage;
using GruntPackage;

namespace LieutenantPackage {

    public class LieutenantsList {
        public static readonly LieutenantTemplate jeremy = new LieutenantTemplate("Jeremy", new Power[] {
            new Power(PowersList.reposition),
            new Power(PowersList.moveOrder),
            new Power(PowersList.inspire),
            new Power(PowersList.charge),
            new Power(PowersList.armyCrawl)
            },
            "My name is Jeremy. I am a jack of all trades, like glue that holds your army together.",
            "jeremyFront.png",
            "jeremyBack.png"
            );

        public static readonly LieutenantTemplate daniel = new LieutenantTemplate("Daniel", new Power[] {
            new Power(PowersList.tacticalRun),
            new Power(PowersList.moveOrder),
            new Power(PowersList.battlecry),
            new Power(PowersList.triumphantAssault),
            new Power(PowersList.unconvincingCharge)
            },
            "I'm Daniel. My men fear nothing but me. Run towards danger, or go home",
            "jeremyFront.png",
            "jeremyBack.png"
            );

        public static readonly LieutenantTemplate sergi = new LieutenantTemplate("Sergi", new Power[] {
            new Power(PowersList.steadyWalk),
            new Power(PowersList.moveOrder),
            new Power(PowersList.triumphantAssault),
            new Power(PowersList.allIn)
            },
            "They call me Sergi. I sit in corner. I plan every move. Then I Strike!",
            "jeremyFront.png",
            "jeremyBack.png"
            );

        public static readonly LieutenantTemplate rodger = new LieutenantTemplate("Rodger", new Power[] {
            new Power(PowersList.reposition),
            new Power(PowersList.moveOrder),
            new Power(PowersList.inspire),
            },
            "Hi There! My name is Rodger. I am going to help you learn how to command an army.",
            "rodgerFront.png",
            "rodgerBack.png"
            );

    }

    public class LieutenantTemplate {

        public Power[] powers;
        public string description;
        public string frontImage;
        public string backImage;
        public string name;

        public LieutenantTemplate(string name, Power[] powers, string description, string frontImage, string backImage) {
            this.name = name;
            this.powers = powers;
            this.description = description;
            this.frontImage = frontImage;
            this.backImage = backImage;
        }

    }

    public class Lieutenant {

        //a slot == to "" means no power
        public Power[] powers;

        public int row;
        public int col;
        private bool hasMovedThisTurn;

        public bool belongsToPlayer;

        public string name;
        public string description;
        public string frontImage;
        public string backImage;

        public Lieutenant(LieutenantTemplate template, int row, int col, bool belongsToPlayer) {
            this.row = row;
            this.col = col;
            
            this.name = template.name;
            this.description = template.description;
            this.frontImage = template.frontImage;
            this.backImage = template.backImage;

            this.belongsToPlayer = belongsToPlayer;

            this.powers = new Power[template.powers.GetLength(0)];
            for (int powerIndex = 0; powerIndex < template.powers.GetLength(0); powerIndex++) {
                this.powers[powerIndex] = new Power(template.powers[powerIndex]);
            }
        }

        public bool isOnCooldown(int slot) {
            if (slot >= powers.Length) 
                throw new Exception("Can not check cooldown for a power in a slot that doesn't exist. Slot: "
                    + slot + "; Length of powers array: " + powers.Length);
            return powers[slot].cooldownLeft != 0;
        }

        public void turnTick() {
            foreach (Power power in powers) {
                power.turnTick();
            }
        }

        public bool canMove() {
            return !hasMovedThisTurn;
        }

        public void move(int row, int col) {
            this.row = row;
            this.col = col;
        }

        public string[] fetchPowersHeaders() {
            string[] headers = new string[powers.Length];
            for (int index = 0; index < powers.Length; index++) {
                headers[index] = powers[index].displayName;
            }
            return headers;
        }

        public string[] fetchPowersProperties() {
            string[] properties = new string[powers.Length];
            for (int index = 0; index < powers.Length; index++) {
                properties[index] = "Cooldown: " + powers[index].cooldownTime + ", Area Of Effect : " + powers[index].areaOfEffect;
            }
            return properties;
        }

        public string[] fetchPowersDescriptions() {
            string[] descriptions = new string[powers.Length];
            for (int index = 0; index < powers.Length; index++) {
                descriptions[index] = powers[index].description;
            }
            return descriptions;
        }

        //returns an array of paths to each power's image in the order that they have been placed in powersList
        public string[] fetchPowersPaths() {
            string[] powersPaths = new string[powers.Length];
            for (int powersIndex = 0; powersIndex < powers.Length; powersIndex++) {
                switch (powers[powersIndex].cooldownLeft) {
                    case 0:
                        powersPaths[powersIndex] = powers[powersIndex].imagePath;
                        break;
                    case 1:
                        powersPaths[powersIndex] = "PowerIcons/Cooldown_1_Icon";
                        break;
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                        powersPaths[powersIndex] = "PowerIcons/Cooldown_2_Icon";
                        break;
                    default:
                        throw new Exception("Invalid cooldown time left: " + powers[powersIndex].cooldownLeft);
                }
            }
            return powersPaths;
        }

        public void activatePower(int slot, List<Grunt>[,] gruntsList, int[] specialArgs) {
            if (String.Equals(powers[slot].name, "")) return;
            if (powers[slot].cooldownLeft != 0) throw new Exception("Can Not Activate A Power That Is On Cooldown! Slot: " + slot);

            powers[slot].activate();

            switch (powers[slot].name) {
                case "reposition":
                    break;
                case "moveOrder":
                    if (specialArgs.Length == 0 || specialArgs == null) {
                        throw new Exception("Can not call a move order without Special argument. specialArgs[0] is an integer 0-8 indicating the direction of the move order.");
                        //if index out of bounds from not being 0-8, let crash
                    }
                    if (specialArgs[0] < 0 || specialArgs[0] > 8) {
                        throw new Exception("Invalid move order direction: " + specialArgs[0]);
                    } //ADD DIRECTION SOMEHOW?

                    int startRow = row - powers[slot].areaOfEffect;
                    if (startRow < 0) startRow = 0;

                    int endRow = row + powers[slot].areaOfEffect;
                    if (endRow > gruntsList.GetLength(0)) endRow = gruntsList.GetLength(0);

                    int startCol = col - powers[slot].areaOfEffect;
                    if (startCol < 0) startRow = 0;

                    int endCol = col + powers[slot].areaOfEffect;
                    if (endCol > gruntsList.GetLength(1)) endCol = gruntsList.GetLength(1);

                    for (int rowi = startRow; rowi < endRow; rowi++) {
                        for (int coli = startCol; coli < endCol; coli++) {
                            if (gruntsList[rowi, coli] != null) {
                                foreach (Grunt grunt in gruntsList[rowi, coli]) {
                                    grunt.tempMoveWeights[specialArgs[0]] += (int) grunt.percentMorale * 1500;
                                }
                            }
                        }
                    }
                    break;
                case "strongInspire":
                    flatPower(35, 35, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "inspire":
                    flatPower(20, 20, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "weakInspire":
                    flatPower(10, 10, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "feebleInspire":
                    flatPower(5, 5, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "strongCharge":
                    flatPower(0, 65, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "charge":
                    flatPower(0, 50, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "weakCharge":
                    flatPower(0, 35, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "feebleCharge":
                    flatPower(0, 20, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "motivate":
                    flatPower(55, 0, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "weakMotivate":
                    flatPower(35, 0, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "feebleMotivate":
                    flatPower(20, 0, 0, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                case "armyCrawl":
                    flatPower(-10, -80, 40, powers[slot].areaOfEffect, row, col, gruntsList);
                    break;
                default:
                    //let crash upon critical failiure of attempting to activate a power that does not exist.
                    throw new Exception("Invalid name of Power, can not activate power:" + powers[slot].name);
            }
        }

        private void flatPower(int morale, int courage, int cover, int areaOfEffect, int centerRow, int centerCol, List<Grunt>[,] gruntsList) {
            int startRow = centerRow - areaOfEffect;
            if (startRow < 0) startRow = 0;

            int endRow = centerRow + areaOfEffect;
            if (endRow > gruntsList.GetLength(0)) endRow = gruntsList.GetLength(0);

            int startCol = centerCol - areaOfEffect;
            if (startCol < 0) startRow = 0;

            int endCol = centerCol + areaOfEffect;
            if (endCol > gruntsList.GetLength(1)) endCol = gruntsList.GetLength(1);

            for (int rowi = startRow; rowi < endRow; rowi++) {
                for (int coli = startCol; coli < endCol; coli++) {
                    if (gruntsList[rowi, coli] != null) {
                        foreach (Grunt grunt in gruntsList[rowi, coli]) {
                            addFlatModifier(morale, courage, cover, grunt);
                        }
                    }
                }
            }
        }

        private void addFlatModifier(int morale, int courage, int cover, Grunt grunt) {
            grunt.addTempMorale(morale);
            grunt.addTempCourage(courage);
            grunt.addTempCover(cover);
        }
    }
}