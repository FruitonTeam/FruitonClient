using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UndefinedFruitonTypeException : Exception
{
    public UndefinedFruitonTypeException() : base(ExceptionsMessages.UNEXPECTED_FRUITON_TYPE)
    {

    }

    public UndefinedFruitonTypeException(string message) : base(message)
    {

    }
}
