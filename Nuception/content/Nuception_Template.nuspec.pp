﻿<?xml version="1.0"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
	<id>$assemblyname$</id>
	<version>1.0.0</version>
	<authors>NAME HERE</authors>
	<owners>OWNER HERE</owners>
	<licenseUrl>http://LICENSE_URL_HERE_OR_DELETE_THIS_LINE</licenseUrl>
	<projectUrl>http://PROJECT_URL_HERE_OR_DELETE_THIS_LINE</projectUrl>
	<iconUrl>http://ICON_URL_HERE_OR_DELETE_THIS_LINE</iconUrl>
	<requireLicenseAcceptance>false</requireLicenseAcceptance>
	<description>DESCRIPTION HERE</description>
	<releaseNotes>RELEASE NOTES</releaseNotes>
	<copyright>Copyright 2014</copyright>
	<tags></tags>
	<dependencies>
	  <!--<dependency id="SampleDependency" version="1.0" />-->
	</dependencies>
  </metadata>
  <files>
	<!--Projects-->
	<file src="$assemblyname$.dll" target="lib/net45" />
	  <file src="$assemblyname$.xml" target="lib/net45" />
	<!--Scripts & Transforms & Files-->
	<file src="nuget_tools\**\*.*" target="tools" exclude="**\*.log" />
	<file src="nuget_content\**\*.*" target="content" />
  </files>
</package>