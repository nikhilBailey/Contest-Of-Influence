using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarScript : MonoBehaviour
{
    public Slider moraleSlider;
    public Slider courageSlider;
    public Slider healthSlider;

    public void setMorale(float relativeMorale) {
        moraleSlider.value = relativeMorale;
    }

    public void setCourage(float relativeCourage) {
        courageSlider.value = relativeCourage;
    }

    public void setHealth(float relativeHealth) {
        healthSlider.value = relativeHealth;
    }
}
