using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using TerrainGeneratorPackage;

public class MoveLieutenantBox : MonoBehaviour
{
    private int size;
    private int width;
    public RenderingEngineAndGameClock _handler;

    private int centerRow;
    private int centerCol;

    public void setSize(int size) {
        this.size = size;
        this.width = 2 * size + 1;
        this.gameObject.transform.localScale = new Vector3(width, width, width);
    }

    public void setPosition(int row, int col) {
        this.centerRow = row;
        this.centerCol = col;
        
        //Weird map flipping here
        this.transform.position = new Vector2(TerrainGenerator.numTilesX + TerrainGenerator.startX - col - 0.5f, 
            TerrainGenerator.numTilesY + TerrainGenerator.startY - row - 0.5f);
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetMouseButtonUp(0) && size > 0) {

            Vector3 screenToWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1.0f));
            //Weird map reversal here
            int row = (int) (TerrainGenerator.numTilesY + TerrainGenerator.startY - Math.Floor(screenToWorld.y) - 1);
            int col = (int) (TerrainGenerator.numTilesX + TerrainGenerator.startX - Math.Floor(screenToWorld.x) - 1);
            
            if (isValidSpot(row, col)) {
                _handler.reposition(row, col);
            }
        }
    }

    private bool isValidSpot(int row, int col) {
        return !(row >= TerrainGenerator.numTilesY || row < 0 || col >= TerrainGenerator.numTilesX || col < 0) 
            && (row >= centerRow - size && row <= centerRow + size && col >= centerCol - size && col <= centerCol + size);
    }
}
