using AmbientSounds.Extensions;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace AmbientSounds.Tests.Models;

public class ModelCopyTests
{
    [Fact]
    public void SoundDeepCopy_IsMatching()
    {
        // Arrange
        var original = new Sound();
        PopulateProperties(original);

        // Act
        Sound copy = original.DeepCopy();

        // Assert
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(copy));
    }

    [Fact]
    public void GuideDeepCopy_IsMatching()
    {
        // Arrange
        var original = new Guide();
        PopulateProperties(original);

        // Act
        Guide copy = original.DeepCopy();

        // Assert
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(copy));
    }

    [Fact]
    public void VideoDeepCopy_IsMatching()
    {
        // Arrange
        var original = new Video();
        PopulateProperties(original);

        // Act
        Video copy = original.DeepCopy();

        // Assert
        Assert.Equal(JsonSerializer.Serialize(original), JsonSerializer.Serialize(copy));
    }

    private static void PopulateProperties(object targetObject)
    {
        PropertyInfo[] properties = targetObject.GetType().GetProperties();
        foreach (PropertyInfo property in properties)
        {
            object? value = null;
            if (property.PropertyType == typeof(string))
            {
                value = "testing";
            }
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(double))
            {
                value = 1;
            }
            else if (property.PropertyType == typeof(bool))
            {
                value = true;
            }
            else if (property.PropertyType.IsArray && property.PropertyType.GetElementType() is Type t)
            {
                value = Array.CreateInstance(t, 4);
            }
            else if (property.PropertyType
                .GetInterfaces()
                .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
            {
                if (property.PropertyType.GetGenericArguments() is [Type firstType])
                {
                    value = Array.CreateInstance(firstType, 4);
                }
                else if (property.PropertyType.GetGenericArguments() is { Length: 2 } arguments)
                {
                    Type dictToCreate = typeof(Dictionary<,>).MakeGenericType(arguments);
                    value = Activator.CreateInstance(dictToCreate);
                    MethodInfo? add = dictToCreate.GetMethod("Add", [arguments[0], arguments[1]]);
                    _ = add?.Invoke(value, ["key", null]);
                }
            }

            if (value is not null)
            {
                property.SetValue(targetObject, value);
            }
        }
    }
}
