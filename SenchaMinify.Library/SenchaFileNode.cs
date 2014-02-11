using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SenchaMinify.Library
{
    /// <summary>
    /// Sencha application source file wrapper
    /// </summary>
    public class SenchaFileNode
    {
        public enum SortColor { White, Gray, Black };

        /// <summary>
        /// File content
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Defined class name
        /// </summary>
        public string ClassName
        {
            get
            {
                return _ClassName ?? (_ClassName = GetClassName());
            }
        }
        private string _ClassName;

        /// <summary>
        /// Application name (for application files)
        /// </summary>
        public string ApplicationName
        {
            get
            {
                return _ApplicationName ?? (_ApplicationName = GetApplicationName());
            }
        }
        private string _ApplicationName;

        /// <summary>
        /// Gets is current file is application file
        /// </summary>
        public bool IsApplication
        {
            get
            {
                if (!_IsApplication.HasValue)
                {
                    _IsApplication = GetIsApplication();
                }
                return _IsApplication.Value;
            }
        }
        private bool? _IsApplication;

        /// <summary>
        /// Gets is autoCreateViewport property is true
        /// </summary>
        public bool AutoCreateViewport
        {
            get
            {
                if (!_AutoCreateViewport.HasValue)
                {
                    _AutoCreateViewport = GetAutoCreateViewport();
                }
                return _AutoCreateViewport.Value;
            }
        }
        private bool? _AutoCreateViewport;

        /// <summary>
        /// Gets a collection of dependency class names for this source file
        /// </summary>
        public IEnumerable<string> DependencyClasses
        {
            get
            {
                return _DependencyClasses ?? (_DependencyClasses = GetDependencyClasses());
            }
        }
        private IEnumerable<string> _DependencyClasses;

        /// <summary>
        /// Gets or sets a collection of known configuration dependency properties
        /// </summary>
        public virtual IEnumerable<string> DependencyProperties
        {
            get
            {
                return _DependencyProperties ?? (_DependencyProperties = new string[] 
                { 
                    "extend",
                    "mixins",
                    "requires",
                    "model"
                });
            }

            set
            {
                _DependencyProperties = value;
            }
        }
        private IEnumerable<string> _DependencyProperties;

        /// <summary>
        /// Modules for Ext.app.Application and Ext.app.Controller
        /// </summary>
        public virtual IEnumerable<string> ModuleProperties
        {
            get
            {
                return _ModuleProperties ?? (_ModuleProperties = new string[] 
                { 
                    "controller",
                    "model",
                    "view",
                    "store"
                });
            }
            set
            {
                _ModuleProperties = value;
            }
        }
        private IEnumerable<string> _ModuleProperties;

        /// <summary>
        /// Get a collection of file dependencies. Should be used after FillDependencies call.
        /// </summary>
        public virtual IEnumerable<SenchaFileNode> Dependencies { get; set; }

        /// <summary>
        /// Sorting color. Used by topological sort.
        /// </summary>
        public SortColor Color { get; set; }

        public SenchaFileNode()
        {
            this.Color = SortColor.White;
        }

        /// <summary>
        /// Get dependency class names
        /// </summary>
        /// <param name="propertyName">Configuration property name where dependencies are located</param>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetDependencyClasses(string propertyName)
        {
            var regex = new Regex(
                propertyName + @"\s*:\s*" +
                @"(" +
                @"['""](?<single>[\w\S]+?)['""]" +
                @"|" +
                @"\[\s*(['""](?<array>[\w\S]+?)['""]\s*,?\s*)*\s*\]" +
                @")",
                RegexOptions.Singleline
            );

            var match = regex.Match(Content);
            if (match.Success)
            {
                if (match.Groups["single"].Success)
                {
                    yield return match.Groups["single"].Value;
                }
                else if (match.Groups["array"].Success)
                {
                    foreach (Capture capture in match.Groups["array"].Captures)
                    {
                        yield return capture.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Get dependency class names for all known dependency properties
        /// </summary>
        /// <returns>Dependency class names</returns>
        protected virtual IEnumerable<string> GetDependencyClasses()
        {
            foreach (var property in DependencyProperties)
            {
                var dependencyClasses = GetDependencyClasses(property);
                foreach (string className in dependencyClasses)
                {
                    yield return className;
                }
            }

            foreach (var module in ModuleProperties)
            {
                var dependencyClasses = GetDependencyClasses(module + "s");
                foreach (string className in dependencyClasses)
                {
                    var fullClassName = GetFullClassName(module, className);
                    yield return fullClassName;
                }
            }

            if (this.IsApplication && this.AutoCreateViewport)
            {
                yield return GetFullClassName("view", "Viewport");
            }
        }

        /// <summary>
        /// Get defined class name
        /// </summary>
        /// <returns>Defined class name if search was successfull, otherwise null</returns>
        protected virtual string GetClassName()
        {
            var regex = new Regex(@"Ext\.define\(['""](?<classname>[\w\S]+?)['""]", RegexOptions.Singleline);
            var match = regex.Match(Content);
            if (match.Success)
            {
                return match.Groups["classname"].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Get the value of 'name' config. Used for application.
        /// </summary>
        /// <returns>Application name if search was successfull, otherwise null</returns>
        protected virtual string GetApplicationName()
        {
            var regex = new Regex(
                @"name\s*:\s*" +
                @"['""](?<single>[\w\S]+?)['""]",
                RegexOptions.Singleline
            );

            var match = regex.Match(Content);
            if (match.Success)
            {
                return match.Groups["single"].Value;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Check is current file is an application file
        /// </summary>
        /// <returns>True if current file is application file, otherwise false</returns>
        protected virtual bool GetIsApplication()
        {
            var regex = new Regex(@"Ext\.application", RegexOptions.Singleline);
            return regex.IsMatch(Content);
        }

        /// <summary>
        /// Get the value of 'autoCreateViewport' propetry. Used for application.
        /// </summary>
        /// <returns>Value of 'autoCreateViewport' property is true, or false if property not found.</returns>
        protected virtual bool GetAutoCreateViewport()
        {
            var regex = new Regex(@"autoCreateViewport\s*:\s*true", RegexOptions.Singleline);
            return regex.IsMatch(Content);
        }

        /// <summary>
        /// Get full class name for provided class name
        /// </summary>
        /// <param name="module">Module name (e.g. "controller", "view" etc.)</param>
        /// <param name="className">Class name</param>
        /// <remarks>
        /// Used for controllers and applications. For example if an Application has config
        /// {
        ///     name: 'MyApp',
        ///     controllers: ['MyController']
        /// }
        /// then 'MyController' class name should be 'MyApp.controller.MyController'.
        /// 
        /// Same thing about 'models', 'views', 'stores':
        /// Ext.define('MyApp.controller.MyController', {
        ///     views: [
        ///         'MyView1',              // -> 'MyApp.view.MyView1'
        ///         'sub.View2',            // -> 'MyApp.view.sub.View2'
        ///         'MyApp.view.MyView3'    // -> 'MyApp.view.MyView3'
        ///     ]
        ///     // ...
        /// });
        /// </remarks>
        /// <returns></returns>
        protected virtual string GetFullClassName(string module, string className)
        {
            string ns; // namespace
            if (this.IsApplication)
            {
                if (String.IsNullOrEmpty(ApplicationName))
                {
                    System.Diagnostics.Debug.WriteLine("Cannot find application name");
                    return className;
                }
                else
                {
                    ns = ApplicationName;
                }
            }
            else
            {
                ns = this.ClassName.Split('.').First();
            }

            // Check if className already full
            if (className.StartsWith(ns + '.'))
            {
                return className;
            }
            else
            {
                return String.Format("{0}.{1}.{2}", ns, module, className);
            }
        }

        /// <summary>
        /// Fills the Dependencies collection using DependencyClasses
        /// </summary>
        /// <param name="allFiles">All files in the bundle</param>
        public virtual void FillDependencies(IEnumerable<SenchaFileNode> allFiles)
        {
            Dependencies = allFiles.Where(f => this.DependencyClasses.Any(dc => dc == f.ClassName));
        }

        /// <summary>
        /// Debug
        /// </summary>
        public override string ToString()
        {
            return this.ClassName ?? this.ApplicationName;
        }
    }
}