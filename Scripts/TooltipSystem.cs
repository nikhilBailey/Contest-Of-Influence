using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TooltipSystemPackage {
    
    public class TooltipSystem : MonoBehaviour {

        private static TooltipSystem current;

        public Tooltip tooltip;

        public RectTransform rectTransform;

        private static bool isHidden;

        void Awake() {
            current = this;
            hide();

            rectTransform = GetComponent<RectTransform>();
        }

        public static void show(string header, string properties, string description) {
            isHidden = false;
            current.Update();
            current.tooltip.setText(header, properties, description);
            current.tooltip.gameObject.SetActive(true);
        }

        public static void hide() {
            isHidden = true;
            current.tooltip.gameObject.SetActive(false);
        }

        // Update is called once per frame
        void Update()
        {
            if (!isHidden) {
                Vector2 position = Input.mousePosition;
                current.tooltip.gameObject.transform.position = new Vector2(position.x, position.y + 80);

                float pivotX = position.x / Screen.width;
                float pivotY = position.y / Screen.height;

                rectTransform.pivot = new Vector2(pivotX, pivotY);
            }        
        }
    }
}