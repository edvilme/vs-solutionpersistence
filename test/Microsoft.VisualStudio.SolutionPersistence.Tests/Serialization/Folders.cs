﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Serialization;

/// <summary>
/// Tests related to manipulating solution folders in the model.
/// </summary>
public sealed class Folders
{
    // Remove a folder from the solution model.
    [Fact]
    public void RemoveFolder()
    {
        // Create a solution with a deep folder structure and projects.
        SolutionModel solution = new SolutionModel();
        SolutionFolderModel folderThis = solution.AddFolder("/This/");
        SolutionFolderModel folderIs = solution.AddFolder("/This/Is/");
        SolutionFolderModel folderA = solution.AddFolder("/This/Is/A/");
        SolutionFolderModel folderNested = solution.AddFolder("/This/Is/A/Nested/");
        SolutionFolderModel folderFolder = solution.AddFolder("/This/Is/A/Nested/Folder/");

        Assert.NotNull(folderThis);
        Assert.NotNull(folderIs);
        Assert.NotNull(folderA);
        Assert.NotNull(folderNested);
        Assert.NotNull(folderFolder);

        SolutionProjectModel projectInA = solution.AddProject("ProjectInA.csproj", folder: folderA);
        Assert.NotNull(projectInA);
        SolutionProjectModel projectInFolder = solution.AddProject("ProjectInFolder.csproj", folder: folderFolder);
        Assert.NotNull(projectInFolder);

        // Remove the middle 'A' folder.
        Assert.True(solution.RemoveFolder(folderA));

        // Make sure remaining folders have updated references.
        Assert.Equal("/This/", folderThis.ItemRef);
        Assert.Equal("/This/Is/", folderIs.ItemRef);
        Assert.Equal("/This/Is/Nested/", folderNested.ItemRef);
        Assert.Equal("/This/Is/Nested/Folder/", folderFolder.ItemRef);

        // Make sure projects have updated references.
        Assert.NotNull(projectInA.Parent);
        Assert.Equal("/This/Is/", projectInA.Parent.ItemRef);

        Assert.NotNull(projectInFolder.Parent);
        Assert.Equal("/This/Is/Nested/Folder/", projectInFolder.Parent.ItemRef);

        // Remove all folders.
        Assert.True(solution.RemoveFolder(folderThis));
        Assert.True(solution.RemoveFolder(folderIs));
        Assert.True(solution.RemoveFolder(folderNested));
        Assert.True(solution.RemoveFolder(folderFolder));

        // Make sure projects are in root.
        Assert.Null(projectInA.Parent);
        Assert.Null(projectInFolder.Parent);
    }

    // Rename a folder in the solution model.
    [Fact]
    public void RenameFolder()
    {
        // Create a solution with a deep folder structure and projects.
        SolutionModel solution = new SolutionModel();
        SolutionFolderModel folderThis = solution.AddFolder("/This/");
        SolutionFolderModel folderIs = solution.AddFolder("/This/Is/");
        SolutionFolderModel folderA = solution.AddFolder("/This/Is/A/");
        SolutionFolderModel folderNested = solution.AddFolder("/This/Is/A/Nested/");
        SolutionFolderModel folderFolder = solution.AddFolder("/This/Is/A/Nested/Folder/");

        Assert.NotNull(folderThis);
        Assert.NotNull(folderIs);
        Assert.NotNull(folderA);
        Assert.NotNull(folderNested);
        Assert.NotNull(folderFolder);

        SolutionProjectModel projectInA = solution.AddProject("ProjectInA.csproj", folder: folderA);
        Assert.NotNull(projectInA);
        SolutionProjectModel projectInFolder = solution.AddProject("ProjectInFolder.csproj", folder: folderFolder);
        Assert.NotNull(projectInFolder);

        folderA.Name = "The";

        // Make sure remaining folders have updated references.
        Assert.Equal("/This/", folderThis.ItemRef);
        Assert.Equal("/This/Is/", folderIs.ItemRef);
        Assert.Equal("/This/Is/The/", folderA.ItemRef);
        Assert.Equal("/This/Is/The/Nested/", folderNested.ItemRef);
        Assert.Equal("/This/Is/The/Nested/Folder/", folderFolder.ItemRef);

        // Make sure projects have updated references.
        Assert.NotNull(projectInA.Parent);
        Assert.Equal("/This/Is/The/", projectInA.Parent.ItemRef);

        Assert.NotNull(projectInFolder.Parent);
        Assert.Equal("/This/Is/The/Nested/Folder/", projectInFolder.Parent.ItemRef);
    }

