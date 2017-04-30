using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[XmlRoot("FruitonsDefs")]
public class Fruitons
{
    public Fruitons()
    {
        FruitonList = new List<Fruiton>();
    }

    [XmlElement("Fruiton")]
    public List<Fruiton> FruitonList { get; set; }

    public override string ToString()
    {
        string result = "";
        foreach(Fruiton fruiton in FruitonList)
        {
            result += fruiton.ToString();
        }
        return result;
    }
}

public class Fruiton {
    public Fruiton() { }

    [XmlAttribute("id")]
    public string Id { get; set; }
    [XmlAttribute("model")]
    public string Model { get; set; }
    [XmlIgnore]
    public GameObject gameobject;

    public override string ToString()
    {
        return "[id=" + Id + ", model=" + Model + "]";
    }

    
}
