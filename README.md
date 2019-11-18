
# Cake VS Proj Helpers


#### AddEmbeddedResources - Embeds files into a .vbproj or .csproj file as embedded resource.

Default options
In this example, the default options are used to add all files using Directory.GetFiles as embedded resource. 
The directory structure used is described below.

        +-- MyProject.csproj
        +-- to_embed
            ¦   +-- rootpage.html
            +-- resources
            ¦   +-- page1.html
 

#### Resulting items
      <ItemGroup>
        <EmbeddedResource Include="to_embed\rootpage.html"/>
        <EmbeddedResource Include="to_embed\resources\page1.html"/>
      </ItemGroup>

 NOTE:  Loads the project file and finds the last item group in its contents
        Then checks each file in the array for an existing entry in the project file
        Assuming a matched file name
		If the entry exists in the project file but not on disk the reference is removed
		a new entry is added to the item group

#### Example Task
 
```sh
Task("Embed-FrontEnd-Files")
    .Does(() =>
{
    var prjRoot = @"./src/";
    
    var files = GetFiles(prjRoot + @"to_embed/**/*");

    AddEmbeddedResources(files,
       new FilePath(prjRoot + "ServiceHost.csproj")
    );

});
```

