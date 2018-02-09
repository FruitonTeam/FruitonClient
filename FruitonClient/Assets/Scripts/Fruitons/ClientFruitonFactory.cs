using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using Google.Protobuf.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

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

    public static IEnumerable<GameObject> CreateClientFruitonTeam(IEnumerable<int> teamIDs, GameObject parent)
    {
        var result = new List<GameObject>();
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        foreach (int id in teamIDs)
        {
            Fruiton kernelFruiton = FruitonFactory.makeFruiton(id, fruitonDatabase);
            var newFruitonObject = UnityEngine.Object.Instantiate(Resources.Load("Models/Battle/BoyFighter", typeof(GameObject))) as GameObject;
            newFruitonObject.GetComponentInChildren<SkeletonAnimation>().skeleton.SetSkin(kernelFruiton.model);
            newFruitonObject.AddComponent<ClientFruiton>().KernelFruiton = kernelFruiton;
            newFruitonObject.transform.parent = parent.transform;
            result.Add(newFruitonObject);
        }
        return result;
    }

}
