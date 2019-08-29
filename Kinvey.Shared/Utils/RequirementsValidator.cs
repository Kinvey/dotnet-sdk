using System;
using System.Linq;
using System.Reflection;

namespace Kinvey
{
    internal static class RequirementsValidator
    {
        internal static void ValidateMissingGetSetAccessors()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var types = assembly.GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(Entity)));
                    foreach (var type in types)
                    {
                        if (type.CustomAttributes.Any(e => e.AttributeType.FullName.Equals("Newtonsoft.Json.JsonObjectAttribute", StringComparison.OrdinalIgnoreCase)))
                        {
                            var fields = type.GetFields().Where(f => f.IsPublic);
                            foreach (var field in fields)
                            {
                                if (field.CustomAttributes.Any(e => e.AttributeType.FullName.Equals("Newtonsoft.Json.JsonPropertyAttribute", StringComparison.OrdinalIgnoreCase)))
                                {
                                    throw new KinveyException(EnumErrorCategory.ERROR_REQUIREMENT, EnumErrorCode.ERROR_REQUIREMENT_MISSING_GET_SET_ACCESSORS,
                                        $"There is the incorrect field {field.Name} in the type {type.Name}");
                                }
                            }
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                    //The exception that is thrown by the GetTypes() method if any of the classes in a module cannot be loaded.
                }
            }
        }
    }
}
