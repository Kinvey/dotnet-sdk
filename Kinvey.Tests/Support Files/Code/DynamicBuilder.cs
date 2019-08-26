using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Kinvey.Tests
{
    internal static class DynamicBuilder
    {

        internal static void CreateAssemblyWithType()
        {
            //The instance of the type which is going to be created dynamically.

            /*
            [JsonObject(MemberSerialization.OptIn)]
            public class TestType : Entity
            {
                [JsonProperty("TestField")]
                public string TestField;
            }
            */

            //Defining the assembly to be loaded to the domain.
            var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("TestAssembly"), AssemblyBuilderAccess.RunAndCollect);

            //Defining the module to be loaded to the assembly.
            var moduleBuilder = assemblyBuilder.DefineDynamicModule("TestModule");

            //Defining the type to be loaded to the module.
            var typeBuilder = moduleBuilder.DefineType("TestType",
                    TypeAttributes.Public |
                    TypeAttributes.Class,
                    typeof(Entity));

            //Defining the constructor for the type.
            var constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);

            //Adding the [JsonObject(MemberSerialization.OptIn)] attribute to the type.
            var constructorParams = new Type[] { typeof(MemberSerialization) };
            var constructorInfo = typeof(JsonObjectAttribute).GetConstructor(constructorParams);
            var customAttributeBuilder = new CustomAttributeBuilder(
                                constructorInfo,
                                new object[] { MemberSerialization.OptIn });
            typeBuilder.SetCustomAttribute(customAttributeBuilder);

            //Creating the field for the type.
            var fieldBuilder = typeBuilder.DefineField("TestField", typeof(string), FieldAttributes.Public);

            //Adding the [JsonProperty("TestField")] attribute to the field.
            constructorParams = new Type[] { typeof(string) };
            constructorInfo = typeof(JsonPropertyAttribute).GetConstructor(constructorParams);
            customAttributeBuilder = new CustomAttributeBuilder(
                               constructorInfo,
                               new object[] { "TestField" });
            fieldBuilder.SetCustomAttribute(customAttributeBuilder);

            //Adding the field to the type.
            var constructorIl = constructorBuilder.GetILGenerator();
            constructorIl.Emit(OpCodes.Stfld, fieldBuilder);

            //Creating the type.
            typeBuilder.CreateTypeInfo();
        }
    }
}
