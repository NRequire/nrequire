﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {

    internal class ConsoleLogAppender : Logger.IAppender {

        public static readonly ConsoleLogAppender Instance = new ConsoleLogAppender();

        public void Append(Logger.Level level, String loggerName, String msg) {
            Console.WriteLine(String.Format("[{0}] {1} {2}",level, loggerName,msg));
        }

        public void Append(Logger.Level level, String loggerName, String msg, Exception e) {
            Console.WriteLine(String.Format("[{0}] {1} {2}", level, loggerName, msg));
            Console.WriteLine(e.ToString());
        }
    }
}
