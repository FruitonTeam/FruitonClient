using System;
using System.IO;
using System.Linq;
using System.Reflection;
using fruiton.dataStructures;
using fruiton.fruitDb.factories;
using fruiton.kernel;
using UnityEngine;

namespace Util
{
    public static class KernelUtils
    {
        private static readonly string KERNEL_DLL_NAME_PREFIX = "kernel";

        public static string LoadTextResource(string resource)
        {
            Assembly kernelAssembly = GetKernelAssembly();
            if (kernelAssembly != null)
            {
                using (Stream resourceStream = kernelAssembly.GetManifestResourceStream(resource))
                {
                    if (resourceStream == null)
                    {
                        throw new FileNotFoundException("Could not find resource " + resource);
                    }
                    using (StreamReader reader = new StreamReader(resourceStream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            else
            {
                return Resources.Load<TextAsset>("Kernel/" + Path.GetFileNameWithoutExtension(resource)).text;
            }
        }

        public static Assembly GetKernelAssembly()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SingleOrDefault(assembly => assembly.GetName().Name.StartsWith(KERNEL_DLL_NAME_PREFIX));
        }

        public static Fruiton GetFruitonAt(Kernel kernel, Point position)
        {
            return kernel.currentState.field.get(position).fruiton;
        }

        public static string GetFruitonName(int id)
        {
            return GameManager.Instance.AllFruitons.First(fruiton => fruiton.dbId == id).name;
        }
    }
}