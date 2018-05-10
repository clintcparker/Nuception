//------------------------------------------------------------------------------
// <copyright file="PrepareForNuGetCommand.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace NuceptionEx
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class PrepareForNuGetCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("818570e4-4283-4a1d-9d60-0abd4d9d86e4");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="PrepareForNuGetCommand"/> class. Adds our
        /// command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private PrepareForNuGetCommand(Package package)
        {
            this.package = package ?? throw new ArgumentNullException("package");

            if (this.ServiceProvider.GetService(typeof(IMenuCommandService)) is OleMenuCommandService commandService)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PrepareForNuGetCommand Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new PrepareForNuGetCommand(package);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">     Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            var dte = (DTE2)ServiceProvider.GetService(typeof(DTE));

            UpdateProject(dte);
        }

        public static void UpdateProject(DTE2 dte)
        {
            var selectedItems = (Array)dte.ToolWindows.SolutionExplorer.SelectedItems;

            var a = selectedItems.Cast<UIHierarchyItem>().First(x => x.IsSelected);

            var n = a.Name;

            var project = dte.Solution.Projects.Cast<Project>().First(x => x.Name == n);

            var loc = Assembly.GetExecutingAssembly().Location;
            var assemblyDirectory = Path.GetDirectoryName(loc);
            var nugetResourcesPath = Path.Combine(assemblyDirectory, "Resources\\nuget_resources");

            //Add tools & content
            var dirs = Directory.EnumerateDirectories(nugetResourcesPath);

            foreach(var dir in dirs)
            {
                var newDir = AddDirectoryToProject(project, dir);
                if (newDir != null)
                {
                    MarkDirectoryAsCopyToOutputRecursive(newDir);
                }
            }
            
            //Add nuspec

            var nuspecPath = Directory.GetFiles(nugetResourcesPath).First(x => x.Contains("nuspec"));
            var name = $"{project.Name}.nuspec";
            ProjectItem nuspec;
            try
            {
                nuspec = project.ProjectItems.AddFromFileCopy(nuspecPath);
                try
                {
                    nuspec.Name = name;

                    nuspec.Open();
                    nuspec.Document.ReplaceText("$assemblyname$", project.Properties.Item("AssemblyName").Value.ToString());
                    MarkFileAsCopyToOutputDirectory(nuspec);
                    nuspec.Save();
                    CopyXMLToOutput(project);
                }
                catch (System.Runtime.InteropServices.COMException e)
                {
                    if (e.Message.Contains($"{name}' already exists") && nuspec != null)
                    {
                        nuspec.Delete();
                    }
                    else
                    {
                        throw;
                    }

                }
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                if(!e.Message.Contains("already exists"))
                {
                    throw;
                }
            }

            
            project.Save();
        }

        private static ProjectItem AddDirectoryToProject(Project project, string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return null;
            }
            ProjectItem projectDir;

            var shortDir = Path.GetFileName(directoryPath);
            //TODO fix this
            try
            {
            projectDir = project.ProjectItems.AddFolder(shortDir);

            }catch (System.Runtime.InteropServices.COMException e)
            {
                 if (e.Message.Contains($"{shortDir}' already exists"))
                {
                    return null;
                }else
                {
                    throw;
                }
            }
            // add directories, files & recurse
            AddChildrenToProjectFolder(projectDir, directoryPath);
            return projectDir;
        }

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

        private static void MarkDirectoryAsCopyToOutputRecursive(ProjectItem item)
        {
            foreach (ProjectItem projectItem in item.ProjectItems)
            {
                MarkFileAsCopyToOutputDirectory(projectItem);
            }
        }

        private static void MarkFileAsCopyToOutputDirectory(ProjectItem item)
        {
            try
            {
                //Write - Host Try set $item.Name
                item.Properties.Item("CopyToOutputDirectory").Value = 1;
            }
            catch
            {
                //Write - Host RecurseOn $item.Name
                MarkDirectoryAsCopyToOutputRecursive(item);
            }
        }

        private static void CopyXMLToOutput(Project project)
        {
            project.ConfigurationManager.Cast<Configuration>()
                .First(x => x.ConfigurationName.Equals("Release", StringComparison.InvariantCultureIgnoreCase))
                .Properties.Cast<Property>()
                .First(x => x.Name.Equals("DocumentationFile"))
                .Value = $"bin\\Release\\{project.Properties.Item("AssemblyName").Value.ToString()}.xml";
        }
    }
}