    [Fact]
    public void MoveProjectToFolder()
    {
        // Create a solution with a deep folder structure and projects.
        SolutionModel solution = new SolutionModel();
        SolutionFolderModel folderThis = solution.AddFolder("/This/");
        SolutionFolderModel folderIs = solution.AddFolder("/This/Is/");
        SolutionFolderModel folderA = solution.AddFolder("/This/Is/A/");
        SolutionFolderModel folderNested = solution.AddFolder("/This/Is/A/Nested/");
        SolutionFolderModel folderFolder = solution.AddFolder("/This/Is/A/Nested/Folder/");

        Assert.NotNull(folderThis);
        Assert.NotNull(folderIs);
        Assert.NotNull(folderA);
        Assert.NotNull(folderNested);
        Assert.NotNull(folderFolder);

        SolutionProjectModel existingProject = solution.AddProject(Path.Join("Different", "Project.csproj"), folder: folderA);

        SolutionProjectModel wanderingProject = solution.AddProject("Project.csproj");

        // Move project to folder
        wanderingProject.MoveToFolder(folderThis);
        Assert.NotNull(wanderingProject.Parent);
        Assert.Equal("/This/", wanderingProject.Parent.ItemRef);

        // Move project to another folder
        wanderingProject.MoveToFolder(folderFolder);
        Assert.NotNull(wanderingProject.Parent);
        Assert.Equal("/This/Is/A/Nested/Folder/", wanderingProject.Parent.ItemRef);

        // Try moving project to folder with existing project
        ArgumentException ex = Assert.Throws<ArgumentException>(() => wanderingProject.MoveToFolder(folderA));
        Assert.Equal(string.Format(Errors.DuplicateProjectName_Arg1, wanderingProject.ActualDisplayName), ex.Message);
        Assert.Equal("/This/Is/A/Nested/Folder/", wanderingProject.Parent.ItemRef);

        // Move project to root
        wanderingProject.MoveToFolder(null);
        Assert.Null(wanderingProject.Parent);
    }

    [Fact]
    public void MoveFolder()
    {
        // Create a solution with a deep folder structure and projects.
        SolutionModel solution = new SolutionModel();
        SolutionFolderModel folderThis = solution.AddFolder("/This/");
        SolutionFolderModel folderIs = solution.AddFolder("/This/Is/");
        SolutionFolderModel folderA = solution.AddFolder("/This/Is/A/");
        SolutionFolderModel folderNested = solution.AddFolder("/This/Is/A/Nested/");
        SolutionFolderModel folderFolder = solution.AddFolder("/This/Is/A/Nested/Folder/");

        Assert.NotNull(folderThis);
        Assert.NotNull(folderIs);
        Assert.NotNull(folderA);
        Assert.NotNull(folderNested);
        Assert.NotNull(folderFolder);

        // Move folder to another folder
        folderFolder.MoveToFolder(folderA);
        Assert.NotNull(folderFolder.Parent);
        Assert.Equal("/This/Is/A/", folderFolder.Parent.ItemRef);

        // Try to move folder under itself.
        ArgumentException ex = Assert.Throws<ArgumentException>(() => folderThis.MoveToFolder(folderNested));
        Assert.StartsWith(Errors.CannotMoveFolderToChildFolder, ex.Message);
    }
}
