#credit goes to James Roland (http://stackoverflow.com/users/413529/james-roland)
param($installPath, $toolsPath, $package, $project)

function MarkDirectoryAsCopyToOutputRecursive($item)
{
    $item.ProjectItems | ForEach-Object { MarkFileASCopyToOutputDirectory($_) }
}

function MarkFileASCopyToOutputDirectory($item)
{
    Try
    {
        Write-Host Try set $item.Name
        $item.Properties.Item("CopyToOutputDirectory").Value = 1
    }
    Catch
    {
        Write-Host RecurseOn $item.Name
        MarkDirectoryAsCopyToOutputRecursive($item)
    }
}



#Now mark everything in the a directories as "Copy always"
MarkDirectoryAsCopyToOutputRecursive($project.ProjectItems.Item("nuget_content"))
MarkDirectoryAsCopyToOutputRecursive($project.ProjectItems.Item("nuget_tools"))

#rename the xdts (they can't be "xdt" because NuGet will execute them
$project.ProjectItems.Item("nuget_content").ProjectItems | Foreach-Object { 
	if ($_.Name -match "nugetxdt"){
		$_.Name = ($_.Name.Replace("nugetxdt","xdt"))
	}
}

#copy XML to output
$project.ConfigurationManager | Foreach-Object {
	if ($_.ConfigurationName -match "Release"){ 
		$_.Properties.Item("DocumentationFile").Value = $("bin\Release\" + $project.ProjectName + ".xml")
	}
}

#rename the nuspec file
$project.ProjectItems | Foreach-Object { 
	if ($_.Name -match "Nuception_Template.nuspec"){ 
		$_.Name = ($_.Name.Replace("Nuception_Template",$project.ProjectName));
		MarkFileASCopyToOutputDirectory($_);
	}
}


$dte.ItemOperations.OpenFile($toolsPath + '\README.Nuception.txt')