using UnityEngine;
using UnityEngine.UI;

namespace Extensions
{
    public static class UnityObjectsExtensions {

        public static void ChangeLayerRecursively(this GameObject gameObject, string layerName)
        {
            gameObject.layer = LayerMask.NameToLayer(layerName);
            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.ChangeLayerRecursively(layerName);
            }
        }

        public static void ChangeAlphaChannel(this Button button, float alpha)
        {
            Color color = button.GetComponent<Image>().color;
            color.a = alpha;
            button.GetComponent<Image>().color = color;
        }

    }
}
