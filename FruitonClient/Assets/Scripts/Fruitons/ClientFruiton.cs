using fruiton.kernel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientFruiton{
    private Fruiton kernelFruiton;
    private GameObject fruitonObject;

    public Fruiton KernelFruiton
    {
        get
        {
            return kernelFruiton;
        }
    }

    public GameObject FruitonObject
    {
        get
        {
            return fruitonObject;
        }
        set
        {
            fruitonObject = value;
        }
    }

    public ClientFruiton(Fruiton kernelFruiton)
    {
        this.kernelFruiton = kernelFruiton;
    }
}
