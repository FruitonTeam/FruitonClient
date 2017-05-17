using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Salad : MonoBehaviour {

    public Salad(List<string> ids)
    {
        fruitonIDs = ids;
    }

    private List<string> fruitonIDs;
    
    public List<string> FruitonIDs
    {
        get
        {
            return fruitonIDs;
        }
        set
        {
            fruitonIDs = value;
        }
    }
    
}
