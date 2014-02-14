using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Ajax.Utilities;

namespace SenchaMinify
{
    public class SenchaClass
    {
        /// <summary>
        /// Defined class name
        /// </summary>
        public string ClassName { get; set; }

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
        public bool IsApplication { get; set; }

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
        /// Конфигурационный блок
        /// </summary>
        ObjectLiteral ConfigNode { get; set; }

        public SenchaClass (ObjectLiteral configNode)
        {
            this.ConfigNode = configNode;
        }

        /// <summary>
        /// Get dependency class names
        /// </summary>
        /// <param name="propertyName">Configuration property name where dependencies are located</param>
        /// <returns></returns>
        protected virtual IEnumerable<string> GetDependencyClasses(string propertyName)
        {
            var property = ConfigNode.Properties.OfType<ObjectLiteralProperty>()
                .Where(p => p.Name.Name == propertyName)
                .FirstOrDefault();

            if (property == null)
            {
                yield break;
            }
            else if (property.Value is ArrayLiteral)
            {
                var arr = property.Value as ArrayLiteral;
                foreach (var nodes in arr.Children.OfType<AstNodeList>())
                {
                    foreach (var node in nodes.OfType<ConstantWrapper>())
                    {
                        yield return node.Value.ToString();
                    }
                }
            }
            else if (property.Value is ConstantWrapper)
            {
                yield return (property.Value as ConstantWrapper).ToString();
            }
            else
            {
                yield break;
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
        /// Get the value of 'name' config. Used for application.
        /// </summary>
        /// <returns>Application name if search was successfull, otherwise null</returns>
        protected virtual string GetApplicationName()
        {
            var result = ConfigNode.Properties.OfType<ObjectLiteralProperty>()
                .Where(p => p.Name.Name == "name")
                .Select(p => p.Value.ToString())
                .FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Get the value of 'autoCreateViewport' propetry. Used for application.
        /// </summary>
        /// <returns>Value of 'autoCreateViewport' property is true, or false if property not found.</returns>
        protected virtual bool GetAutoCreateViewport()
        {
            var result = ConfigNode.Properties.OfType<ObjectLiteralProperty>()
                .Where(p => p.Name.Name == "autoCreateViewport")
                .Select(p => p.Value)
                .Select(val => val.Context.Code == "true") // very sensitive :(
                .FirstOrDefault();

            return result;
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
        /// Debug
        /// </summary>
        public override string ToString()
        {
            return this.ClassName ?? this.ApplicationName;
        }
    }
}
