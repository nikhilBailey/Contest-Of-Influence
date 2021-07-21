using System.Collections;
using System.Collections.Generic;
using System;

using UnityEngine;

using LieutenantPackage;
using ArmyPackage;
using TerrainGeneratorPackage;
using TooltipSystemPackage;

public class LieutenantScript : MonoBehaviour
{
    //Data from engine on this, is and can be mutated
    private Lieutenant lieutenant;
    //The army this belongs to
    private Army army;

    public static readonly Color lieutenantBaseColor = new Color(20, 20, 255);
    public static readonly Color lieutenantHighlightColor = new Color(255, 255, 255);

    public RenderingEngineAndGameClock _handler;

    private bool isHoveredOver = false;

    IEnumerator envokeTooltip(float time) {
        yield return new WaitForSeconds(time);
        // Code to execute after the delay
        if (isHoveredOver) {
            TooltipSystem.show(lieutenant.name, "", "");
        }
    }

    void OnMouseEnter() {
        if (!lieutenant.belongsToPlayer) return;
        isHoveredOver = true;
        setHighlightColor();
        StartCoroutine(envokeTooltip(0.25f));
    }

    void OnMouseExit() {
        if (!lieutenant.belongsToPlayer) return;
        isHoveredOver = false;
        setBaseColor();
        TooltipSystem.hide();
    }

    void OnMouseUp() {
        if (!lieutenant.belongsToPlayer) Debug.Log("Lieutenant Belongs To AI");
        if (!lieutenant.belongsToPlayer) return;
        _handler.lieutenantClickedOn(this.gameObject);
    }

    public void refreshPosition() {
        this.transform.localPosition = new Vector3(0,0,0);
        this.transform.Translate(TerrainGenerator.invertCol(lieutenant.col) + TerrainGenerator.startX + 0.5f,
            TerrainGenerator.invertRow(lieutenant.row) + TerrainGenerator.startY + 0.5f, 0);
    }

    public void setPosition(int row, int col) {
        lieutenant.row = row;
        lieutenant.col = col;
        refreshPosition();
    }

    public bool isOnCooldown(int slot) {
        return lieutenant.isOnCooldown(slot);
    }

    public Lieutenant getLieutenant() {
        if (lieutenant == null) throw new Exception("This Lieutenant never recieved a Lieutenant from the "
            + "game engine. Call (setLieutenant()) before (getLieutenant())");
        return lieutenant;
    }

    public void setLieutenant(Lieutenant lieutenant) {
        this.lieutenant = lieutenant;
    }

    public void setArmy(Army army) {
        this.army = army;
    }

    public Army getArmy() {
        if (army == null) throw new Exception("This Lieutenant never recieved an Army from the "
            + "game engine. Call (setArmy()) before (getArmy())");
        return army;
    }

    public void setBaseColor() {
        GetComponent<SpriteRenderer>().color = lieutenantBaseColor;
    }

    public void setHighlightColor() {
        GetComponent<SpriteRenderer>().color = lieutenantHighlightColor;
    }
}
