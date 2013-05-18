using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NRequire.Lang {

    public static class PreConditions {

        public static void NotBlank(String val, String name, Func<String> msgFactory = null) {
            NotNull(val, name, msgFactory);
            if (val.Length == 0) {
                throw new ArgumentNullException(String.Format("Expected '{0}' to not be empty{1}", name, AppendMsg(msgFactory)));

            }
            if (string.IsNullOrWhiteSpace(val)) {
                throw new ArgumentNullException(String.Format("Expected '{0}' to not be whitespace{1}", name, AppendMsg(msgFactory)));
            }
        }
        
        public static void NotNull<T>(T val, String name, Func<String> msgFactory = null) {
            if (val == null) {
                throw new ArgumentNullException(String.Format("Expected '{0}' to not be null{1}", name,AppendMsg(msgFactory)));
            }
        }

        

        private static String AppendMsg(Func<String> msgFactory){
            if( msgFactory != null){
                return ". " + msgFactory.Invoke();
            }
            return "";
        }
    }
}
