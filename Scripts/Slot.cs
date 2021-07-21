using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using System;

using TooltipSystemPackage;

public class Slot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    
    public int SlotIndex;

    private string powerHeader;
    private string powerProperties;
    private string powerDescription;

    private bool isHoveredOver = false;

    // [SerializeField] private GameObject _handler;
    // [System.NonSerialized] public RenderingEngineAndGameClock handler;

    //Runs when this Script is loaded
    void awake() {
        //Loads the used script
        // handler = GameObject.Find("Grid").GetComponent<RenderingEngineAndGameClock>();
        // Debug.Log(handler);
    }

    IEnumerator envokeTooltip(float time) {
        yield return new WaitForSeconds(time);
        // Code to execute after the delay
        if (isHoveredOver) {
            TooltipSystem.show(powerHeader, powerProperties, powerDescription);
        }
    }

    public void OnPointerEnter(PointerEventData eventData) {
        isHoveredOver = true;
        StartCoroutine(envokeTooltip(0.85f));
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHoveredOver = false;
        TooltipSystem.hide();
    }

    public void OnClick() {
        var handler = GameObject.Find("Grid").GetComponent<RenderingEngineAndGameClock>(); 
        if (handler == null) {
            throw new Exception("No handler set on slot: " + SlotIndex);
        }
        handler.uiPowerSlotClicked(SlotIndex);
    }

    public void setTooltipText(string header, string properties, string description) {
        powerHeader = header;
        powerProperties = properties;
        powerDescription = description;
    }
}
