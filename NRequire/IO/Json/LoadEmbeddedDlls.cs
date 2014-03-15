using System;
using System.Reflection;

namespace NRequire.IO.Json {
    internal static class LoadEmbeddedDlls 
    {
        private static bool Loaded = false;
        private static readonly Object m_lock = new object();

        internal static void Load(){
            if(!Loaded){
                lock (m_lock) {
                    if (!Loaded) {
                        //load embedded Json.Net
                        //http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
                        //http://adamthetech.com/2011/06/embed-dll-files-within-an-exe-c-sharp-winforms/
                        AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                            String resourceName = "NRequire.Resources." + new AssemblyName(args.Name).Name + ".dll";
                            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                                Byte[] assemblyData = new Byte[stream.Length];
                                stream.Read(assemblyData, 0, assemblyData.Length);
                                return Assembly.Load(assemblyData);
                            }
                        };
                    }
                    Loaded = true;
                }
            }
        }
    }
}
