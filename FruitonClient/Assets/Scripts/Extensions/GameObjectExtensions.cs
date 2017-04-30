using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectExtensions {

    public static void ChangeLayerRecursively(this GameObject gameObject, string layerName)
    {
        gameObject.layer = LayerMask.NameToLayer(layerName);
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.ChangeLayerRecursively(layerName);
        }
    }
}
