using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ClientFruitonFactory {

    public const string HEALTH = "Health";
    public const string DAMAGE = "Damage";
    public const string TAGS = "Tags";

    public static IEnumerable<Fruiton> CreateAllKernelFruitons()
    {
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        var result = new List<Fruiton>();
        foreach (int key in fruitonDatabase.fruitonDb._keys)
        {
            // This check is here because Haxe IntMap may also contain invalid key-value pairs, which can be recognised by the fact that their key is 0. 
            if (key == 0) continue;
            result.Add(FruitonFactory.makeFruiton(key, fruitonDatabase));

        }
        return result;
    }

    public static IEnumerable<GameObject> CreateClientFruitons()
    {
        Debug.Log("Loading ALL fruitons.");
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        var result = new List<GameObject>();
        foreach (int key in fruitonDatabase.fruitonDb._keys)
        {
            // This check is here because Haxe IntMap may also contain invalid key-value pairs, which can be recognised by the fact that their key is 0. 
            if (key == 0) continue;
            Fruiton kernelFruiton = FruitonFactory.makeFruiton(key, fruitonDatabase);
            var newFruiton = new GameObject();
            newFruiton.AddComponent<ClientFruiton>().KernelFruiton = kernelFruiton;
            result.Add(newFruiton);
        }
        return result;
    }

    public static IEnumerable<GameObject> CreateClientFruitonTeam(RepeatedField<int> teamIDs)
    {
        var result = new List<GameObject>();
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        foreach (int id in teamIDs)
        {
            Fruiton kernelFruiton = FruitonFactory.makeFruiton(id, fruitonDatabase);
            var newObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Battle/" + kernelFruiton.model, typeof(GameObject))) as GameObject;
            newObject.AddComponent<ClientFruiton>().KernelFruiton = kernelFruiton;
            var tags = UnityEngine.Object.Instantiate(Resources.Load("Models/Auxiliary/Tags", typeof(GameObject))) as GameObject;
            tags.name = TAGS;
            tags.transform.position = newObject.transform.position + new Vector3(0, 1, -1.33f);
            tags.transform.Rotate(new Vector3(45, 0, 0));
            tags.transform.parent = newObject.transform;
            foreach (Transform child in tags.transform)
            {
                switch (child.name)
                {
                    case HEALTH:
                        {
                            child.GetComponentInChildren<TextMesh>().text = kernelFruiton.hp.ToString();
                        } break;
                    case DAMAGE:
                        {
                            child.GetComponentInChildren<TextMesh>().text =  ((AttackGenerator)kernelFruiton.attackGenerators[0]).damage.ToString();
                        } break;
                }
            }
            result.Add(newObject);
        }
        return result;
    }

}
