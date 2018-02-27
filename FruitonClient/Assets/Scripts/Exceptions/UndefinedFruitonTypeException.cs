using System;

namespace Exceptions
{
    public class UndefinedFruitonTypeException : Exception
    {
        public UndefinedFruitonTypeException() : base(ExceptionsMessages.UNEXPECTED_FRUITON_TYPE)
        {

        }

        public UndefinedFruitonTypeException(string message) : base(message)
        {

        }
    }
}
