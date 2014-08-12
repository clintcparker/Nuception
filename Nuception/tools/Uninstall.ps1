param($installPath, $toolsPath, $package, $project)

 $project.ProjectItems.Item("nuget_content").ProjectItems | Foreach-Object {
	Write-Host $_.Name
	if ($_.Name -match "xdt"){
		$_.Name = ($_.Name.Replace("xdt","nugetxdt"))
	}
}


$project.ProjectItems | Foreach-Object { if ($_.Name -match $($project.ProjectName + ".nuspec")){$_.Name = ($_.Name.Replace($project.ProjectName,"Nuception_Template"))}}