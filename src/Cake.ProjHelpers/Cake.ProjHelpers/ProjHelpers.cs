using Cake.Core;
using Cake.Core.Annotations;
using System.IO;
using System.Linq;
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
        /// <param name="files">Cake.Core.IO.FilePathCollection of file paths to add</param>
        /// <param name="projectFile">Cake.Core.IO.FilePath to the VS project file to modify</param>
        /// <returns>Nothing.</returns>
        [CakeMethodAlias]
        public static void AddEmbeddedResources(this ICakeContext context, Cake.Core.IO.FilePathCollection files, Cake.Core.IO.FilePath projectFile)
        {
            AddFilesToProjectAsEmbeddedResources(files.Select(f=> f.FullPath).ToArray(), projectFile.FullPath.ToString());
        }


        /// <summary>
        /// Adds absolute/relative file paths into a .proj as embed resource with relative paths
        /// </summary>
        /// <param name="files">Cake.Core.IO.FilePathCollection of file paths to add</param>
        /// <param name="projectFile">String path to VS project file to modify</param>
        /// <returns>Nothing.</returns>
        [CakeMethodAlias]
        public static void AddEmbeddedResources(this ICakeContext context, Cake.Core.IO.FilePathCollection files, string projectFile)
        {
            AddFilesToProjectAsEmbeddedResources(files.Select(f => f.FullPath).ToArray(), projectFile);
        }

        /// <summary>
        /// Assuming files in project folder, adds absolute/relative file paths into a .proj file
        /// as embed resource (with relative paths to the project file)
        /// </summary> 
        /// <param name="files">String array of file paths to add</param>
        /// <param name="projectFile">String path to VS project file to modify</param>
        /// <returns>Nothing.</returns>
        [CakeMethodAlias]
        public static void AddEmbeddedResources(this ICakeContext context, string[] files, string projectFile)
        {
            AddFilesToProjectAsEmbeddedResources(files, projectFile);
        }

        /// <summary>
        ///Loads the project file and finds the last item group in its contents
        ///Then checks each file in the array for an existing entry in the project file
        ///Assuming a matched file name
        ///If the entry exists in the project file but not on disk the reference is removed
        ///a new entry is added to the item group
        /// </summary> 
        /// <param name="files">String array of file paths to add</param>
        /// <param name="projectFilePath">String path to VS project file to modify</param>
        /// <returns>Nothing.</returns>
        public static void AddFilesToProjectAsEmbeddedResources(string[] files, string projectFilePath)
        {  
            var ProjectFile = XDocument.Load(projectFilePath);

            var itemGroup = GetLastItemGroup(ProjectFile);
             
            foreach (var file in files)
            {
                //Make a path relative from the file specified. 
                var relativePath = new Cake.Core.IO.FilePath(System.IO.Path.GetFullPath(projectFilePath))
                    .GetRelativePath(new Core.IO.FilePath(System.IO.Path.GetFullPath(file)))
                    .ToString().Replace('/', Path.DirectorySeparatorChar);

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
        


    }
}
