using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NRequire.Matcher;

namespace NRequire {

    public class AnAbstractDependency:AnAbstractDependency<AnAbstractDependency,AbstractDependency>{
        public static AnAbstractDependency With(){
            return new AnAbstractDependency();
        }
    }

    public abstract class AnAbstractDependency<TSelf,T> : AReflectionMatcher<T> 
        where TSelf : AnAbstractDependency<TSelf,T>
        where T : AbstractDependency
    {
        protected TSelf Self { get { return (TSelf)this; } }

        public TSelf Group(String expect) {
            Group(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Group(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Group", (actual) => actual.Group, matcher);
            return Self;
        }

        public TSelf Name(String expect) {
            Name(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Name(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Name", (actual) => actual.Name, matcher);
            return Self;
        }

        public TSelf Runtime(String expect) {
            Runtime(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Runtime(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Runtime", (actual) => actual.Runtime, matcher);
            return Self;
        }

        public TSelf Arch(String expect) {
            Arch(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Arch(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Arch", (actual) => actual.Arch, matcher);
            return Self;
        }

        public TSelf NullExt() {
            Ext(AString.Null());
            return Self;
        }


        public TSelf Ext(String expect) {
            Ext(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Ext(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Ext", (actual) => actual.Ext, matcher);
            return Self;
        }

        public TSelf Classifiers(String expect) {
            Classifiers(AString.EqualTo(expect));
            return Self;
        }

        public TSelf Classifiers(IExtendedMatcher<Classifiers> matcher) {
            AddProperty<Classifiers>("Classifiers", (actual) => actual.Classifiers, matcher);
            return Self;
        }

        public TSelf Classifiers(IExtendedMatcher<String> matcher) {
            AddProperty<String>("Classifiers", (actual) => actual.Classifiers==null?null:actual.Classifiers.ToString(), matcher);
            return Self;
        }

    }
}
