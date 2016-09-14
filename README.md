# NuceptionEx
A NuGet (and now a VS extension) package to help make NuGet packages.



## Why
NuGetter is awesome. But it's not very clear on how to use NuGetter and take adavntage of all the features of NuGet.

## What
Nuception creates all the fun parts of a NuGet package:

* PowerShell scripts
    * Init.ps1
    * Install.ps1
    * Uninstall.ps1
* Config transforms
    * web
    * App
* .nuspec file


All of these are set to copy to output directory on install.

## More

* The nuspec is populated with the appropriate assembly info, including XML docs.
* XML documentation is automatically set to publish for the Release build.
* Nuception is a development dependency, so it wont be included in downstream packages.