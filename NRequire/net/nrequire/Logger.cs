﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace net.nrequire {
    
    internal class Logger {
        
        private readonly Level m_level;
        private readonly String  m_name;
        private readonly IAppender m_appender;

        internal Logger(String name,Level level,IAppender appender){
            m_level = level;
            m_name = name;
            m_appender = appender;
        }

        public static Logger GetLogger(Object instance){
            return GetLogger(instance.GetType());
        }

        public static Logger GetLogger(Type type){
            return GetLogger(type.Name);
        }

        public static Logger GetLogger(String name){
            return new Logger(name,Level.Debug, ConsoleLogAppender.Instance);
        }

        public bool IsTraceEnabled() {
            return IsLevel(Level.Trace);
        }

        public bool IsDebugEnabled() {
            return IsLevel(Level.Debug);
        }

        public bool IsWarnEnabled() {
            return IsLevel(Level.Warn);
        }
        
        public void Trace(String msg){
            Log(Level.Trace,msg);
        }

        public void Trace(String msg, Exception e){
            Log(Level.Trace,msg, e);
        }

        public void TraceFormat(String msg, params Object[] args){
            LogFormat(Level.Trace, msg, args);
        }

        public void Debug(String msg) {
            Log(Level.Debug,msg);
        }

        public void Debug(String msg, Exception e){
            Log(Level.Debug,msg, e);
        }

        public void DebugFormat(String msg, params Object[] args){
            LogFormat(Level.Debug, msg, args);
        }
        
        public void Info(String msg){
            Log(Level.Info,msg);
        }

        public void Info(String msg, Exception e){
            Log(Level.Info,msg, e);
        }

        public void InfoFormat(String msg, params Object[] args){
            LogFormat(Level.Info, msg, args);
        }
        
        public void Warn(String msg){
            Log(Level.Warn,msg);
        }

        public void Warn(String msg, Exception e){
            Log(Level.Warn,msg, e);
        }

        public void WarnFormat(String msg, params Object[] args){
            LogFormat(Level.Warn, msg, args);
        }

        public void Error(String msg){
            Log(Level.Error,msg);
        }

        public void Error(String msg, Exception e){
            Log(Level.Error,msg, e);
        }

        public void ErrorFormat(String msg, params Object[] args){
            LogFormat(Level.Error, msg, args);
        }

        private void Log(Level level,String msg){
            if(IsLevel(level)){
                Append(level,msg);
            }
        }

        private void Log(Level level,String msg, Exception e){
            if(IsLevel(level)){
                if( e == null ){
                    Append(level,msg);
                } else {
                    Append(level,msg,e);
                }
            }
        }
        
        private void LogFormat(Level level,String msg, params Object[] args){
            if(IsLevel(level)){
                if(args!= null && args.Length > 0){
                    Append(level,String.Format(msg,args));
                } else {
                    Append(level,msg);
                }
            }
        }

        private bool IsLevel(Level level){
            return m_level <= level;
        }

        private void Append(Logger.Level level, String msg){
            m_appender.Append(level,m_name,msg);
        }

        private void Append(Logger.Level level, String msg, Exception e){
            m_appender.Append(level,m_name,msg,e);
        }

        internal enum Level {
            Trace,Debug,Info,Warn,Error,Off
        }

        internal interface IAppender {
            void Append(Logger.Level level, String loggerName, String msg);
            void Append(Logger.Level level, String loggerName, String msg, Exception e);
        }
    }
}