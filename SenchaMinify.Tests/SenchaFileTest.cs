using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SenchaMinify.Tests
{
    [TestClass]
    public class SenchaFileTest
    {
        [TestMethod]
        public void Define_Single_Class()
        {
            var f = new SenchaFile("Ext.define('App.Class1')");

            var classesCount = f.Classes.Count();
            Assert.AreEqual(1, classesCount, "Classes count");

            var className = f.Classes.Single().ClassName;
            Assert.AreEqual("App.Class1", className);
        }

        [TestMethod]
        public void Define_Multiple_Classes()
        {
            var f = new SenchaFile(
                "Ext.define('App.Class1')" + Environment.NewLine +
                "Ext.define('App.Class2')"
            );

            var classesCount = f.Classes.Count();
            Assert.AreEqual(2, classesCount, "Classes count");

            var firstClassName = f.Classes.First().ClassName;
            var secondClassName = f.Classes.Skip(1).First().ClassName;
            Assert.AreEqual("App.Class1", firstClassName);
            Assert.AreEqual("App.Class2", secondClassName);
        }

        [TestMethod]
        public void Get_Dependencies()
        {
            var appFile = new SenchaFile(@"
Ext.application({
    name: 'App',
    controllers: [
        'App.controller.Main'
    ]
})
");
            var controllerFile = new SenchaFile(@"
Ext.define('App.controller.Main', {
    extend: 'Ext.app.Controller',
    views: [
        'App.view.UserForm'
    ]
})
");

            var viewFile = new SenchaFile(@"
Ext.define('App.view.UserForm', {
    extend: 'Ext.form.FormPanel'
})
");

            var files = new[] 
            {
                appFile,
                controllerFile,
                viewFile
            };

            appFile.FillDependencies(files);
            controllerFile.FillDependencies(files);
            viewFile.FillDependencies(files);

            Assert.AreEqual(controllerFile, appFile.Dependencies.Single());
            Assert.AreEqual(viewFile, controllerFile.Dependencies.Single());
            Assert.IsFalse(viewFile.Dependencies.Any());
        }

        [TestMethod]
        public void Order_Files()
        {
            var appFile = new SenchaFile(@"
Ext.application({
    name: 'App',
    controllers: [
        'App.controller.Main'
    ]
})
");
            var controllerFile = new SenchaFile(@"
Ext.define('App.controller.Main', {
    extend: 'Ext.app.Controller',
    views: [
        'App.view.UserForm'
    ]
})
");

            var viewFile = new SenchaFile(@"
Ext.define('App.view.UserForm', {
    extend: 'Ext.form.FormPanel'
})
");

            var files = new[] 
            {
                appFile,
                controllerFile,
                viewFile
            };

            var orderer = new SenchaOrderer();
            var orderedFiles = orderer.OrderFiles(files);

            Assert.AreEqual(viewFile, orderedFiles.Skip(0).First(), "viewFile");
            Assert.AreEqual(controllerFile, orderedFiles.Skip(1).First(), "controllerFile");
            Assert.AreEqual(appFile, orderedFiles.Skip(2).First(), "appFile");
        }
    }
}
