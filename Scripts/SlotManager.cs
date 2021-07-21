using System.Collections;
using System.Collections.Generic;
using System;

using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using LieutenantPackage;

public class SlotManager : MonoBehaviour {

    [SerializeField] private GameObject _slot;
    private List<GameObject> _slots;

    [SerializeField] private GameObject _onClickHandler;
    private RenderingEngineAndGameClock onClickHandlerScript;

    //Runs when this Script is loaded
    void Awake() {
        //Loads the used script
        onClickHandlerScript = _onClickHandler.GetComponent<RenderingEngineAndGameClock>();
        _slots = new List<GameObject>();
    }

    //Creates a new slot
    private void createSlot() {
        GameObject slot = Instantiate(_slot, GameObject.Find("PowersBarCanvas").transform.GetChild(0).transform);
        _slots.Add(slot);
        
        slot.gameObject.GetComponent<Slot>().SlotIndex = _slots.Count - 1;
    
        //sets the reference to the onclick handler, in this case the rendering engine
        // slot.gameObject.GetComponent<Slot>().handler = onClickHandlerScript;
    }

    //Destroys a slot at the specified index and reindexes the remaining slots
    private void destroySlot(int index) {
        if (_slots.ElementAtOrDefault(index) != null)
        {
            Destroy(_slots[index]);
            _slots.Remove(_slots[index]);
            
            for (int checkingIndex = 0; checkingIndex < _slots.Count; checkingIndex++) {
                Slot slot = _slots[checkingIndex].gameObject.GetComponent<Slot>();

                if (slot.SlotIndex != checkingIndex) {
                    slot.SlotIndex = checkingIndex;
                }
            }
        }
    }

    //Changes the number of slots
    private void setNumSlots(int numSlots) {
        _slots.RemoveAll(item => item == null);

        if (numSlots > 8)
            throw new Exception("Can't have more than 8 Slots");
        if (numSlots < 0)
            throw new Exception("Can't have less than 0 Slots");

        while (_slots.Count > numSlots) {
            destroySlot(0);
        }
        while (_slots.Count < numSlots) {
            createSlot();
        }
        if (_slots.Count != numSlots) {
            throw new Exception("This error should not have occurred. Failed to properly create slots.");
        }
    }

    //Changes the number of slots and fills them with images. Images are fetched from the resources folder by path
    public void renderNewLieutenant(string[] imagePathsArray, string[] powerHeaders, string[] powerProperties, string[] powerDescriptions) {
        
        if (imagePathsArray.Length != powerHeaders.Length || imagePathsArray.Length != powerDescriptions.Length 
            || imagePathsArray.Length != powerProperties.Length) {
            throw new Exception("Can not change number of slots if images and powers and descriptions are different lengths");
        }
        setNumSlots(imagePathsArray.Length);
        
        for(int imageIndex = 0; imageIndex < imagePathsArray.Length; imageIndex++) {
            string path = imagePathsArray[imageIndex];
            try {
                SetImage(path, imageIndex);
            }
            catch (Exception ex) {
                Debug.Log("Invalid image path at index: " + imageIndex);
                Debug.Log(ex);
            }

            setTooltipText(powerHeaders[imageIndex], powerProperties[imageIndex], powerDescriptions[imageIndex], imageIndex);
        }
    }

    // private void Update() {
    //     if (Input.GetKeyDown(KeyCode.A))
    //         createSlot();
    //     if (Input.GetKeyDown(KeyCode.S))
    //         destroySlot(0);
    // }

    //Fetches an image from path using resources folder. Then loads the image into the slot at index.
    public void SetImage(string path, int index) {
        Sprite loadingImage = Resources.Load<Sprite>(path);
        if (_slots.ElementAtOrDefault(index) != null && loadingImage != null)
            _slots[index].transform.GetChild(0).GetComponent<Image>().sprite = loadingImage;
        else {
            throw new Exception("Image loaded as null, or there is no slot at this index. Index: " + index + "; loadingImage: " + loadingImage);
        }

    }

    public void setTooltipText(string powerHeader, string powerProperties, string powerDescription, int index) {
        _slots.ElementAtOrDefault(index);
        _slots[index].GetComponent<Slot>().setTooltipText(powerHeader, powerProperties, powerDescription);
    }
}
