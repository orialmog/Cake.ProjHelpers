
# Cake VS Proj Helpers

#### AddEmbeddedResources - Embeds files into a .vbproj or .csproj file as embedded resource.

Default options
In this example, the default options are used to add all files using Directory.GetFiles as embedded resource. 
The directory structure used is described below.

```sh
AddEmbeddedResources(Directory.GetFiles(@"./to_embed/", @"\*.*", SearchOption.AllDirectories), @"./MyProject.csproj");
```
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

 NOTE: When the files are already found in the project as EmbeddedResources a new entry wont be added.
 Embeds files into a .vbproj or .csproj file as embedded resource.



