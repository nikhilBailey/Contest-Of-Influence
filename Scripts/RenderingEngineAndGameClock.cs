using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using Unity.UI;

using BattleProcessorPackage;
using TerrainGeneratorPackage;
using ArmyPackage;
using LieutenantPackage;

public class RenderingEngineAndGameClock : MonoBehaviour {

    //Uses reference Schematics to populate the landscape
    private BattleProcessor gameEngine = new BattleProcessor(TerrainGenerator.lanscapeGenNums);
    
    private static Tile[] tilesForConversion;
    public Tilemap landscape;
    public Tilemap gruntsMap;

    [SerializeField] private GameObject _healthBarCanvas;
    private List<GameObject> healthBarList = new List<GameObject>();
    private static readonly float healthBarOfsetX = 0.5f;
    private static readonly float healthBarOfsetY = 0.8f;

    //All Lieutenants are intances of the reference Lieutenant.
    //Each clone needs a rescale (1,1,1) and a new position.
    [SerializeField] private GameObject _referenceLieutenant;
    private List<GameObject> lieutenantObjectList = new List<GameObject>();

    [SerializeField] private GameObject _lieutenantPowersBarPanel;
    private SlotManager lieutenantPowersBarSlotManager;

    //awake is called after the code compiles, as unity is launching
    void Awake() {
        //Fetches the required Script
        lieutenantPowersBarSlotManager = _lieutenantPowersBarPanel.GetComponent<SlotManager>();

        generateNewLandscape();
        fetchLandscapeTiles();
        renderLandscapeTiles();
    }

    // Start is called before the first frame update
    void Start() {

        //Initialized the location of the first healthbar which will soon be cloned
        // _healthBarCanvas.transform.localPosition = new Vector3(0, 0, 0);
        // _healthBarCanvas.transform.localScale = new Vector3(0, 0, 0);

        //Initializes the reference Lieutenant
        // _referenceLieutenant.transform.localScale = new Vector3(0, 0, 0);
        _referenceLieutenant.GetComponent<LieutenantScript>().setBaseColor();

        //renders the tiles by getting loading the game objects,
        fetchLandscapeTiles();
        //then drawing them
        renderLandscapeTiles();
        //fetches the tiles of the different grunts
        fetchGruntTiles();
        //Forces the grunts to calculate the stats of the tile they are standing on,
        //necessary to initialize health bars
        gameEngine.forceCalcStats();
        //renders the tiles of the grunts
        renderGrunts();
        //renders the Lieutenant GameObjects
        instantiateLieutenants();
        //restarts the game clock
        timer = timeBetweenTicks;
    }

    // Update is called once per frame
    //This is the game clock.
    public static float timeBetweenTicks = 0.8f;
    private float timer;
    private float uselessValueBecauseUnityIsStupid;
    //This is the Script of the lieutenant that we have selected
    private LieutenantScript currentlySelectedLieutenantScript;
    private GameObject currentlySelectedLieutenant;

    public void Update() {
        
        if (!gameEngine.isOnTurn) {
            timer -= Time.deltaTime;
            if (timer < 0) {
                timer = timeBetweenTicks;
                
                //Ticks all the grunts and forces them to calculate their statistics
                gameEngine.processTick();

                //renders the Grunt Tiles in the Tilemap
                renderGrunts();
                //renders the Lieutenant GameObjects
                renderLieutenants();
                //renders the new ui
                if (gameEngine.isOnTurn && currentlySelectedLieutenantScript != null) {
                    renderLieutenantPowersBar(currentlySelectedLieutenantScript.getLieutenant());
                }
            }
        }
        else {
            if (Input.GetKey(KeyCode.Space)) {
                if (isPartwayThroughMoveOrder) {
                    Debug.Log("Can't end turn partway through move order");
                }
                else if (isPartwayThroughReposition) {
                    Debug.Log("Can't end turn partway through reposition");
                }
                else {
                    gameEngine.endTurn();
                    print("TURNENDED!!!/n" + "TURNENDED!!!/n" + "TURNENDED!!!");
                    //Resets Time.deltaTime so that the timer wasn't timing during the turn
                    uselessValueBecauseUnityIsStupid = Time.deltaTime;
                }
            }
        }
    }

