using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public static class LoggerFactory
	{
		[SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter", Justification="This is by design")]
		public static T CreateLogger<T>()
		where T : class
		{
			Type type = null;
			Type type1 = typeof(T);
			if (!type1.IsInterface)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { type1 };
				throw new LoggerException(string.Format(invariantCulture, "The type {0} is not an interface", objArray));
			}
			AssemblyName assemblyName = new AssemblyName()
			{
				Name = string.Concat(type1.Namespace, ".Generated"),
				Version = new Version(1, 0, 0, 0)
			};
			AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("Module");
			Type type2 = typeof(object);
			string str = string.Concat(type1.FullName, "Generated");
			Type[] typeArray = new Type[] { type1 };
			TypeBuilder typeBuilder = moduleBuilder.DefineType(str, TypeAttributes.Public | TypeAttributes.Sealed, type2, typeArray);
			Dictionary<string, LoggerFactory.PropertyDescription> strs = LoggerFactory.VerifyLoggerInterface(type1);
			Dictionary<string, FieldBuilder> strs1 = LoggerFactory.DefineFields(typeBuilder, strs);
			LoggerFactory.DefineConstructor(typeBuilder, strs, strs1);
			LoggerFactory.DefineMethods(typeBuilder, strs, strs1);
			try
			{
				type = typeBuilder.CreateType();
			}
			catch (NotSupportedException notSupportedException1)
			{
				NotSupportedException notSupportedException = notSupportedException1;
				throw new LoggerException(string.Concat("Failed to create type: ", notSupportedException.Message), notSupportedException);
			}
			catch (InvalidOperationException invalidOperationException1)
			{
				InvalidOperationException invalidOperationException = invalidOperationException1;
				throw new LoggerException(string.Concat("Failed to create type: ", invalidOperationException.Message), invalidOperationException);
			}
			catch (TypeLoadException typeLoadException1)
			{
				TypeLoadException typeLoadException = typeLoadException1;
				throw new LoggerException(string.Concat("Failed to create type: ", typeLoadException.Message), typeLoadException);
			}
			if (LoggerProvider.Instance == null)
			{
				LoggerProvider.Instance = new DummyLoggerProvider();
			}
			T t = default(T);
			try
			{
				t = (T)Activator.CreateInstance(type);
			}
			catch (TargetInvocationException targetInvocationException1)
			{
				TargetInvocationException targetInvocationException = targetInvocationException1;
				if (targetInvocationException.InnerException != null)
				{
					CultureInfo cultureInfo = CultureInfo.InvariantCulture;
					object[] message = new object[] { type, targetInvocationException.Message };
					throw new LoggerException(string.Format(cultureInfo, "Constructor for type {0} threw an exception: {1}", message), targetInvocationException);
				}
				throw;
			}
			return t;
		}

		private static void DefineConstructor(TypeBuilder typeBuilder, Dictionary<string, LoggerFactory.PropertyDescription> properties, Dictionary<string, FieldBuilder> fieldBuilders)
		{
			ConstructorInfo constructor = typeof(object).GetConstructor(new Type[0]);
			ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[0]);
			ILGenerator lGenerator = constructorBuilder.GetILGenerator();
			lGenerator.Emit(OpCodes.Ldarg_0);
			lGenerator.Emit(OpCodes.Call, constructor);
			foreach (KeyValuePair<string, LoggerFactory.PropertyDescription> property in properties)
			{
				LoggerFactory.PropertyDescription value = property.Value;
				FieldBuilder item = fieldBuilders[property.Key];
				lGenerator.Emit(OpCodes.Ldarg_0);
				lGenerator.Emit(OpCodes.Call, typeof(LoggerProvider).GetProperty("Instance").GetGetMethod());
				lGenerator.Emit(OpCodes.Ldtoken, value.GetMethod.ReturnType);
				lGenerator.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle", BindingFlags.Static | BindingFlags.Public));
				lGenerator.Emit(OpCodes.Ldstr, value.EventStreamName);
				OpCode callvirt = OpCodes.Callvirt;
				Type type = typeof(ILoggerProvider);
				Type[] typeArray = new Type[] { typeof(Type), typeof(string) };
				lGenerator.Emit(callvirt, type.GetMethod("GetLogger", typeArray));
				lGenerator.Emit(OpCodes.Castclass, value.GetMethod.ReturnType);
				lGenerator.Emit(OpCodes.Stfld, item);
			}
			lGenerator.Emit(OpCodes.Ret);
		}

		private static Dictionary<string, FieldBuilder> DefineFields(TypeBuilder typeBuilder, Dictionary<string, LoggerFactory.PropertyDescription> properties)
		{
			Dictionary<string, FieldBuilder> strs = new Dictionary<string, FieldBuilder>();
			foreach (KeyValuePair<string, LoggerFactory.PropertyDescription> property in properties)
			{
				string key = property.Key;
				LoggerFactory.PropertyDescription value = property.Value;
				string str = string.Concat(key, "Field");
				Type returnType = value.GetMethod.ReturnType;
				strs[key] = typeBuilder.DefineField(str, returnType, FieldAttributes.Private);
			}
			return strs;
		}

		private static void DefineMethods(TypeBuilder typeBuilder, Dictionary<string, LoggerFactory.PropertyDescription> properties, Dictionary<string, FieldBuilder> fieldBuilders)
		{
			foreach (KeyValuePair<string, LoggerFactory.PropertyDescription> property in properties)
			{
				string key = property.Key;
				LoggerFactory.PropertyDescription value = property.Value;
				MethodBuilder methodBuilder = typeBuilder.DefineMethod(value.GetMethod.Name, MethodAttributes.FamANDAssem | MethodAttributes.Family | MethodAttributes.Public | MethodAttributes.Virtual, CallingConventions.Standard, value.GetMethod.ReturnType, new Type[0]);
				ILGenerator lGenerator = methodBuilder.GetILGenerator();
				lGenerator.Emit(OpCodes.Ldarg_0);
				lGenerator.Emit(OpCodes.Ldfld, fieldBuilders[key]);
				lGenerator.Emit(OpCodes.Ret);
			}
		}

		private static Dictionary<string, LoggerFactory.PropertyDescription> VerifyLoggerInterface(Type loggerType)
		{
			Dictionary<string, LoggerFactory.PropertyDescription> strs = new Dictionary<string, LoggerFactory.PropertyDescription>();
			List<Type> types = new List<Type>();
			types.AddRange(loggerType.GetInterfaces());
			types.Add(loggerType);
			foreach (Type type in types)
			{
				PropertyInfo[] properties = type.GetProperties();
				for (int i = 0; i < (int)properties.Length; i++)
				{
					PropertyInfo propertyInfo = properties[i];
					EventStreamAttribute eventStreamAttribute = null;
					object[] customAttributes = propertyInfo.GetCustomAttributes(typeof(EventStreamAttribute), false);
					for (int j = 0; j < (int)customAttributes.Length; j++)
					{
						object obj = customAttributes[j];
						if (eventStreamAttribute != null)
						{
							CultureInfo invariantCulture = CultureInfo.InvariantCulture;
							object[] name = new object[] { propertyInfo.Name };
							throw new LoggerException(string.Format(invariantCulture, "Multiple EventStream attributes found on property {0}", name));
						}
						eventStreamAttribute = obj as EventStreamAttribute;
					}
					if (eventStreamAttribute == null)
					{
						CultureInfo cultureInfo = CultureInfo.InvariantCulture;
						object[] objArray = new object[] { propertyInfo.Name };
						throw new LoggerException(string.Format(cultureInfo, "EventStream attribute not found on property {0}", objArray));
					}
					MethodInfo getMethod = propertyInfo.GetGetMethod();
					if (propertyInfo.GetSetMethod() != null)
					{
						CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
						object[] name1 = new object[] { propertyInfo.Name };
						throw new LoggerException(string.Format(invariantCulture1, "Property {0} should not have a set method", name1));
					}
					if (getMethod == null)
					{
						CultureInfo cultureInfo1 = CultureInfo.InvariantCulture;
						object[] objArray1 = new object[] { propertyInfo.Name };
						throw new LoggerException(string.Format(cultureInfo1, "Property {0} does not have a get method", objArray1));
					}
					LoggerFactory.PropertyDescription propertyDescription = new LoggerFactory.PropertyDescription()
					{
						EventStreamAttribute = eventStreamAttribute,
						GetMethod = getMethod,
						EventStreamName = eventStreamAttribute.EventStreamName ?? propertyInfo.Name
					};
					strs[propertyInfo.Name] = propertyDescription;
				}
			}
			return strs;
		}

		private class PropertyDescription
		{
			private string eventStreamName;

			private MethodInfo getMethod;

			private EventStreamAttribute eventStreamAttribute;

			[SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
			public EventStreamAttribute EventStreamAttribute
			{
				get
				{
					return this.eventStreamAttribute;
				}
				set
				{
					this.eventStreamAttribute = value;
				}
			}

			public string EventStreamName
			{
				get
				{
					return this.eventStreamName;
				}
				set
				{
					this.eventStreamName = value;
				}
			}

			public MethodInfo GetMethod
			{
				get
				{
					return this.getMethod;
				}
				set
				{
					this.getMethod = value;
				}
			}

			public PropertyDescription()
			{
			}
		}
	}
}