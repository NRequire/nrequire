using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace net.nrequire {
    internal static class LoadEmbeddedDlls 
    {
        private static bool Loaded = false;
        
        internal static void Load(){
            if(!Loaded){
                //load embedded Json.Net
                //http://blogs.msdn.com/b/microsoft_press/archive/2010/02/03/jeffrey-richter-excerpt-2-from-clr-via-c-third-edition.aspx
                //http://adamthetech.com/2011/06/embed-dll-files-within-an-exe-c-sharp-winforms/
                AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>  {
                    String resourceName = "net.nrequire.Resources." + new AssemblyName(args.Name).Name + ".dll";
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName)) {
                        Byte[] assemblyData = new Byte[stream.Length];
                        stream.Read(assemblyData, 0, assemblyData.Length);
                        return Assembly.Load(assemblyData);
                    }
                };
                Loaded = true;
            }
        }
    }
}
