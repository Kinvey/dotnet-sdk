using System;
using System.Reflection;

namespace AndroidLibtester
{
	public class ReflectionHelper
	{
		public static object GetPropValue(object src, string propName)
		{
			return src.GetType().GetProperty(propName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(src);
		}

		public static object getFieldValue(object src, string fieldName)
		{
			return src.GetType ().GetField (fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue (src);
		}

		public static object GetInheritedFieldValue(object src, string fieldName){
			return src.GetType ().BaseType.GetField (fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue (src);
		}

		public static object GetDoubleInheritedFieldValue(object src, string fieldName){
			return src.GetType ().BaseType.BaseType.GetField (fieldName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue (src);

		}
	}
}

