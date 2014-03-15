using System;
using System.Collections.Generic;
using NRequire.Model;

namespace NRequire 
{
    public abstract class BaseDependencyTest {

        private static ISource Source = SourceLocations.FromName("BaseDependencyTest");

        protected NewDependencyCache CacheWith(){
            return NewDependencyCache.With();
        }

        protected Dependency DepFrom(String parseString) {
            return Dependency.Parse(parseString);
        }

        protected Dependency DepWith(String name, String version) {
            return DepWith().Defaults().Name(name).Version(version).Source(Source);
        }

        protected NewDependency DepWith() {
            return NewDependency.With();
        }

        protected ADependency ADepWith(String name, String version) {
            return ADepWith().Group(TestDefaults.Group).Name(name).Version(version);
        }

        protected ADependency ADepWith() {
            return ADependency.With();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        /// <returns></returns>
        protected ADependency ADepFrom(String parseString) {
            return ADependency.From(parseString);
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers:scope
        /// </summary>
        /// <param name="parseString"></param>
        /// <returns></returns>
        protected Wish WishFrom(String parseString) {
            return NewWish.Parse(parseString);
        }

        protected Wish WishWith(String name, String version) {
            return WishWith().Defaults().Name(name).Version(version).Source(Source);
        }

        protected NewWish WishWith() {
            return NewWish.With();
        }

        protected AWish AWishWith(String name, String version) {
            return AWishWith().Group(TestDefaults.Group).Name(name).Version(version);
        }

        protected AWish AWishWith() {
            return AWish.With();
        }

        /// <summary>
        /// Parse from  group:name:version:ext:classifiers
        /// </summary>
        /// <param name="parseString"></param>
        /// <returns></returns>
        protected NewModule ModuleFrom(String parseString) {
            return NewModule.Parse(parseString);
        }

        protected NewModule ModuleWith(String name, String version) {
            return ModuleWith().Defaults().Name(name).Version(version).Source(Source);
        }

        protected NewModule ModuleWith() {
            return NewModule.With();
        }
        
        protected List<Wish> ListWith(params Wish[] wishes){        
            return new List<Wish>(wishes);
        }

        protected List<Dependency> ListWith(params Dependency[] deps){
            return new List<Dependency>(deps);
        }
    }
}
