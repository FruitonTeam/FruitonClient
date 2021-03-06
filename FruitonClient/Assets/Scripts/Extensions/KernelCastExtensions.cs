﻿using System.Collections.Generic;
using Cz.Cuni.Mff.Fruiton.Dto;
using KVector2 = fruiton.dataStructures.Point;

namespace Extensions
{
    public static class KernelCastExtensions {

        public static List<T> ToList<T>(this haxe.root.Array<T> array)
        {
            var list = new List<T>(array.length);
            for (int i = 0; i < array.length; i++)
            {
                list.Add(array[i]);
            }
            return list;
        }

        public static List<TOut> CastToList<TOut, TIn>(this haxe.root.Array<TIn> array)
            where TIn : class
            where TOut : TIn
        {
            var list = new List<TOut>(array.length);
            for (int i = 0; i < array.length; i++)
            {
                list.Add((TOut)array[i]);
            }
            return list;
        }

        public static List<TOut> CastToList<TOut>(this haxe.root.Array<object> array)
        {
            return array.CastToList<TOut, object>();
        }

        public static List<List<TOut>> CastToList2D<TOut>(this haxe.root.Array<object> array)
            where TOut : class
        {
            var list = new List<List<TOut>>(array.length);
            for (int i = 0; i < array.length; i++)
            {
                list.Add(new List<TOut>());
                var inner = (haxe.root.Array<object>) array[i];
                for (int j = 0; j < inner.length; j++)
                {
                    list[i].Add((TOut)inner[j]);
                }
            }
            return list;
        }

        public static Position ToPosition(this KVector2 kernelPoint)
        {
            return new Position { X = kernelPoint.x, Y = kernelPoint.y };
        }

        public static KVector2 ToKernelPosition(this Position protoPosition)
        {
            return new KVector2(protoPosition.X, protoPosition.Y);
        }
    }
}
