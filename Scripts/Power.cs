using System;

namespace PowerPackage {
    public class Power {

        public readonly int cooldownTime;
        public int cooldownLeft;
        public int areaOfEffect;

        public string name;
        public string displayName;
        public string imagePath;
        public string description;

        public Power(int cooldown, int areaOfEffect, string name, string displayName, string imagePath, string description) {
            //powers start off cooldown
            this.cooldownTime = cooldown;
            this.cooldownLeft = 0;
            this.areaOfEffect = areaOfEffect;

            this.name = name;
            this.displayName = displayName;
            this.imagePath = imagePath;
            this.description = description;
        }

        public Power(Power toCopy) {
            this.cooldownTime = toCopy.cooldownTime;
            this.cooldownLeft = 0;
            this.areaOfEffect = toCopy.areaOfEffect;

            this.name = toCopy.name;
            this.displayName = toCopy.displayName;
            this.imagePath = toCopy.imagePath;
            this.description = toCopy.description;
        }

        public void turnTick() {
            if (cooldownLeft > 0) cooldownLeft--;
        }

        public void activate() {
            if (cooldownLeft != 0) throw new Exception("code allowed user to activate Power that is still on cooldown!");
            cooldownLeft = cooldownTime;
        }
    }

    public class PowersList {
        //cooldown, areaOfEffect, Name, DisplayName, imagePath, Description
        public static readonly Power reposition = new Power(1, 2, "reposition", "Repositon!", "PowerIcons/MoveOrderIcon", "Moves me to a new location on the battlefield");
        public static readonly Power tacticalRun = new Power(1, 4, "reposition", "Tactical Run", "PowerIcons/MoveOrderIcon", "(Reposition) Moves me to a new location on the battlefield");
        public static readonly Power steadyWalk = new Power(1, 1, "reposition", "Steady Walk", "PowerIcons/MoveOrderIcon", "(Reposition) Moves me to a new location on the battlefield");
        public static readonly Power moveOrder = new Power(1, 2, "moveOrder", "Move Order!", "PowerIcons/MoveOrderIcon", "Orders nearby units to move in a direction");
        public static readonly Power inspire = new Power(3, 2, "inspire", "Inspire!", "Slot1", "Gives a boost to the morale and courage of nearby troops");
        public static readonly Power charge = new Power(5, 2, "charge", "Charge!", "Slot1", "Gives a large courage boost to nearby troops");
        public static readonly Power unconvincingCharge = new Power(1, 1, "feebleCharge", "Unconvincing Charge", "Slot1", "(Charge--) Gives a large courage boost to nearby troops");
        public static readonly Power armyCrawl = new Power(3, 1, "armyCrawl", "Drop & Crawl!", "Slot1", "Gives a moderate defensive boost at the price of a large courage penalty to troops immediately nearby");
        public static readonly Power battlecry = new Power(5, 4, "weakInspire", "Battlecry", "Slot1", "(Inspire-) Gives a boost to the morale and courage of nearby troops");
        public static readonly Power warDrums = new Power(4, 10, "feebleCharge", "War Drums", "Slot1", "(Charge--) Gives a large courage boost to nearby troops");
        public static readonly Power triumphantAssault = new Power(8, 3, "strongCharge", "Triumphant Assault", "Slot1", "(Charge+) Gives a large courage boost to nearby troops");
        public static readonly Power allIn = new Power(8, 10, "strongInspire", "All In", "Slot1", "(Inspire+) Gives a boost to the morale and courage of nearby troops");
    }
}