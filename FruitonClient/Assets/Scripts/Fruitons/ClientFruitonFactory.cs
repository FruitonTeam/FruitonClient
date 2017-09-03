using fruiton.fruitDb;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ClientFruitonFactory {

    public static IEnumerable<ClientFruiton> CreateClientFruitons()
    {
        Debug.Log("Loading ALL fruitons.");
        FruitonDatabase fruitonDatabase = GameManager.Instance.FruitonDatabase;
        System.Collections.Generic.List<ClientFruiton> result = new List<ClientFruiton>();
        foreach (int key in fruitonDatabase.fruitonDb._keys)
        {
            // This check is here because Haxe IntMap may also contain invalid key-value pairs, which can be recognised by the fact that their key is 0. 
            if (key == 0) continue;
            Fruiton kernelFruiton = FruitonFactory.makeFruiton(key, fruitonDatabase);
            result.Add(new ClientFruiton(kernelFruiton));
        }
        return result;
    }

}
