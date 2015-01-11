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
        public string AutoCreateViewport
        {
            get
            {
                if (String.IsNullOrEmpty(_AutoCreateViewport))
                {
                    _AutoCreateViewport = GetAutoCreateViewport();
                }
                return _AutoCreateViewport;
            }
        }
        private string _AutoCreateViewport;

        /// <summary>
        /// Gets a collection of dependency class names for this source file
        /// </summary>
        public IEnumerable<string> DependencyClassNames
        {
            get
            {
                return _DependencyClassNames ?? (_DependencyClassNames = GetDependencyClassNames());
            }
        }
        private IEnumerable<string> _DependencyClassNames;

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
                    "override",
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

        public SenchaClass(ObjectLiteral configNode)
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
            if (ConfigNode == null)
            {
                yield break;
            }

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
            else if (property.Value is ObjectLiteral)
            {
                /**
                 * Exclusively for mixins defined as object:
                 * mixins: {
                 *      observable: 'Ext.mixin.Observable',
                 *      responsive: 'Ext.mixin.Responsive'
                 * }
                 */
                var obj = property.Value as ObjectLiteral;
                foreach (var prop in obj.Properties.OfType<ObjectLiteralProperty>().Where(p => p.IsConstant))
                {
                    yield return prop.Value.ToString();
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
        protected virtual IEnumerable<string> GetDependencyClassNames()
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

            if (this.IsApplication && !String.IsNullOrEmpty(this.AutoCreateViewport))
            {
                if (this.AutoCreateViewport == "true") 
                {
                    yield return GetFullClassName("view", "Viewport");
                }
                else if (this.AutoCreateViewport != "false")
                {
                    yield return this.AutoCreateViewport;
                }
            }
        }

        /// <summary>
        /// Get the value of 'name' config. Used for application.
        /// </summary>
        /// <returns>Application name if search was successfull, otherwise null</returns>
        protected virtual string GetApplicationName()
        {
            if (ConfigNode == null)
            {
                return null;
            }

            var result = ConfigNode.Properties.OfType<ObjectLiteralProperty>()
                .Where(p => p.Name.Name == "name")
                .Select(p => p.Value.ToString())
                .FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Get the value of 'autoCreateViewport' propetry. Used for application.
        /// </summary>
        /// <returns>Value of 'autoCreateViewport' property, or null if property not found.</returns>
        protected virtual string GetAutoCreateViewport()
        {
            if (ConfigNode == null)
            {
                return null;
            }

            var value = ConfigNode.Properties.OfType<ObjectLiteralProperty>()
                .Where(p => p.Name.Name == "autoCreateViewport")
                .Select(p => p.Value)
                .FirstOrDefault();

            string result = null;

            if (value is ConstantWrapper)
            {
                result = ((ConstantWrapper)value).Value.ToString();
            }
            else if (value != null)
            {
                result = value.Context.Code;
            }

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
        ///         'MyView1',                      // -> 'MyApp.view.MyView1'
        ///         'sub.MyView2',                  // -> 'MyApp.view.sub.MyView2'
        ///         'MyApp.view.MyView3',           // -> 'MyApp.view.MyView3'
        ///         'MyApp.SubApp.view.MyView4',    // -> 'MyApp.SubApp.view.MyView4'
        ///         'MyApp.view.sub.MyView5',       // -> 'MyApp.view.sub.MyView5'
        ///         'OtherApp.view.View6',          // -> 'OtherApp.view.View6'
        ///     ]
        ///     // ...
        /// });
        /// </remarks>
        /// <returns></returns>
        protected virtual string GetFullClassName(string module, string className)
        {
            string ns; // this class' namespace
            if (this.IsApplication)
            {
                if (String.IsNullOrEmpty(ApplicationName))
                {
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

            if (className.StartsWith(ns + '.'))
            {
                // className is already full and has same namespace ('MyApp.view.MyView3', ...)
                return className;
            }
            else if (className.IndexOf('.' + module + '.') > 0)
            {
                // className is already full and located in another namespace ('OtherApp.view.View6')
                return className;
            }
            else
            {
                // className is short ('MyView1', 'sub.MyView2'). Prepending the namespace.
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
