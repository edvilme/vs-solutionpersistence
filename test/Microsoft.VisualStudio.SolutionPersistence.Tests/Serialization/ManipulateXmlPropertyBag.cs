﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;

namespace Serialization;

/// <summary>
/// These tests update properties in a properties bag in a slnx file.
/// This test is intented to exercize the code that updates child items in a collection.
/// </summary>
public class ManipulateXmlPropertyBag
{
    /// <summary>
    /// Validates that properties can be removed from a properties bag.
    /// It ensures that whitespace is updated correctly.
    /// </summary>
    [Fact]
    public async Task RemoveSomePropertiesAsync()
    {
        await ValidateModifiedPropertiesAsync(CreateModifiedModel, SlnAssets.XmlSlnxJustProperties, SlnAssets.XmlSlnxProperties_No2No4);

        // Make a new model with some of the properties removed
        static SolutionModel CreateModifiedModel(SolutionModel solution) => solution.CreateCopy(solution =>
        {
            // Remove some of the properties
            SolutionPropertyBag? properties = solution.FindProperties("TestProperties") ?? throw new InvalidOperationException();
            _ = properties.Remove("Prop2");
            _ = properties.Remove("Prop4");
        });
    }

    /// <summary>
    /// Validates that all properties can be removed from a properties bag.
    /// It ensures that whitespace is updated correctly.
    /// </summary>
    [Fact]
    public async Task RemoveAllPropertiesAsync()
    {
        await ValidateModifiedPropertiesAsync(CreateModifiedModel, SlnAssets.XmlSlnxJustProperties, SlnAssets.XmlSlnxProperties_Empty);

        // Make a new model with all the properties removed
        static SolutionModel CreateModifiedModel(SolutionModel solution) => solution.CreateCopy(solution =>
        {
            // Remove all of the properties
            SolutionPropertyBag? properties = solution.FindProperties("TestProperties") ?? throw new InvalidOperationException();
            foreach (string? propertyName in properties.PropertyNames.ToArray())
            {
                _ = properties.Remove(propertyName);
            }
        });
    }

    /// <summary>
    /// This test validates that properties can be added to an existing properties bag.
    /// It ensures that whitespace is updated correctly.
    /// </summary>
    [Fact]
    public async Task AddPropertiesAsync()
    {
        await ValidateModifiedPropertiesAsync(CreateModifiedModel, SlnAssets.XmlSlnxJustProperties, SlnAssets.XmlSlnxProperties_Add0Add7);

        // Make a new model with all the properties removed
        static SolutionModel CreateModifiedModel(SolutionModel solution) => solution.CreateCopy(solution =>
        {
            // Remove some of the properties
            SolutionPropertyBag? properties = solution.FindProperties("TestProperties") ?? throw new InvalidOperationException();
            properties.Add($"Prop0", $"Value0");
            properties.Add($"Prop7", $"Value7");
        });
    }

    /// <summary>
    /// Validates that properties can be added to an empty properties bag.
    /// It ensures that whitespace is updated correctly.
    /// </summary>
    [Fact]
    public async Task AddFromEmptyAsync()
    {
        await ValidateModifiedPropertiesAsync(CreateModifiedModel, SlnAssets.XmlSlnxProperties_Empty, SlnAssets.XmlSlnxProperties_NoComments);

        // Make a new model with all the properties removed
        static SolutionModel CreateModifiedModel(SolutionModel solution) => solution.CreateCopy(solution =>
        {
            // Add all of the properties
            SolutionPropertyBag? properties = solution.FindProperties("TestProperties") ?? throw new InvalidOperationException();
            Assert.True(properties.IsNullOrEmpty());
            for (int i = 0; i < 6; i++)
            {
                properties.Add($"Prop{i + 1}", $"Value{i + 1}");
            }
        });
    }

    private static async Task ValidateModifiedPropertiesAsync(Func<SolutionModel, SolutionModel> createModifiedModel, ResourceStream originalSlnx, ResourceStream expectedSlnx)
    {
        // Open the Model from stream.
        SolutionModel model = await SolutionSerializers.SlnXml.OpenAsync(originalSlnx.Stream, CancellationToken.None);
        AssertNotTarnished(model);

        SolutionModel updateModel = createModifiedModel(model);

        // Save the Model back to stream.
        FileContents reserializedSolution = await ModelToLinesAsync(SolutionSerializers.SlnXml, updateModel);

        AssertSolutionsAreEqual(expectedSlnx.ToLines(), reserializedSolution);
    }
}