    public void lieutenantClickedOn(GameObject lieutenantGameObject) {
        if (isPartwayThroughMoveOrder) {
            Debug.Log("Can't switch Lieutenant while partway through move orders");
        }
        else if (isPartwayThroughReposition) {
            Debug.Log("Can't switch Lieutenant while partway through repositions");
        }
        else {
            currentlySelectedLieutenant = lieutenantGameObject;
            currentlySelectedLieutenantScript = lieutenantGameObject.GetComponent<LieutenantScript>();
            jumpTo(currentlySelectedLieutenant);
            renderLieutenantPowersBar(currentlySelectedLieutenantScript.getLieutenant());
        }
    }

    //Generates integer numbers for a the landscape to be rendered later
    private void generateNewLandscape() {
        //tries to generate a new landscape until one works
        TerrainGenerator.generateLandscapeGenNumsFromWeights();
        int somethingWentWrongCount = 0;
        while (!TerrainGenerator.populateEmptiesLandscapeGenNums()) {
            somethingWentWrongCount++;
            if (somethingWentWrongCount >= 4) throw new Exception("Error Populating Seed" + TerrainGenerator.seed);
            TerrainGenerator.generateLandscapeGenNumsFromWeights();
        }
    }


    //Populates the slots with image files found from image paths in the lieutenant object
    private void renderLieutenantPowersBar(Lieutenant lieutanant) {
        string[] imagePaths = lieutanant.fetchPowersPaths();
        string[] powerHeaders = lieutanant.fetchPowersHeaders();
        string[] powerProperties = lieutanant.fetchPowersProperties();
        string[] powerDescriptions = lieutanant.fetchPowersDescriptions();
        lieutenantPowersBarSlotManager.renderNewLieutenant(imagePaths, powerHeaders, powerProperties, powerDescriptions);
    }

