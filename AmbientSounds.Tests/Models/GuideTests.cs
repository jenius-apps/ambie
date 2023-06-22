using AmbientSounds.Extensions;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Xunit;

#nullable enable

namespace AmbientSounds.Tests.Models;

public class GuideTests
{
    [Fact]
    public void DeepCopy_IsMatching()
    {
        // Arrange
        var originalGuide = new Guide();
        var properties = originalGuide.GetType().GetProperties();
        foreach (var property in properties)
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
                    var add = dictToCreate.GetMethod("Add", new[] { arguments[0], arguments[1] });
                    add?.Invoke(value, new object?[] { "key", null });
                }
            }

            if (value is not null)
            {
                property.SetValue(originalGuide, value);
            }
        }

        // Act
        var copyGuide = originalGuide.DeepCopy();

        // Assert
        Assert.Equal(JsonSerializer.Serialize(originalGuide), JsonSerializer.Serialize(copyGuide));
    }
}
