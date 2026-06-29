using UnityEngine;
using System;

// This allows it to be used on fields only, and prevents stacking multiple [Required] tags
[AttributeUsage(AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
public class RequiredAttribute : PropertyAttribute
{
    public string Message { get; private set; }

    public RequiredAttribute(string message = "This required field is empty!")
    {
        Message = message;
    }
}