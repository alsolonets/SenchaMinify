using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenchaMinify.Tests
{
    [TestClass]
    public class SenchaClassTest
    {
        [TestMethod]
        public void Create_Application()
        {
            var f = new SenchaFile("Ext.application({ name: 'App' })");
            var app = f.Classes.Single();
            Assert.AreEqual("App", app.ApplicationName);
        }

        [TestMethod]
        public void Application_With_AutoCreateViewport_True()
        {
            var f = new SenchaFile(@"
Ext.application({ 
    name: 'App',
    autoCreateViewport: true
})
");
            var app = f.Classes.Single();
            Assert.AreEqual("true", app.AutoCreateViewport, "autoCreateViewport value");

            var viewportFullName = app.DependencyClassNames.Single();
            Assert.AreEqual("App.view.Viewport", viewportFullName, "Dependency's viewport class name");
        }

        [TestMethod]
        public void Application_With_AutoCreateViewport_False()
        {
            var f = new SenchaFile(@"
Ext.application({ 
    name: 'App',
    autoCreateViewport: false
})
");
            var app = f.Classes.Single();
            Assert.AreEqual("false", app.AutoCreateViewport, "autoCreateViewport value");

            var dependenciesCount = app.DependencyClassNames.Count();
            Assert.AreEqual(0, dependenciesCount, "Dependencies count");
        }

        [TestMethod]
        public void Application_With_AutoCreateViewport_Undefined()
        {
            var f = new SenchaFile(@"
Ext.application({ 
    name: 'App'
})
");
            var app = f.Classes.Single();
            Assert.IsNull(app.AutoCreateViewport, "autoCreateViewport value");

            var dependenciesCount = app.DependencyClassNames.Count();
            Assert.AreEqual(0, dependenciesCount, "Dependencies count");
        }

        [TestMethod]
        public void Application_With_AutoCreateViewport_Name()
        {
            var f = new SenchaFile(@"
Ext.application({ 
    name: 'App',
    autoCreateViewport: 'App.view.MyViewport'
})
");
            var app = f.Classes.Single();
            Assert.AreEqual("App.view.MyViewport", app.AutoCreateViewport, "autoCreateViewport value");

            var viewportFullName = app.DependencyClassNames.Single();
            Assert.AreEqual("App.view.MyViewport", viewportFullName, "Dependency's viewport class name");
        }

        [TestMethod]
        public void Extend_Class()
        {
            var file = new SenchaFile(@"
Ext.define('App.SubClass', {
    extend: 'App.Class'
});
");
            var cls = file.Classes.Single();
            var baseClassName = cls.DependencyClassNames.First();

            Assert.AreEqual("App.Class", baseClassName, "Base class name");
        }

        [TestMethod]
        public void Override_Class()
        {
            var file = new SenchaFile(@"
Ext.define('App.ClassOverride', {
    override: 'App.Class'
});
");
            var cls = file.Classes.Single();
            var baseClassName = cls.DependencyClassNames.First();

            Assert.AreEqual("App.Class", baseClassName, "Base class name");
        }

        [TestMethod]
        public void Add_Mixins_As_Class_Names()
        {
            var file = new SenchaFile(@"
Ext.define('App.Class', {
    mixins: [
        'Ext.mixin.Observable', 
        'Ext.mixin.Responsive'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "Ext.mixin.Observable", 
                    "Ext.mixin.Responsive"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Add_Mixins_As_Object()
        {
            var file = new SenchaFile(@"
Ext.define('App.Class', {
    mixins: {
        observable: 'Ext.mixin.Observable', 
        responsive: 'Ext.mixin.Responsive'
    }
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "Ext.mixin.Observable", 
                    "Ext.mixin.Responsive"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Add_Required_Class_Names()
        {
            var file = new SenchaFile(@"
Ext.define('App.Class', {
    requires: [
        'App.Class2', 
        'App.Class3'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.Class2", 
                    "App.Class3"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Store_With_A_Model_Full()
        {
            var file = new SenchaFile(@"
Ext.define('App.store.MyStore', {
    model: 'App.model.MyModel'
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.model.MyModel"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Store_With_A_Model_Short()
        {
            var file = new SenchaFile(@"
Ext.define('App.store.MyStore', {
    model: 'MyModel'
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "MyModel"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Application_With_Controllers()
        {
            var file = new SenchaFile(@"
Ext.application({
    name: 'App',
    controllers: [
        'App.controller.Controller1',
        'Controller2'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.controller.Controller1",
                    "App.controller.Controller2"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Controller_With_Models()
        {
            var file = new SenchaFile(@"
Ext.define('App.controller.Controller', {
    models: [
        'App.model.Model1',
        'Model2'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.model.Model1",
                    "App.model.Model2"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Controller_With_Views()
        {
            var file = new SenchaFile(@"
Ext.define('App.controller.Controller', {
    views: [
        'App.view.View1',
        'View2'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.view.View1",
                    "App.view.View2"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }

        [TestMethod]
        public void Define_Controller_With_Stores()
        {
            var file = new SenchaFile(@"
Ext.define('App.controller.Controller', {
    stores: [
        'App.store.Store1',
        'Store2'
    ]
});
");
            var cls = file.Classes.Single();
            var mixinDependencies = cls.DependencyClassNames;
            var areSame = !mixinDependencies
                .Except(new[] 
                { 
                    "App.store.Store1",
                    "App.store.Store2"
                })
                .Any();

            Assert.IsTrue(areSame, "Dependencies are same");
        }
    }
}
