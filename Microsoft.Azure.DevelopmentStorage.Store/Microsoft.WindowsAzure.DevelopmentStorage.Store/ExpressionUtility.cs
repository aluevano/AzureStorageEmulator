using Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class ExpressionUtility
	{
		public ExpressionUtility()
		{
		}

		internal static string GetSqlQueryExpressionOperator(ExpressionType nodeType)
		{
			ExpressionType expressionType = nodeType;
			if (expressionType == ExpressionType.AndAlso)
			{
				return "and";
			}
			switch (expressionType)
			{
				case ExpressionType.Equal:
				{
					return "=";
				}
				case ExpressionType.ExclusiveOr:
				case ExpressionType.Invoke:
				case ExpressionType.Lambda:
				case ExpressionType.LeftShift:
				{
					throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", nodeType));
				}
				case ExpressionType.GreaterThan:
				{
					return ">";
				}
				case ExpressionType.GreaterThanOrEqual:
				{
					return ">=";
				}
				case ExpressionType.LessThan:
				{
					return "<";
				}
				case ExpressionType.LessThanOrEqual:
				{
					return "<=";
				}
				default:
				{
					switch (expressionType)
					{
						case ExpressionType.NotEqual:
						{
							return "!=";
						}
						case ExpressionType.Or:
						{
							throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", nodeType));
						}
						case ExpressionType.OrElse:
						{
							return "or";
						}
						default:
						{
							throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", nodeType));
						}
					}
					break;
				}
			}
		}

		internal static string GetSqlTypeForClrType(Type type)
		{
			if (type.Name == typeof(int?).Name)
			{
				type = type.GetGenericArguments()[0];
			}
			string name = type.Name;
			string str = name;
			if (name != null)
			{
				switch (str)
				{
					case "Int32":
					{
						return "int";
					}
					case "Int64":
					{
						return "bigint";
					}
					case "DateTime":
					{
						return "datetime";
					}
					case "Guid":
					{
						return "uniqueidentifier";
					}
					case "Double":
					{
						return "float(53)";
					}
					case "Boolean":
					{
						return "bit";
					}
					case "Byte[]":
					{
						return "varbinary(max)";
					}
					case "String":
					{
						return "nvarchar(max)";
					}
				}
			}
			throw new NotImplementedException();
		}

		internal static bool IsCompareToMethodCall(Expression expression)
		{
			if (expression.NodeType != ExpressionType.Call)
			{
				return false;
			}
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			return string.Equals(methodCallExpression.Method.Name, "Compare", StringComparison.OrdinalIgnoreCase);
		}

		internal static bool IsConstantExpressionEquivalent(Expression expression)
		{
			switch (expression.NodeType)
			{
				case ExpressionType.Constant:
				{
					return true;
				}
				case ExpressionType.Convert:
				{
					return ExpressionUtility.IsConstantExpressionEquivalent((expression as UnaryExpression).Operand);
				}
			}
			return false;
		}

		internal static Expression StripQuotes(Expression e)
		{
			while (e.NodeType == ExpressionType.Quote)
			{
				e = ((UnaryExpression)e).Operand;
			}
			return e;
		}

		internal static void ValidateIsSimpleExpression(Expression expression)
		{
			ExpressionType nodeType = expression.NodeType;
			switch (nodeType)
			{
				case ExpressionType.Call:
				{
					if (!((MethodCallExpression)expression).Method.Name.Equals("GetValue"))
					{
						throw new TableServiceArgumentException("This expression is not supported in the LHS of a comparison");
					}
					return;
				}
				case ExpressionType.Coalesce:
				case ExpressionType.Conditional:
				{
					throw new TableServiceArgumentException("This expression is not supported in the LHS of a comparison");
				}
				case ExpressionType.Constant:
				case ExpressionType.Convert:
				{
					return;
				}
				default:
				{
					if (nodeType != ExpressionType.MemberAccess)
					{
						throw new TableServiceArgumentException("This expression is not supported in the LHS of a comparison");
					}
					else
					{
						return;
					}
				}
			}
		}
	}
}