# ThunderPipe

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
