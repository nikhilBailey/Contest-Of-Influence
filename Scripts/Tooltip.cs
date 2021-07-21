using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode()]
public class Tooltip : MonoBehaviour
{
    public Text headerField;

    public Text propertiesField;

    public Text contentField;

    public void setText(string header, string properties, string description) {
        headerField.text = header;
        propertiesField.text = properties;
        contentField.text = description;

        propertiesField.enabled = (properties != "");
        contentField.enabled = (description != "");
    }
}
