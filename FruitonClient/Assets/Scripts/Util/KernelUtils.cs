using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Util
{
    public static class KernelUtils
    {
        private static readonly bool KERNEL_NOT_IN_ASSEMBLY = false;

        private static readonly string KERNEL_DLL_NAME_PREFIX = "kernel";

        public static string LoadTextResource(string resource)
        {
            if (KERNEL_NOT_IN_ASSEMBLY)
            {
                return Resources.Load<TextAsset>(resource).text;
            }
            
            Assembly kernelAssembly = AppDomain.CurrentDomain.GetAssemblies()
                    .SingleOrDefault(assembly => assembly.GetName().Name.StartsWith(KERNEL_DLL_NAME_PREFIX));
            if (kernelAssembly == null)
            {
                throw new DllNotFoundException("No dll with prefix " + KERNEL_DLL_NAME_PREFIX + " found");
            }
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
        
    }
}