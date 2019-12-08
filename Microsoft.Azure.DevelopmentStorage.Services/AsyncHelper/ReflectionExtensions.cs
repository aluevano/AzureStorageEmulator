using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AsyncHelper
{
	public static class ReflectionExtensions
	{
		public static Action<TTarget, TFieldType> GetFieldSetter<TTarget, TFieldType>(string fieldName)
		{
			FieldInfo field = typeof(TTarget).GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (field == null)
			{
				throw new ArgumentException(string.Format("{0} field not found on type: {1}", fieldName, typeof(TTarget)));
			}
			if (field.FieldType != typeof(TFieldType))
			{
				throw new ArgumentException(string.Format("{0}'s field type does not match action type: {1}", fieldName, typeof(Action<TTarget, TFieldType>)));
			}
			ParameterExpression parameterExpression = Expression.Parameter(typeof(TTarget), "target");
			ParameterExpression parameterExpression1 = Expression.Parameter(typeof(TFieldType), "value");
			MemberExpression memberExpression = Expression.Field(parameterExpression, field);
			BinaryExpression binaryExpression = Expression.Assign(memberExpression, parameterExpression1);
			Type type = typeof(Action<TTarget, TFieldType>);
			ParameterExpression[] parameterExpressionArray = new ParameterExpression[] { parameterExpression, parameterExpression1 };
			return (Action<TTarget, TFieldType>)Expression.Lambda(type, binaryExpression, parameterExpressionArray).Compile();
		}

		public static LateBoundMethod GetGenericLateBoundMethod(this MethodInfo methodInfo, Type[] genericTypeArguments)
		{
			return methodInfo.MakeGenericMethod(genericTypeArguments).GetLateBoundMethod();
		}

		public static LateBoundVoidMethod GetGenericLateBoundVoidMethod(this MethodInfo methodInfo, Type[] genericTypeArguments)
		{
			return methodInfo.MakeGenericMethod(genericTypeArguments).GetLateBoundVoidMethod();
		}

		public static LateBoundMethod GetLateBoundMethod(this MethodInfo methodInfo)
		{
			return ReflectionExtensions.DelegateFactory.Create(methodInfo);
		}

		public static LateBoundVoidMethod GetLateBoundVoidMethod(this MethodInfo methodInfo)
		{
			return ReflectionExtensions.DelegateFactory.CreateVoid(methodInfo);
		}

		private static class DelegateFactory
		{
			public static LateBoundMethod Create(MethodInfo method)
			{
				return ((object target, object[] arguments) => (object)ReflectionExtensions.DelegateFactory.GetMethodCallExpression(method, arguments, target)).Compile();
			}

			private static Expression[] CreateParameterExpressions(MethodInfo method, Expression argumentsParameter)
			{
				return method.GetParameters().Select<ParameterInfo, UnaryExpression>((ParameterInfo parameter, int index) => Expression.Convert(Expression.ArrayIndex(argumentsParameter, Expression.Constant(index)), parameter.ParameterType)).ToArray<UnaryExpression>();
			}

			public static LateBoundVoidMethod CreateVoid(MethodInfo method)
			{
				return ((object target, object[] arguments) => (void)ReflectionExtensions.DelegateFactory.GetMethodCallExpression(method, arguments, target)).Compile();
			}

			private static MethodCallExpression GetMethodCallExpression(MethodInfo method, ParameterExpression argumentsParameter, ParameterExpression instanceParameter)
			{
				Expression expression;
				if (method.IsStatic)
				{
					expression = null;
				}
				else
				{
					expression = Expression.Convert(instanceParameter, method.DeclaringType);
				}
				return Expression.Call(expression, method, ReflectionExtensions.DelegateFactory.CreateParameterExpressions(method, argumentsParameter));
			}
		}
	}
}