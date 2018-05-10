//------------------------------------------------------------------------------
// <copyright file="PrepareForNuGetCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.IO;
using EnvDTE;
/*
 #credit goes to James Roland (http://stackoverflow.com/users/413529/james-roland)
*/

public static class Extensions
{
    private static void AddChildrenToProjectFolder(ProjectItem projectItem, string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
        {
            return;
        }
        // add directories & recurse
        var childDirs = Directory.EnumerateDirectories(directoryPath);

        foreach (var childDir in childDirs)
        {
            var pi = projectItem.ProjectItems.AddFolder(Path.GetFileName(childDir));
            AddChildrenToProjectFolder(pi, childDir);
        }

        //add files
        var childFiles = Directory.EnumerateFiles(directoryPath);
        foreach (var childFile in childFiles)
        {
            projectItem.ProjectItems.AddFromFileCopy(childFile);
        }
    }
}