using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Cake.ProjEmbedResources.Tests.Resources;

namespace Cake.ProjEmbedResources.Tests
{
    [TestClass]
    public class TaskTests
    {

        [TestInitialize]
        public void Init()
        {
            File.WriteAllText(@".\Sample.proj", StringResources.EmptyCSConsoleProject);
            File.Delete(@"Resources\to_embed\sub\stuff_one.js");
        }

        [TestCleanup]
        public void Distroy()
        {
            File.Delete(@".\Sample.proj");
            File.Delete(@"Resources\to_embed\sub\stuff_one.js");
        }

        [TestMethod]
        public void CanWriteFilesToXml()
        {
            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\",@"*.*", SearchOption.AllDirectories), @".\Sample.proj");

            Assert.AreEqual(File.ReadAllText(@".\Sample.proj"), StringResources.PopulatedCSConsoleProject)
;
        }
        [TestMethod]
        public void ReRunDoesNotWriteExtraFilesToXml()
        {
            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\", @"*.*", SearchOption.AllDirectories), @".\Sample.proj");
            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\", @"*.*", SearchOption.AllDirectories), @".\Sample.proj");
            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\", @"*.*", SearchOption.AllDirectories), @".\Sample.proj");
              
            Assert.AreEqual(File.ReadAllText(@".\Sample.proj"), StringResources.PopulatedCSConsoleProject);
        }


        [TestMethod]
        public void MovingFilesDoesNotWriteExtraFilesToXml()
        {
            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\", @"*.*", SearchOption.AllDirectories), @".\Sample.proj");

            File.Move(@"Resources\to_embed\stuff_one.js", @"Resources\to_embed\sub\stuff_one.js");
            File.Delete(@"Resources\to_embed\stuff_one.js");

            ProjEmbedResources.AddFilesToProject(Directory.GetFiles(@"Resources\to_embed\", @"*.*", SearchOption.AllDirectories), @".\Sample.proj");
              
            Assert.AreEqual(File.ReadAllText(@".\Sample.proj"), StringResources.MovedFileCSConsoleProject);

        }

    }
}
