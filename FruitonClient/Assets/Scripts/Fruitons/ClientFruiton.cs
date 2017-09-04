using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientFruiton {

    public Fruiton KernelFruiton { get; private set;}

    public GameObject FruitonObject { get; set; }

    public ClientFruiton(Fruiton kernelFruiton)
    {
        KernelFruiton = kernelFruiton;
    }
}
