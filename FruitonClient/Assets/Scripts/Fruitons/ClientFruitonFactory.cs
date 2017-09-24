using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClientFruitonFactory {

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
            result.Add(newObject);
        }
        return result;
    }

}
