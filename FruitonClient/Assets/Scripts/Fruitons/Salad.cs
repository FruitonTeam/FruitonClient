using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Salad {

    private string name;

    public Salad(string name)
    {
        this.name = name;
        fruitonIDs = new List<string>();
    }

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

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public void Add(string fruitonID)
    {
        fruitonIDs.Add(fruitonID);
    }
    
}
