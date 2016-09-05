using Cake.Core;
using Cake.Core.Annotations;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Cake.ProjHelpers
{


    /// <summary>
    /// Helpers to ease manipulation of vs proj files. 
    /// </summary>
    [CakeAliasCategory("VS .proj file helpers")]
    public static class ProjHelpers
    {
        /// <summary>
        /// Adds absolute/relative file paths into a .proj as embed resource with relative paths
        /// </summary>
        [CakeMethodAlias]
        public static void AddEmbeddedResources(this ICakeContext context, string[] files, string projectFile)
        {
            AddFilesToProjectAsEmbeddedResources(files, projectFile);
        }

        /// <summary>
        /// Adds absolute/relative file paths into a .proj as embed resource with relative paths
        /// </summary>
        public static void AddFilesToProjectAsEmbeddedResources(string[] files, string projectFilePath)
        {

            var ProjectFile = XDocument.Load(projectFilePath);

            var itemGroup = GetLastItemGroup(ProjectFile);

            foreach (var file in files)
            {
                //Add the resources from the files specified.

                var relativePath = System.IO.Path.IsPathRooted(file) ? 
                    GetRelativePath(file, System.IO.Path.GetDirectoryName(projectFilePath)) : 
                    file;

                if (!EmbeddedResourceExists(relativePath, ProjectFile))
                {
                    var xelem = AddEmbeddedResource(relativePath, ProjectFile);
                    itemGroup.Add(xelem);
                }

            }
            ProjectFile.Save(projectFilePath);
        }

        private static XElement GetLastItemGroup(XDocument doc)
        {
            var itemGroup = doc.Nodes().OfType<XElement>()
                              .DescendantNodes().OfType<XElement>()
                              .LastOrDefault(xy => xy.Name.LocalName == "ItemGroup");

            if (itemGroup == null)
            {
                XNamespace rootNamespace = doc.Root.Name.NamespaceName;

                var node = new XElement(rootNamespace + "ItemGroup");

                doc.Nodes().OfType<XElement>()
                   .First(n => n.Name.LocalName == "Project")
                   .Add(node);

                return node;
            }
            return itemGroup;
        }

        private static XElement AddEmbeddedResource(string pathToAdd, XDocument doc)
        {
            XNamespace rootNamespace = doc.Root.Name.NamespaceName;
            var xelem = new XElement(rootNamespace + "EmbeddedResource");
            xelem.Add(new XAttribute("Include", pathToAdd));
            return xelem;
        }

        private static bool EmbeddedResourceExists(string relativePath, XDocument doc)
        {
            var existingRef = doc.DescendantNodes().OfType<XElement>()
                .FirstOrDefault(n => n.Name.LocalName == "EmbeddedResource" &&
                Path.GetFileName(n.Attribute("Include").Value) == Path.GetFileName(relativePath));

            if (existingRef != null && !File.Exists(existingRef.Attribute("Include").Value))
            {
                existingRef.Remove();
                return false;
            }

            return existingRef != null;
        }

        private static string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }



    }
}
