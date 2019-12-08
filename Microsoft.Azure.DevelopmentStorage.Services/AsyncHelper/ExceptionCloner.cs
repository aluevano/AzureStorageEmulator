using System;
using System.Collections.Generic;
using System.Reflection;

namespace AsyncHelper
{
	public class ExceptionCloner
	{
		private readonly static Dictionary<Type, LateBoundMethod> exceptionTypesWhichCanBeCloned;

		private readonly static Action<Exception, string> remoteStackTraceSetter;

		public static bool EnableExceptionStackPreservation;

		public static HashSet<Type> StackPreservingExceptionTypes;

		static ExceptionCloner()
		{
			ExceptionCloner.exceptionTypesWhichCanBeCloned = new Dictionary<Type, LateBoundMethod>();
			ExceptionCloner.remoteStackTraceSetter = ReflectionExtensions.GetFieldSetter<Exception, string>("_remoteStackTraceString");
			ExceptionCloner.EnableExceptionStackPreservation = false;
			ExceptionCloner.StackPreservingExceptionTypes = new HashSet<Type>();
			Type[] types = Assembly.GetExecutingAssembly().GetTypes();
			for (int i = 0; i < (int)types.Length; i++)
			{
				Type type = types[i];
				object[] customAttributes = type.GetCustomAttributes(typeof(ExceptionClonerAttribute), false);
				for (int j = 0; j < (int)customAttributes.Length; j++)
				{
					ExceptionClonerAttribute lateBoundMethod = (ExceptionClonerAttribute)customAttributes[j];
					ExceptionCloner.exceptionTypesWhichCanBeCloned.ContainsKey(lateBoundMethod.ExceptionTypeCanClone);
					Type[] exceptionTypeCanClone = new Type[] { lateBoundMethod.ExceptionTypeCanClone };
					MethodInfo method = type.GetMethod("CreateRethrowableClone", BindingFlags.Static | BindingFlags.Public, null, exceptionTypeCanClone, null);
					ExceptionCloner.exceptionTypesWhichCanBeCloned[lateBoundMethod.ExceptionTypeCanClone] = method.GetLateBoundMethod();
				}
			}
		}

		public ExceptionCloner()
		{
		}

		public static Exception AttemptClone(Exception ex, RethrowableWrapperBehavior behavior)
		{
			LateBoundMethod lateBoundMethod;
			Exception rethrowableClone = null;
			if (ex == null)
			{
				return null;
			}
			IRethrowableException rethrowableException = ex as IRethrowableException;
			if (rethrowableException != null)
			{
				rethrowableClone = rethrowableException.GetRethrowableClone();
			}
			else if (ExceptionCloner.exceptionTypesWhichCanBeCloned.TryGetValue(ex.GetType(), out lateBoundMethod))
			{
				object[] objArray = new object[] { ex };
				rethrowableClone = (Exception)lateBoundMethod(null, objArray);
			}
			else if (behavior != RethrowableWrapperBehavior.NoWrap)
			{
				rethrowableClone = RethrowableWrapperException.MakeRethrowable(ex);
			}
			else
			{
				ExceptionCloner.remoteStackTraceSetter(ex, ex.StackTrace);
				rethrowableClone = ex;
			}
			return rethrowableClone;
		}

		private static bool ShouldPreserveExceptionStack(Exception ex)
		{
			if (ExceptionCloner.EnableExceptionStackPreservation)
			{
				return true;
			}
			if (ExceptionCloner.StackPreservingExceptionTypes == null)
			{
				return false;
			}
			return ExceptionCloner.StackPreservingExceptionTypes.Contains(ex.GetType());
		}
	}
}