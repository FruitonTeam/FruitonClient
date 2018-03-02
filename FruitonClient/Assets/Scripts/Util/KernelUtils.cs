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
    /// <summary>
    /// Contains kernel related helper methods.
    /// </summary>
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

        /// <param name="kernel">kernel instance</param>
        /// <param name="position">position on a game board</param>
        /// <returns>fruiton that's on the given position in the kernel, null if given position is empty</returns>
        public static Fruiton GetFruitonAt(Kernel kernel, Point position)
        {
            return kernel.currentState.field.get(position).fruiton;
        }

        /// <summary>
        /// Finds name of a fruiton based on its id.
        /// </summary>
        /// <param name="id">if of the fruiton</param>
        /// <returns>fruiton's name</returns>
        public static string GetFruitonName(int id)
        {
            return GameManager.Instance.AllFruitons.First(fruiton => fruiton.dbId == id).name;
        }
    }
}