    //looks at a specific gameObject
    private void jumpTo(GameObject gameObject) {
        Vector3 LookAtPoint = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 10);
            Camera.main.transform.position = LookAtPoint;
    }

    [SerializeField] private GameObject _moveOrderArrow;
    private GameObject moveOrderArrowInstance;
    private bool isPartwayThroughMoveOrder;

    [SerializeField] private GameObject _repositionBox;
    private GameObject repositionBoxInstance;
    private bool isPartwayThroughReposition;

    public void reposition(int row, int col) {

        Army army = currentlySelectedLieutenantScript.getArmy();

        if (army.containsLieutenantsAtSpot(row, col)) {
            Debug.Log("Can't have two Lieutenants at the same spot!");
            return;
        }
        
        if (!isPartwayThroughReposition) throw new Exception("Can't finish executing a reposition if not partway through one");

        currentlySelectedLieutenantScript.setPosition(row, col);
        currentlySelectedLieutenantScript.getLieutenant().activatePower(0, null, null);

        Destroy(repositionBoxInstance);
        isPartwayThroughReposition = false;
        renderLieutenantPowersBar(currentlySelectedLieutenantScript.getLieutenant());
        
        return;
    }

    //Called by the move order arrow when it is clicked
    public void moveOrder(int direction) {
        if (!isPartwayThroughMoveOrder) throw new Exception("Can't finish executing a move order if not partway through one");
        
        Lieutenant lieutenant = currentlySelectedLieutenantScript.getLieutenant();
        Army army = currentlySelectedLieutenantScript.getArmy();
                
        currentlySelectedLieutenantScript.getLieutenant().activatePower(1, army.gruntMatrix, new int[] {direction});
        Destroy(moveOrderArrowInstance);

        isPartwayThroughMoveOrder = false;
        renderLieutenantPowersBar(lieutenant);
        return;
    }

    //Called by clicking a power slot. currentlySelectedLieutenantScript is the lieutenant that is currently selected.
    public void uiPowerSlotClicked(int indexOfSlot) {
        if (!gameEngine.isOnTurn || isPartwayThroughMoveOrder || isPartwayThroughReposition) return;

        Lieutenant lieutenant = currentlySelectedLieutenantScript.getLieutenant();
        Army army = currentlySelectedLieutenantScript.getArmy();

        if (lieutenant.isOnCooldown(indexOfSlot)) {
            Debug.Log("Power is on cooldown :)");
            return;
        }
        else if (indexOfSlot == 0) {
            //REPOSITION!!!
            Lieutenant target = currentlySelectedLieutenantScript.getLieutenant();

            repositionBoxInstance = Instantiate(_repositionBox);
            repositionBoxInstance.GetComponent<MoveLieutenantBox>().setSize(target.powers[0].areaOfEffect);
            repositionBoxInstance.GetComponent<MoveLieutenantBox>().setPosition(target.row, target.col);
            repositionBoxInstance.GetComponent<MoveLieutenantBox>()._handler = this.gameObject.GetComponent<RenderingEngineAndGameClock>();

            isPartwayThroughReposition = true;
            return;
        }
        else if (indexOfSlot == 1) {
            //MOVE ORDER!!!
            
            moveOrderArrowInstance = Instantiate(_moveOrderArrow, currentlySelectedLieutenant.transform);
            moveOrderArrowInstance.transform.position = currentlySelectedLieutenant.transform.position;
            moveOrderArrowInstance.GetComponent<MoveOrderArrowScript>()._handler = this.gameObject;

            isPartwayThroughMoveOrder = true;
            return;
        }
        else {
            //Changes the state of the game
            lieutenant.activatePower(indexOfSlot, army.gruntMatrix, null);
            
            //Forces the grunts to calculate new game state, then renders their new healthbars
            army.forceCalcStats(gameEngine.terrainGenArray);
            renderGrunts();
            
            //Shows that the power is now on cooldown.
            renderLieutenantPowersBar(lieutenant);
            return;
        }
    }

    //when there is multiple grunts at one spot, Armies zip them in a GruntsAtSpotDataContainer. 
    //A GruntRenderCount uses its container (data) as a running total by adding outside containters to itself using the data.num += ... ect.
    //It also keeps track of the faction of a stack of grunts in case grunts have different factions.
    public class GruntRenderCount {
        public int faction;
        public GruntsAtSpotDataContainer data;

        public GruntRenderCount() {
            faction = -1;
            data = new GruntsAtSpotDataContainer(0, 0, 0, 0, 0, 0, 0);
        }
    }

    //this is a matrix of data objects showing what image to render at each spot and what should be in the healthbar.
    public GruntRenderCount[,] gruntRenderMatrix = new GruntRenderCount[TerrainGenerator.lanscapeGenNums.GetLength(0), TerrainGenerator.lanscapeGenNums.GetLength(1)];
    //The name of each tile to load, and the Tile once loaded. The row is the faction and the column is the number of grunts at that spot.
    public static string[,] gruntTileCodex = {{"empty", "oneRedBack", "twoRedBack", "threeRedBack"},{"empty", "oneBlueBack", "twoBlueBack", "threeBlueBack"}};
    public static Tile[,] gruntTileSet = new Tile[gruntTileCodex.GetLength(0), gruntTileCodex.GetLength(1)];

    private void fetchGruntTiles() {
        for (int row = 0; row < gruntTileCodex.GetLength(0); row++) {
            for (int col = 0; col < gruntTileCodex.GetLength(1); col++) {
                gruntTileSet[row, col] = getTileByName(gruntTileCodex[row, col]);
            }
        }
    }

    private void renderGrunts() {
        //Clears the tilemap
        gruntsMap.ClearAllTiles();
        foreach (GameObject healthBar in healthBarList) {
            Destroy(healthBar);
        }
        healthBarList.Clear();

        //Resets the counts to render
        for (int row = 0; row < gruntRenderMatrix.GetLength(0); row++) {
            for (int col = 0; col < gruntRenderMatrix.GetLength(1); col++) {
                gruntRenderMatrix[row, col] = new GruntRenderCount();
            }
        }

        //Counts each grunt and stores it's faction
        GruntRenderCount grc;
        for (int row = 0; row < gruntRenderMatrix.GetLength(0); row++) {
            for (int col = 0; col < gruntRenderMatrix.GetLength(1); col++) {
                foreach (Army army in gameEngine.listOfArmies) {
                    GruntsAtSpotDataContainer gruntsData = army.numGruntsAtSpot(TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col));

                    if (gruntsData != null) {
                        grc = gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)];
                        //Uses mutate (reference Schematics)

                        if (grc.faction == -1) {
                            grc.faction = army.faction;
                        }
                        if (grc.faction == army.faction) {//Like a switch statement without a break, there is runnoff here.

                            //Whether or not we changed the faction, we still have to add the stats that will display on the healthBar.
                            grc.data.num += gruntsData.num;

                            grc.data.totalMorale += gruntsData.totalMorale;
                            grc.data.netMorale += gruntsData.netMorale;
                            grc.data.totalCourage += gruntsData.totalCourage;
                            grc.data.netCourage += gruntsData.netCourage;
                            grc.data.totalHealtPoints += gruntsData.totalHealtPoints;
                            grc.data.netHealthPoints += gruntsData.netHealthPoints;
                        }
                        else {
                            //If there is multiple grunts on the same tile, they will fight to the death
                            throw new Exception("Grunts did not properly fight to the death. Row: " + row + " Col: " + col);
                        }
                    }
                }
            }
        }

        //Assignes the correct Tile to each stack of grunts using a the matrix of data needed to render gruntRenderMatrix
        for (int row = 0; row < gruntRenderMatrix.GetLength(0); row++) {
            for (int col = 0; col < gruntRenderMatrix.GetLength(1); col++) {

                
                //Only proceeds if we are certain that there are grunts here
                if (gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].faction != -1 && gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.num != 0) {
                    //fetches the type of tile to display
                    int tileType = gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.num;
                    //we don't yet have support for more than 3 grunts on a stack
                    if (tileType > 3) tileType = 3;

                    //places the fetched tile in the grid using the set of actual Tile objects
                    Vector3Int offset = new Vector3Int(col + TerrainGenerator.startX, row + TerrainGenerator.startY, 0);
                    gruntsMap.SetTile(offset, gruntTileSet[gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].faction, tileType]);

                    //Creates a healthBar for our new stack of grunts
                    GameObject healthBar = initializeNewHealthBar(col + TerrainGenerator.startX, row + TerrainGenerator.startY);
                    healthBarList.Add(healthBar);

                    //Fills in the data for our new help bar by accessing it's script
                    HealthBarScript healthBarObject = healthBar.transform.GetChild(0).GetComponent<HealthBarScript>();
                    healthBarObject.setMorale((float) gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.netMorale / gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.totalMorale);
                    healthBarObject.setCourage((float) gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.netCourage / gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.totalCourage);
                    healthBarObject.setHealth((float) gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.netHealthPoints / gruntRenderMatrix[TerrainGenerator.invertRow(row), TerrainGenerator.invertCol(col)].data.totalHealtPoints);
                }
            }
        }
    }


    //Renders the inital location of the lieutanants. 
    //Unlike grunts which are Objects used to paint a grid, Lieutenants are GameObjects containing Lieutenant Objects in each script
    private void instantiateLieutenants() {
        foreach (Army army in gameEngine.listOfArmies) {
            foreach (Lieutenant lieutenant in army.lieutenantsList) {
                
                //Created the lieutenant in game, moves it to the correct location, and rescales it to the proper size.
                GameObject lieutenantObject = Instantiate(_referenceLieutenant);
                // lieutenantObject.transform.localScale = new Vector3(1, 1, 1);

                //Assigns the correct data to the instantiated object
                //In this case it is a reference to an already existing Lieutenant Object in the game engine, and its army.
                //It will use these references to manipulate the game and to communicate with other objects through
                //    mutation and reference schematics, resolved upon rendering.
                //note: the classes are structured like this to achive simplicity while avoiding circular dependencies.
                lieutenantObject.GetComponent<LieutenantScript>().setLieutenant(lieutenant);
                lieutenantObject.GetComponent<LieutenantScript>().setArmy(army);
                lieutenantObject.GetComponent<LieutenantScript>()._handler = this.gameObject.GetComponent<RenderingEngineAndGameClock>();

                //Places the lieutenant on the correct tile
                LieutenantScript script = lieutenantObject.GetComponent<LieutenantScript>();
                script.refreshPosition();
                // lieutenantObject.transform.localPosition = new Vector3(0,0,0);
                // lieutenantObject.transform.Translate(TerrainGenerator.invertCol(script.getLieutenant().col) + TerrainGenerator.startX + 0.5f,
                //     TerrainGenerator.invertRow(script.getLieutenant().row) + TerrainGenerator.startY + 0.5f, 0);

                //Stores the lieutenant in a place where it can be rendered and ticked from
                lieutenantObjectList.Add(lieutenantObject);
            }
        }
    }

    private void renderLieutenants() {
        foreach (GameObject lieutenantObject in lieutenantObjectList) {
            LieutenantScript script = lieutenantObject.GetComponent<LieutenantScript>();
            
            lieutenantObject.transform.localPosition = new Vector3(0,0,0);
            //Note: the entire gameboard is mirrored over the x and y axis at this step and in other places
            lieutenantObject.transform.Translate(TerrainGenerator.invertCol(script.getLieutenant().col) + TerrainGenerator.startX + 0.5f,
                TerrainGenerator.invertRow(script.getLieutenant().row) + TerrainGenerator.startY + 0.5f, 0);
        }
    }

    private static Tile getTileByName(string name) {
        if (String.Equals(name,"empty")) return null;
        Tile tile = Resources.Load<Tile>("Tiles/" + name);

        if (tile is null) {
            throw new Exception("Unable to find requested Asset in tile folder:" + name);
        }
        return tile;
    }

    //Turns the Tile codex (String names of tiles) into a set of Tile objects that actually exist and can be instantiated
    private static void fetchLandscapeTiles() {
        string[] codex = TerrainGenerator.codex;
        tilesForConversion = new Tile[codex.Length];
        tilesForConversion[0] = null;
        for (int codexIndex = 1; codexIndex < codex.Length; codexIndex++) {
            tilesForConversion[codexIndex] = getTileByName(codex[codexIndex]);
        }
    }

    //Actually renders All Tiles in the Landscape. Only needs to be called when the lanscape changes
    private void renderLandscapeTiles() {
        
        landscape.ClearAllTiles();
        int[,] lanscapeGenNums = TerrainGenerator.lanscapeGenNums;
        
        if (tilesForConversion == null) throw new Exception("You must call fetchLandscapeTiles() first");
        
        for (int row = 0; row < lanscapeGenNums.GetLength(0); row++) {
            for (int col = 0; col < lanscapeGenNums.GetLength(1); col++) {

                //Note: the entire gameboard is mirrored over the x and y axis at this step and in other places
                Vector3Int offset = new Vector3Int(col + TerrainGenerator.startX, row + TerrainGenerator.startY, 0);
                landscape.SetTile(offset, tilesForConversion[lanscapeGenNums[TerrainGenerator.invertRow(row),
                    TerrainGenerator.invertCol(col)]]);
            }
        }
    }

    //needed to instantinate and rescale a new healthbar
    private GameObject initializeNewHealthBar(int difx, int dify) {
        GameObject newHealtBar = Instantiate(_healthBarCanvas, new Vector3(difx + healthBarOfsetX, dify + healthBarOfsetY, 0), new Quaternion(0, 0, 0, 1));
        if (newHealtBar == null) throw new Exception("Attempetd tp instantiate a new HealthBar, but it returned null! X: " + difx + ", Y: " + dify);
        newHealtBar.transform.localScale = new Vector3(1, 1, 1);
        return newHealtBar;
    }
}
