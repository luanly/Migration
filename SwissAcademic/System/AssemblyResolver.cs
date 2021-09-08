using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SwissAcademic
{
    public static class AssemblyResolver
    {
        static string WorkingFolder;
        static bool Started;
        public static void Start(string workingFolder)
        {
            if (Started) return;
            Started = true;
            WorkingFolder = workingFolder;
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        public static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            try
            {
                var assemblyName = args.Name.IndexOf(",") != -1 ? args.Name.Substring(0, args.Name.IndexOf(",")).Trim() + ".dll" : args.Name + ".dll";
                if(assemblyName.Contains(".resources"))
                {
                    return null;
                }
                var assemblyPath = Path.Combine(WorkingFolder, assemblyName);

                Assembly assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName == args.Name);
                if (assembly != null)
                    return assembly;

                if (File.Exists(assemblyPath))
                {
                    return Assembly.LoadFrom(assemblyPath);
                }

            }
            catch
            {

            }
            return null;
        }
    }
}
