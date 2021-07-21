using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lieutenant_Move_Script : MonoBehaviour
{
    public int size;

    public GameObject lieutanant;

    public void setLieutenant(GameObject lieutanant) {
        this.lieutanant = lieutanant;
        this.gameObject.transform.localPosition = lieutanant.gameObject.transform.position;
        }

    public void setSize(int size) {
        this.size = size;
        this.gameObject.transform.localScale = new Vector3(size, size, size);
    }
}
