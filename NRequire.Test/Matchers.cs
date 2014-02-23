 using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TestFirst.Net;
using TestFirst.Net.Matcher;

namespace NRequire {

    public partial class AVersion : PropertyMatcher<NRequire.Version>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Version PropertyNames = null;


        public static AVersion With(){
                return new AVersion();
        }

        public static IMatcher<NRequire.Version> Null(){
                return AnInstance.Null<NRequire.Version>();
        }

        public static IMatcher<NRequire.Version> NotNull(){
                return AnInstance.NotNull<NRequire.Version>();
        }

        public static IMatcher<NRequire.Version> Instance(NRequire.Version expect){
                return AnInstance.SameAs(expect);
        }

        public AVersion Build(int expect) {
            Build(AnInt.EqualTo(expect));
            return this;
        }

        public AVersion Build(IMatcher<int?> matcher) {
            WithProperty(()=>PropertyNames.Build,matcher);
            return this;
        }

        public AVersion IsBuild(bool expect) {
            IsBuild(ABool.EqualTo(expect));
            return this;
        }

        public AVersion IsBuild(IMatcher<bool?> matcher) {
            WithProperty(()=>PropertyNames.IsBuild,matcher);
            return this;
        }

        public AVersion IsQualified(bool expect) {
            IsQualified(ABool.EqualTo(expect));
            return this;
        }

        public AVersion IsQualified(IMatcher<bool?> matcher) {
            WithProperty(()=>PropertyNames.IsQualified,matcher);
            return this;
        }

        public AVersion IsSnapshot(bool expect) {
            IsSnapshot(ABool.EqualTo(expect));
            return this;
        }

        public AVersion IsSnapshot(IMatcher<bool?> matcher) {
            WithProperty(()=>PropertyNames.IsSnapshot,matcher);
            return this;
        }

        public AVersion IsTimestamped(bool expect) {
            IsTimestamped(ABool.EqualTo(expect));
            return this;
        }

        public AVersion IsTimestamped(IMatcher<bool?> matcher) {
            WithProperty(()=>PropertyNames.IsTimestamped,matcher);
            return this;
        }

        public AVersion Major(int expect) {
            Major(AnInt.EqualTo(expect));
            return this;
        }

        public AVersion Major(IMatcher<int?> matcher) {
            WithProperty(()=>PropertyNames.Major,matcher);
            return this;
        }

        public AVersion MatchString(string expect) {
            MatchString(AString.EqualTo(expect));
            return this;
        }

        public AVersion MatchStringNull() {
            MatchString(AString.Null());
            return this;
        }

