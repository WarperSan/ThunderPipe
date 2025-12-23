<br>
<div align="center">
    <img alt="Logo" src="https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/icon.png" height="128"/>
    <h1>ThunderPipe</h1>
    <div>
        <a href="https://www.nuget.org/packages/WarperSan.ThunderPipe"><img alt="NuGet Badge" src="https://img.shields.io/nuget/v/WarperSan.ThunderPipe.svg"></a>
        <a href="https://www.nuget.org/packages/WarperSan.ThunderPipe"><img alt="NuGet Version" src="https://img.shields.io/nuget/dt/WarperSan.ThunderPipe.svg"></a>
        <a href="https://raw.githubusercontent.com/WarperSan/ThunderPipe/master/LICENSE"><img alt="License: GNU Affero" src="https://img.shields.io/badge/License-GNU Affero-purple.svg"></a>
    </div>
</div>

ThunderPipe is a command-line tool for building, validating and publishing mod packages to [Thunderstore](https://thunderstore.io/).

## Why this instead of TCLI?

[Thunderstore CLI](https://github.com/thunderstore-io/thunderstore-cli) has the advantages that it is made and maintained by the organization handling Thunderstore. However, I've found that it has issues that this tool tries to fix:

> [!WARNING]  
> I am **not blaming nor shaming** the developers of TCLI. I believe both have pros and cons, and if you are using this tool as a way to "flex" on other devs, **be ashamed of yourself**.

<details>
    <summary>TCLI is not <i>for</i> CLI only</summary>
    <p>
        As of v0.2.4, TCLI tries to be a mod installer, manager and publisher. It is useful <b>if</b> you want all of these features. However, most people use mod managers like <i>Gale</i>. Personally, I never found an use to have a lot of features that are not used.
    </p>
</details>
<details>
    <summary>TCLI offers no easy workflow</summary>
    <p>
        I am aware that this is a gripe. The wiki even mentions that they do not have any official workflow that developers can use. However, this tool tries to offer an easy-to-use tool and workflow so people can automatically or manually upload mods without much issue.
    </p>
</details>

## Installation

You can install this tool using:

```bash
dotnet tool install ThunderPipe
```

This will allow you to run it with `dotnet tool run ThunderPipe`.

## Usage

You can find all the commands using `ThunderPipe --help`.

### Validating

Using the command `validate`, you are able to validate if your package will be allowed on the Thunderstore servers, even before publishing it:

```
DESCRIPTION:
Validates a package

USAGE:
    ThunderPipe validate <package-folder> [OPTIONS]

ARGUMENTS:
    <package-folder>    Folder containg the package's files

OPTIONS:
                           DEFAULT                                                                      
    -h, --help                                        Prints help information                           
        --token                                       Authentication token used to publish the package. Required
        --icon             ./icon.png                 Path from the package folder to the icon file     
        --manifest         ./manifest.json            Path from the package folder to the manifest file 
        --author                                      Name of the author that would publish the package 
        --readme           ./README.md                Path from the package folder to the README file   
        --disable-local                               Determines if local validation will be ignored    
        --enable-remote                               Determines if remote validation rules will be used
        --repository       https://thunderstore.io    URL of the server hosting the package 
```

### Publishing

Using the command `publish`, you are able to publish your package to the Thunderstore server:

```
DESCRIPTION:
Publish a package to Thunderstore

USAGE:
    ThunderPipe publish <file> <team> <community> [OPTIONS]

ARGUMENTS:
    <file>         Path to the package file to publish   
    <team>         Team to publish the package for       
    <community>    Community where to publish the package

OPTIONS:
                                 DEFAULT                                                                              
    -h, --help                                              Prints help information                                   
        --token                                             Authentication token used to publish the package. Required
        --repository             https://thunderstore.io    URL of the server hosting the package                     
        --categories <VALUES>                               Categories used to label this package                     
        --has-nsfw                                          Determines if this package has NSFW content
```