using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowersBarCanvasInitializer : MonoBehaviour
{
    [SerializeField] private GameObject panel;

    // Start is called before the first frame update
    void Start()
    {
        GameObject panelInstance = Instantiate(panel, gameObject.transform);
        // panelInstance.transform.parent = gameObject.transform;
    }
}