        public AVersion MatchString(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.MatchString,matcher);
            return this;
        }

        public AVersion Minor(int expect) {
            Minor(AnInt.EqualTo(expect));
            return this;
        }

        public AVersion Minor(IMatcher<int?> matcher) {
            WithProperty(()=>PropertyNames.Minor,matcher);
            return this;
        }

        public AVersion Qualifier(string expect) {
            Qualifier(AString.EqualTo(expect));
            return this;
        }

        public AVersion QualifierNull() {
            Qualifier(AString.Null());
            return this;
        }

        public AVersion Qualifier(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Qualifier,matcher);
            return this;
        }

        public AVersion Revision(int expect) {
            Revision(AnInt.EqualTo(expect));
            return this;
        }

        public AVersion Revision(IMatcher<int?> matcher) {
            WithProperty(()=>PropertyNames.Revision,matcher);
            return this;
        }

        public AVersion Timestamp(System.DateTime? expect) {
            Timestamp(ADateTime.EqualTo(expect));
            return this;
        }

        public AVersion TimestampNull() {
            Timestamp(AnInstance.EqualTo<System.DateTime?>(null));
            return this;
        }

        public AVersion Timestamp(IMatcher<System.DateTime?> matcher) {
            WithProperty(()=>PropertyNames.Timestamp,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AClassifier : PropertyMatcher<NRequire.Classifiers>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Classifiers PropertyNames = null;


        public static AClassifier With(){
                return new AClassifier();
        }

        public static IMatcher<NRequire.Classifiers> Null(){
                return AnInstance.Null<NRequire.Classifiers>();
        }

        public static IMatcher<NRequire.Classifiers> NotNull(){
                return AnInstance.NotNull<NRequire.Classifiers>();
        }

        public static IMatcher<NRequire.Classifiers> Instance(NRequire.Classifiers expect){
                return AnInstance.SameAs(expect);
        }

        public AClassifier Count(int expect) {
            Count(AnInt.EqualTo(expect));
            return this;
        }

        public AClassifier Count(IMatcher<int?> matcher) {
            WithProperty(()=>PropertyNames.Count,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class ADependency : PropertyMatcher<NRequire.Dependency>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Dependency PropertyNames = null;


        public static ADependency With(){
                return new ADependency();
        }

        public static IMatcher<NRequire.Dependency> Null(){
                return AnInstance.Null<NRequire.Dependency>();
        }

        public static IMatcher<NRequire.Dependency> NotNull(){
                return AnInstance.NotNull<NRequire.Dependency>();
        }

        public static IMatcher<NRequire.Dependency> Instance(NRequire.Dependency expect){
                return AnInstance.SameAs(expect);
        }

        public ADependency Arch(string expect) {
            Arch(AString.EqualTo(expect));
            return this;
        }

        public ADependency ArchNull() {
            Arch(AString.Null());
            return this;
        }

        public ADependency Arch(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Arch,matcher);
            return this;
        }

        public ADependency Classifiers(string expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public ADependency Classifiers(NRequire.Classifiers expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public ADependency ClassifiersNull() {
            Classifiers(AnInstance.EqualTo<NRequire.Classifiers>(null));
            return this;
        }

        public ADependency Classifiers(IMatcher<NRequire.Classifiers> matcher) {
            WithProperty(()=>PropertyNames.Classifiers,matcher);
            return this;
        }

        public ADependency Ext(string expect) {
            Ext(AString.EqualTo(expect));
            return this;
        }

        public ADependency ExtNull() {
            Ext(AString.Null());
            return this;
        }

        public ADependency Ext(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Ext,matcher);
            return this;
        }

        public ADependency Group(string expect) {
            Group(AString.EqualTo(expect));
            return this;
        }

        public ADependency GroupNull() {
            Group(AString.Null());
            return this;
        }

        public ADependency Group(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Group,matcher);
            return this;
        }

        public ADependency Name(string expect) {
            Name(AString.EqualTo(expect));
            return this;
        }

        public ADependency NameNull() {
            Name(AString.Null());
            return this;
        }

        public ADependency Name(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Name,matcher);
            return this;
        }

        public ADependency Runtime(string expect) {
            Runtime(AString.EqualTo(expect));
            return this;
        }

        public ADependency RuntimeNull() {
            Runtime(AString.Null());
            return this;
        }

        public ADependency Runtime(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Runtime,matcher);
            return this;
        }

        public ADependency Scope(NRequire.Scopes? expect) {
            Scope(AnInstance.EqualTo(expect));
            return this;
        }

        public ADependency Scope(IMatcher<NRequire.Scopes?> matcher) {
            WithProperty(()=>PropertyNames.Scope,matcher);
            return this;
        }

        public ADependency SourceNull() {
            Source(AnInstance.EqualTo<NRequire.SourceLocations>(null));
            return this;
        }

        public ADependency Source(IMatcher<NRequire.SourceLocations> matcher) {
            WithProperty(()=>PropertyNames.Source,matcher);
            return this;
        }

        public ADependency Url(System.Uri expect) {
            Url(AnUri.EqualTo(expect));
            return this;
        }

        public ADependency UrlNull() {
            Url(AnInstance.EqualTo<System.Uri>(null));
            return this;
        }

        public ADependency Url(IMatcher<System.Uri> matcher) {
            WithProperty(()=>PropertyNames.Url,matcher);
            return this;
        }

        public ADependency Version(string expect) {
            Version(NRequire.Version.Parse(expect));
            return this;
        }

        public ADependency Version(NRequire.Version expect) {
            Version(AnInstance.EqualTo(expect));
            return this;
        }

        public ADependency VersionNull() {
            Version(AnInstance.EqualTo<NRequire.Version>(null));
            return this;
        }

        public ADependency Version(IMatcher<NRequire.Version> matcher) {
            WithProperty(()=>PropertyNames.Version,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AWish : PropertyMatcher<NRequire.Wish>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Wish PropertyNames = null;


        public static AWish With(){
                return new AWish();
        }

        public static IMatcher<NRequire.Wish> Null(){
                return AnInstance.Null<NRequire.Wish>();
        }

        public static IMatcher<NRequire.Wish> NotNull(){
                return AnInstance.NotNull<NRequire.Wish>();
        }

        public static IMatcher<NRequire.Wish> Instance(NRequire.Wish expect){
                return AnInstance.SameAs(expect);
        }

        public AWish Arch(string expect) {
            Arch(AString.EqualTo(expect));
            return this;
        }

        public AWish ArchNull() {
            Arch(AString.Null());
            return this;
        }

        public AWish Arch(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Arch,matcher);
            return this;
        }

        public AWish Classifiers(string expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AWish Classifiers(NRequire.Classifiers expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AWish ClassifiersNull() {
            Classifiers(AnInstance.EqualTo<NRequire.Classifiers>(null));
            return this;
        }

        public AWish Classifiers(IMatcher<NRequire.Classifiers> matcher) {
            WithProperty(()=>PropertyNames.Classifiers,matcher);
            return this;
        }

        public AWish CopyTo(string expect) {
            CopyTo(AString.EqualTo(expect));
            return this;
        }

        public AWish CopyToNull() {
            CopyTo(AString.Null());
            return this;
        }

        public AWish CopyTo(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.CopyTo,matcher);
            return this;
        }

        public AWish Ext(string expect) {
            Ext(AString.EqualTo(expect));
            return this;
        }

        public AWish ExtNull() {
            Ext(AString.Null());
            return this;
        }

        public AWish Ext(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Ext,matcher);
            return this;
        }

        public AWish Group(string expect) {
            Group(AString.EqualTo(expect));
            return this;
        }

        public AWish GroupNull() {
            Group(AString.Null());
            return this;
        }

        public AWish Group(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Group,matcher);
            return this;
        }

        public AWish Name(string expect) {
            Name(AString.EqualTo(expect));
            return this;
        }

        public AWish NameNull() {
            Name(AString.Null());
            return this;
        }

        public AWish Name(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Name,matcher);
            return this;
        }

        public AWish Runtime(string expect) {
            Runtime(AString.EqualTo(expect));
            return this;
        }

        public AWish RuntimeNull() {
            Runtime(AString.Null());
            return this;
        }

        public AWish Runtime(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Runtime,matcher);
            return this;
        }

        public AWish Scope(NRequire.Scopes? expect) {
            Scope(AnInstance.EqualTo(expect));
            return this;
        }

        public AWish Scope(IMatcher<NRequire.Scopes?> matcher) {
            WithProperty(()=>PropertyNames.Scope,matcher);
            return this;
        }

        public AWish SourceNull() {
            Source(AnInstance.EqualTo<NRequire.SourceLocations>(null));
            return this;
        }

        public AWish Source(IMatcher<NRequire.SourceLocations> matcher) {
            WithProperty(()=>PropertyNames.Source,matcher);
            return this;
        }

        public AWish TransitiveWishesNull() {
            TransitiveWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AWish TransitiveWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.TransitiveWishes,matcher);
            return this;
        }

        public AWish Url(System.Uri expect) {
            Url(AnUri.EqualTo(expect));
            return this;
        }

        public AWish UrlNull() {
            Url(AnInstance.EqualTo<System.Uri>(null));
            return this;
        }

        public AWish Url(IMatcher<System.Uri> matcher) {
            WithProperty(()=>PropertyNames.Url,matcher);
            return this;
        }

        public AWish Version(string expect) {
            Version(Matchers.Function<VersionMatcher>(actual => actual.ToString().Equals(expect), () => expect));
            return this;
        }

        public AWish Version(NRequire.Version expect) {
            Version(expect.ToString());
            return this;
        }

        public AWish VersionNull() {
            Version(AnInstance.EqualTo<NRequire.VersionMatcher>(null));
            return this;
        }

        public AWish Version(IMatcher<NRequire.VersionMatcher> matcher) {
            WithProperty(()=>PropertyNames.Version,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AModule : PropertyMatcher<NRequire.Module>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Module PropertyNames = null;


        public static AModule With(){
                return new AModule();
        }

        public static IMatcher<NRequire.Module> Null(){
                return AnInstance.Null<NRequire.Module>();
        }

        public static IMatcher<NRequire.Module> NotNull(){
                return AnInstance.NotNull<NRequire.Module>();
        }

        public static IMatcher<NRequire.Module> Instance(NRequire.Module expect){
                return AnInstance.SameAs(expect);
        }

        public AModule Arch(string expect) {
            Arch(AString.EqualTo(expect));
            return this;
        }

        public AModule ArchNull() {
            Arch(AString.Null());
            return this;
        }

        public AModule Arch(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Arch,matcher);
            return this;
        }

        public AModule Classifiers(string expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AModule Classifiers(NRequire.Classifiers expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AModule ClassifiersNull() {
            Classifiers(AnInstance.EqualTo<NRequire.Classifiers>(null));
            return this;
        }

        public AModule Classifiers(IMatcher<NRequire.Classifiers> matcher) {
            WithProperty(()=>PropertyNames.Classifiers,matcher);
            return this;
        }

        public AModule Ext(string expect) {
            Ext(AString.EqualTo(expect));
            return this;
        }

        public AModule ExtNull() {
            Ext(AString.Null());
            return this;
        }

        public AModule Ext(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Ext,matcher);
            return this;
        }

        public AModule Group(string expect) {
            Group(AString.EqualTo(expect));
            return this;
        }

        public AModule GroupNull() {
            Group(AString.Null());
            return this;
        }

        public AModule Group(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Group,matcher);
            return this;
        }

        public AModule Name(string expect) {
            Name(AString.EqualTo(expect));
            return this;
        }

        public AModule NameNull() {
            Name(AString.Null());
            return this;
        }

        public AModule Name(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Name,matcher);
            return this;
        }

        public AModule OptionalWishesNull() {
            OptionalWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AModule OptionalWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.OptionalWishes,matcher);
            return this;
        }

        public AModule Runtime(string expect) {
            Runtime(AString.EqualTo(expect));
            return this;
        }

        public AModule RuntimeNull() {
            Runtime(AString.Null());
            return this;
        }

        public AModule Runtime(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Runtime,matcher);
            return this;
        }

        public AModule RuntimeWishesNull() {
            RuntimeWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AModule RuntimeWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.RuntimeWishes,matcher);
            return this;
        }

        public AModule Scope(NRequire.Scopes? expect) {
            Scope(AnInstance.EqualTo(expect));
            return this;
        }

        public AModule Scope(IMatcher<NRequire.Scopes?> matcher) {
            WithProperty(()=>PropertyNames.Scope,matcher);
            return this;
        }

        public AModule SourceNull() {
            Source(AnInstance.EqualTo<NRequire.SourceLocations>(null));
            return this;
        }

        public AModule Source(IMatcher<NRequire.SourceLocations> matcher) {
            WithProperty(()=>PropertyNames.Source,matcher);
            return this;
        }

        public AModule SourceName(string expect) {
            SourceName(AString.EqualTo(expect));
            return this;
        }

        public AModule SourceNameNull() {
            SourceName(AString.Null());
            return this;
        }

        public AModule SourceName(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.SourceName,matcher);
            return this;
        }

        public AModule TransitiveWishesNull() {
            TransitiveWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AModule TransitiveWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.TransitiveWishes,matcher);
            return this;
        }

        public AModule Url(System.Uri expect) {
            Url(AnUri.EqualTo(expect));
            return this;
        }

        public AModule UrlNull() {
            Url(AnInstance.EqualTo<System.Uri>(null));
            return this;
        }

        public AModule Url(IMatcher<System.Uri> matcher) {
            WithProperty(()=>PropertyNames.Url,matcher);
            return this;
        }

        public AModule Version(string expect) {
            Version(NRequire.Version.Parse(expect));
            return this;
        }

        public AModule Version(NRequire.Version expect) {
            Version(AnInstance.EqualTo(expect));
            return this;
        }

        public AModule VersionNull() {
            Version(AnInstance.EqualTo<NRequire.Version>(null));
            return this;
        }

        public AModule Version(IMatcher<NRequire.Version> matcher) {
            WithProperty(()=>PropertyNames.Version,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class ASolution : PropertyMatcher<NRequire.Solution>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Solution PropertyNames = null;


        public static ASolution With(){
                return new ASolution();
        }

        public static IMatcher<NRequire.Solution> Null(){
                return AnInstance.Null<NRequire.Solution>();
        }

        public static IMatcher<NRequire.Solution> NotNull(){
                return AnInstance.NotNull<NRequire.Solution>();
        }

        public static IMatcher<NRequire.Solution> Instance(NRequire.Solution expect){
                return AnInstance.SameAs(expect);
        }

        public ASolution SolutionFormat(string expect) {
            SolutionFormat(AString.EqualTo(expect));
            return this;
        }

        public ASolution SolutionFormatNull() {
            SolutionFormat(AString.Null());
            return this;
        }

        public ASolution SolutionFormat(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.SolutionFormat,matcher);
            return this;
        }

        public ASolution SourceNull() {
            Source(AnInstance.EqualTo<NRequire.SourceLocations>(null));
            return this;
        }

        public ASolution Source(IMatcher<NRequire.SourceLocations> matcher) {
            WithProperty(()=>PropertyNames.Source,matcher);
            return this;
        }

        public ASolution SourceName(string expect) {
            SourceName(AString.EqualTo(expect));
            return this;
        }

        public ASolution SourceNameNull() {
            SourceName(AString.Null());
            return this;
        }

        public ASolution SourceName(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.SourceName,matcher);
            return this;
        }

        public ASolution WishDefaultsNull() {
            WishDefaults(AnInstance.EqualTo<NRequire.Wish>(null));
            return this;
        }

        public ASolution WishDefaults(IMatcher<NRequire.Wish> matcher) {
            WithProperty(()=>PropertyNames.WishDefaults,matcher);
            return this;
        }

        public ASolution WishesNull() {
            Wishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public ASolution Wishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.Wishes,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AProject : PropertyMatcher<NRequire.Project>{

        // provide IDE rename and find reference support
        private static readonly NRequire.Project PropertyNames = null;


        public static AProject With(){
                return new AProject();
        }

        public static IMatcher<NRequire.Project> Null(){
                return AnInstance.Null<NRequire.Project>();
        }

        public static IMatcher<NRequire.Project> NotNull(){
                return AnInstance.NotNull<NRequire.Project>();
        }

        public static IMatcher<NRequire.Project> Instance(NRequire.Project expect){
                return AnInstance.SameAs(expect);
        }

        public AProject Arch(string expect) {
            Arch(AString.EqualTo(expect));
            return this;
        }

        public AProject ArchNull() {
            Arch(AString.Null());
            return this;
        }

        public AProject Arch(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Arch,matcher);
            return this;
        }

        public AProject Classifiers(string expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AProject Classifiers(NRequire.Classifiers expect) {
            Classifiers(AClassifier.EqualTo(expect));
            return this;
        }

        public AProject ClassifiersNull() {
            Classifiers(AnInstance.EqualTo<NRequire.Classifiers>(null));
            return this;
        }

        public AProject Classifiers(IMatcher<NRequire.Classifiers> matcher) {
            WithProperty(()=>PropertyNames.Classifiers,matcher);
            return this;
        }

        public AProject Ext(string expect) {
            Ext(AString.EqualTo(expect));
            return this;
        }

        public AProject ExtNull() {
            Ext(AString.Null());
            return this;
        }

        public AProject Ext(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Ext,matcher);
            return this;
        }

        public AProject Group(string expect) {
            Group(AString.EqualTo(expect));
            return this;
        }

        public AProject GroupNull() {
            Group(AString.Null());
            return this;
        }

        public AProject Group(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Group,matcher);
            return this;
        }

        public AProject Name(string expect) {
            Name(AString.EqualTo(expect));
            return this;
        }

        public AProject NameNull() {
            Name(AString.Null());
            return this;
        }

        public AProject Name(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Name,matcher);
            return this;
        }

        public AProject OptionalWishesNull() {
            OptionalWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AProject OptionalWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.OptionalWishes,matcher);
            return this;
        }

        public AProject ProjectFormat(string expect) {
            ProjectFormat(AString.EqualTo(expect));
            return this;
        }

        public AProject ProjectFormatNull() {
            ProjectFormat(AString.Null());
            return this;
        }

        public AProject ProjectFormat(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.ProjectFormat,matcher);
            return this;
        }

        public AProject ProvidedWishesNull() {
            ProvidedWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AProject ProvidedWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.ProvidedWishes,matcher);
            return this;
        }

        public AProject Runtime(string expect) {
            Runtime(AString.EqualTo(expect));
            return this;
        }

        public AProject RuntimeNull() {
            Runtime(AString.Null());
            return this;
        }

        public AProject Runtime(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Runtime,matcher);
            return this;
        }

        public AProject RuntimeWishesNull() {
            RuntimeWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AProject RuntimeWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.RuntimeWishes,matcher);
            return this;
        }

        public AProject Scope(NRequire.Scopes? expect) {
            Scope(AnInstance.EqualTo(expect));
            return this;
        }

        public AProject Scope(IMatcher<NRequire.Scopes?> matcher) {
            WithProperty(()=>PropertyNames.Scope,matcher);
            return this;
        }

        public AProject SourceNull() {
            Source(AnInstance.EqualTo<NRequire.SourceLocations>(null));
            return this;
        }

        public AProject Source(IMatcher<NRequire.SourceLocations> matcher) {
            WithProperty(()=>PropertyNames.Source,matcher);
            return this;
        }

        public AProject SourceName(string expect) {
            SourceName(AString.EqualTo(expect));
            return this;
        }

        public AProject SourceNameNull() {
            SourceName(AString.Null());
            return this;
        }

        public AProject SourceName(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.SourceName,matcher);
            return this;
        }

        public AProject TransitiveWishesNull() {
            TransitiveWishes(AnInstance.EqualTo<System.Collections.Generic.IEnumerable<NRequire.Wish>>(null));
            return this;
        }

        public AProject TransitiveWishes(IMatcher<System.Collections.Generic.IEnumerable<NRequire.Wish>> matcher) {
            WithProperty(()=>PropertyNames.TransitiveWishes,matcher);
            return this;
        }

        public AProject Url(System.Uri expect) {
            Url(AnUri.EqualTo(expect));
            return this;
        }

        public AProject UrlNull() {
            Url(AnInstance.EqualTo<System.Uri>(null));
            return this;
        }

        public AProject Url(IMatcher<System.Uri> matcher) {
            WithProperty(()=>PropertyNames.Url,matcher);
            return this;
        }

        public AProject Version(string expect) {
            Version(NRequire.Version.Parse(expect));
            return this;
        }

        public AProject Version(NRequire.Version expect) {
            Version(AnInstance.EqualTo(expect));
            return this;
        }

        public AProject VersionNull() {
            Version(AnInstance.EqualTo<NRequire.Version>(null));
            return this;
        }

        public AProject Version(IMatcher<NRequire.Version> matcher) {
            WithProperty(()=>PropertyNames.Version,matcher);
            return this;
        }

        public AProject WishDefaultsNull() {
            WishDefaults(AnInstance.EqualTo<NRequire.Wish>(null));
            return this;
        }

        public AProject WishDefaults(IMatcher<NRequire.Wish> matcher) {
            WithProperty(()=>PropertyNames.WishDefaults,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AVSSolution : PropertyMatcher<NRequire.VSSolution>{

        // provide IDE rename and find reference support
        private static readonly NRequire.VSSolution PropertyNames = null;


        public static AVSSolution With(){
                return new AVSSolution();
        }

        public static IMatcher<NRequire.VSSolution> Null(){
                return AnInstance.Null<NRequire.VSSolution>();
        }

        public static IMatcher<NRequire.VSSolution> NotNull(){
                return AnInstance.NotNull<NRequire.VSSolution>();
        }

        public static IMatcher<NRequire.VSSolution> Instance(NRequire.VSSolution expect){
                return AnInstance.SameAs(expect);
        }

        public AVSSolution Path(System.IO.FileInfo expect) {
            Path(AFileInfo.EqualTo(expect));
            return this;
        }

        public AVSSolution PathNull() {
            Path(AnInstance.EqualTo<System.IO.FileInfo>(null));
            return this;
        }

        public AVSSolution Path(IMatcher<System.IO.FileInfo> matcher) {
            WithProperty(()=>PropertyNames.Path,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AProjectReference : PropertyMatcher<NRequire.VSSolution.ProjectReference>{

        // provide IDE rename and find reference support
        private static readonly NRequire.VSSolution.ProjectReference PropertyNames = null;


        public static AProjectReference With(){
                return new AProjectReference();
        }

        public static IMatcher<NRequire.VSSolution.ProjectReference> Null(){
                return AnInstance.Null<NRequire.VSSolution.ProjectReference>();
        }

        public static IMatcher<NRequire.VSSolution.ProjectReference> NotNull(){
                return AnInstance.NotNull<NRequire.VSSolution.ProjectReference>();
        }

        public static IMatcher<NRequire.VSSolution.ProjectReference> Instance(NRequire.VSSolution.ProjectReference expect){
                return AnInstance.SameAs(expect);
        }

        public AProjectReference Guid(System.Guid expect) {
            Guid(AGuid.EqualTo(expect));
            return this;
        }

        public AProjectReference Guid(IMatcher<System.Guid?> matcher) {
            WithProperty(()=>PropertyNames.Guid,matcher);
            return this;
        }

        public AProjectReference Name(string expect) {
            Name(AString.EqualTo(expect));
            return this;
        }

        public AProjectReference NameNull() {
            Name(AString.Null());
            return this;
        }

        public AProjectReference Name(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Name,matcher);
            return this;
        }

        public AProjectReference Path(string expect) {
            Path(AString.EqualTo(expect));
            return this;
        }

        public AProjectReference PathNull() {
            Path(AString.Null());
            return this;
        }

        public AProjectReference Path(IMatcher<string> matcher) {
            WithProperty(()=>PropertyNames.Path,matcher);
            return this;
        }
    }
}

namespace NRequire {

    public partial class AVSProject : PropertyMatcher<NRequire.VSProject>{

        // provide IDE rename and find reference support
        private static readonly NRequire.VSProject PropertyNames = null;


        public static AVSProject With(){
                return new AVSProject();
        }

        public static IMatcher<NRequire.VSProject> Null(){
                return AnInstance.Null<NRequire.VSProject>();
        }

        public static IMatcher<NRequire.VSProject> NotNull(){
                return AnInstance.NotNull<NRequire.VSProject>();
        }

        public static IMatcher<NRequire.VSProject> Instance(NRequire.VSProject expect){
                return AnInstance.SameAs(expect);
        }

        public AVSProject Path(System.IO.FileInfo expect) {
            Path(AFileInfo.EqualTo(expect));
            return this;
        }

        public AVSProject PathNull() {
            Path(AnInstance.EqualTo<System.IO.FileInfo>(null));
            return this;
        }

        public AVSProject Path(IMatcher<System.IO.FileInfo> matcher) {
            WithProperty(()=>PropertyNames.Path,matcher);
            return this;
        }
    }
}

