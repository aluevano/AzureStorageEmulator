using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DevStoreTableQueryVisitor : ExpressionVisitor
	{
		private string _accountName;

		private string m_continuationToken;

		private Expression m_linqExpression;

		private bool m_IsOrderByQuery;

		private Stack<bool> m_needsClosing = new Stack<bool>();

		private int paramIndex;

		private int m_orderByCount;

		private ProcessingState m_processingState = ProcessingState.Where;

		internal bool? IsPointQuery
		{
			get;
			private set;
		}

		internal int MaxRowCount
		{
			get;
			private set;
		}

		public List<object> Parameters
		{
			get;
			private set;
		}

		public StringBuilder SqlQuery
		{
			get;
			private set;
		}

		internal int TakeCount
		{
			get;
			private set;
		}

		internal DevStoreTableQueryVisitor(string accountName, Expression linqExpression, string continuationToken, int maxRowCount)
		{
			this.m_linqExpression = linqExpression;
			this.MaxRowCount = maxRowCount;
			this.SqlQuery = new StringBuilder();
			this.Parameters = new List<object>();
			this._accountName = accountName;
			this.m_continuationToken = continuationToken;
			this.TakeCount = -1;
			this.IsPointQuery = null;
		}

		private string GetSqlQueryExpressionOperator(ExpressionType nodeType)
		{
			if (nodeType != ExpressionType.Equal || this.IsPointQuery.HasValue)
			{
				this.IsPointQuery = new bool?(false);
			}
			else
			{
				this.IsPointQuery = new bool?(true);
			}
			return ExpressionUtility.GetSqlQueryExpressionOperator(nodeType);
		}

		protected void ProcessMemberAccess(MethodCallExpression getValueExp)
		{
			int count = getValueExp.Arguments.Count - 1;
			ConstantExpression item = (ConstantExpression)getValueExp.Arguments[count];
			if (item == null)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] str = new object[] { getValueExp.ToString() };
				throw new NotSupportedException(string.Format(invariantCulture, "{0} is not supported", str));
			}
			ResourceProperty value = item.Value as ResourceProperty;
			string name = null;
			if (value == null)
			{
				throw new DataServiceException(400, "Bad query");
			}
			name = value.Name;
			this.ProcessMemberAccess(name);
		}

		protected void ProcessMemberAccess(string propertyName)
		{
			if (this.m_processingState == ProcessingState.Where)
			{
				string str = propertyName;
				if (str == null || !(str == "TableName"))
				{
					throw new DataServiceException(400, "Bad query");
				}
				this.SqlQuery.AppendFormat("UPPER({0})", propertyName);
				return;
			}
			if (this.m_processingState != ProcessingState.OrderBy)
			{
				throw new NotSupportedException("The GetValue method call is only supported for 'Where' and 'OrderBy'");
			}
			if (this.m_orderByCount == 0 && propertyName != "TableName" || this.m_orderByCount >= 1)
			{
				throw new NotSupportedException("OrderBy is not supported.");
			}
			this.SqlQuery.Append(propertyName);
			this.m_orderByCount++;
		}

		protected void TranslateExpression()
		{
			this.Visit(this.m_linqExpression);
		}

		internal void TranslateQuery()
		{
			this.SqlQuery.Append("FROM TableContainer");
			this.SqlQuery.AppendLine();
			this.SqlQuery.Append(string.Concat("WHERE AccountName = '", this._accountName, "'"));
			this.SqlQuery.AppendLine();
			if (this.m_continuationToken != null)
			{
				string str = string.Concat("@p", this.paramIndex);
				this.paramIndex++;
				this.Parameters.Add(this.m_continuationToken);
				this.SqlQuery.Append(string.Concat("AND TableName >= ", str));
				this.SqlQuery.AppendLine();
			}
			this.TranslateExpression();
			if (!this.m_IsOrderByQuery)
			{
				this.SqlQuery.AppendLine();
				this.SqlQuery.Append("ORDER BY TableName");
			}
			if (this.TakeCount < 0)
			{
				this.TakeCount = this.MaxRowCount;
			}
			this.SqlQuery.AppendLine();
			int takeCount = this.TakeCount + 1;
			this.SqlQuery.Insert(0, string.Concat("SELECT TOP ", takeCount.ToString(), " *\n"));
		}

		protected override Expression VisitBinary(BinaryExpression b)
		{
			int num = 0;
			this.SqlQuery.Append("( ");
			ExpressionType nodeType = b.NodeType;
			switch (nodeType)
			{
				case ExpressionType.And:
				case ExpressionType.AndAlso:
				{
				Label0:
					this.m_needsClosing.Push(true);
					break;
				}
				default:
				{
					switch (nodeType)
					{
						case ExpressionType.Or:
						case ExpressionType.OrElse:
						{
							goto Label0;
						}
						default:
						{
							this.m_needsClosing.Push(false);
							break;
						}
					}
					break;
				}
			}
			bool methodCall = ExpressionUtility.IsCompareToMethodCall(b.Left);
			if (!methodCall)
			{
				if (!this.m_needsClosing.Peek())
				{
					ExpressionUtility.ValidateIsSimpleExpression(b.Left);
				}
				this.Visit(b.Left);
			}
			else
			{
				Expression item = ((MethodCallExpression)b.Left).Arguments[0];
				ExpressionUtility.ValidateIsSimpleExpression(item);
				this.Visit(item);
			}
			if (b.Left is ConstantExpression || b.Left.NodeType == ExpressionType.Convert)
			{
				num++;
			}
			this.SqlQuery.AppendFormat(" {0} ", this.GetSqlQueryExpressionOperator(b.NodeType));
			if (!methodCall)
			{
				this.Visit(b.Right);
			}
			else
			{
				this.Visit(((MethodCallExpression)b.Left).Arguments[1]);
			}
			if (b.Right is ConstantExpression || b.Right.NodeType == ExpressionType.Convert)
			{
				num++;
			}
			this.SqlQuery.Append(")");
			this.m_needsClosing.Pop();
			if ((b.NodeType == ExpressionType.GreaterThan || b.NodeType == ExpressionType.GreaterThanOrEqual || b.NodeType == ExpressionType.LessThan || b.NodeType == ExpressionType.LessThanOrEqual || b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) && num < 1)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] mLinqExpression = new object[] { this.m_linqExpression };
				throw new NotSupportedException(string.Format(invariantCulture, "Query filter '{0}' not supported. A constant should be provided in a binary expression.", mLinqExpression));
			}
			return b;
		}

		protected override Expression VisitConstant(ConstantExpression c)
		{
			if (!(c.Value is IQueryable))
			{
				if (c.Value == null)
				{
					throw new ArgumentException("Nulls are not supported in a query.", "c");
				}
				this.SqlQuery.Append(string.Concat("@p", this.paramIndex));
				this.paramIndex++;
				if (!(c.Value is string))
				{
					this.Parameters.Add(c.Value);
				}
				else
				{
					this.Parameters.Add(c.Value.ToString().ToUpperInvariant());
				}
			}
			return c;
		}

		protected override Expression VisitMember(MemberExpression m)
		{
			if (m.Expression == null || m.Expression.NodeType != ExpressionType.Parameter)
			{
				throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.ToString()));
			}
			this.ProcessMemberAccess(m.Member.Name);
			return m;
		}

		protected override Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Queryable) && string.Equals(m.Method.Name, "Where"))
			{
				return this.VisitWhere(m);
			}
			if (string.Equals(m.Method.Name, "GetValue"))
			{
				this.ProcessMemberAccess(m);
				return m;
			}
			if (string.Equals(m.Method.Name, "Take"))
			{
				return this.VisitTake(m);
			}
			if (!string.Equals(m.Method.Name, "OrderBy") && !string.Equals(m.Method.Name, "ThenBy"))
			{
				if (string.Equals(m.Method.Name, "AndAlso") || string.Equals(m.Method.Name, "OrElse") || string.Equals(m.Method.Name, "Select"))
				{
					throw new XStoreArgumentException(string.Format("The method '{0}' is not supported for '{1}' table", m.Method.Name, "Tables"));
				}
				throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
			}
			return this.VisitOrderBy(m);
		}

		private Expression VisitOrderBy(MethodCallExpression m)
		{
			this.m_IsOrderByQuery = true;
			this.Visit(m.Arguments[0]);
			if (!string.Equals(m.Method.Name, "OrderBy"))
			{
				this.SqlQuery.Append(", ");
			}
			else
			{
				this.m_processingState = ProcessingState.OrderBy;
				this.SqlQuery.AppendLine();
				this.SqlQuery.Append("ORDER BY ");
			}
			LambdaExpression lambdaExpression = (LambdaExpression)ExpressionUtility.StripQuotes(m.Arguments[1]);
			this.Visit(lambdaExpression.Body);
			return m;
		}

		private Expression VisitTake(MethodCallExpression m)
		{
			ConstantExpression item = m.Arguments[1] as ConstantExpression;
			if (item == null)
			{
				throw new NotSupportedException("We do not support Take(N) where N is not a constant number.");
			}
			this.TakeCount = (int)item.Value;
			if (this.TakeCount < 0 || this.TakeCount > this.MaxRowCount)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] maxRowCount = new object[] { this.MaxRowCount };
				throw new ArgumentException(string.Format(invariantCulture, "We do not support Take(N) where N < 0 or N > {0}.", maxRowCount));
			}
			this.Visit(m.Arguments[0]);
			return m;
		}

		protected override Expression VisitUnary(UnaryExpression u)
		{
			ExpressionType nodeType = u.NodeType;
			if (nodeType == ExpressionType.Convert)
			{
				this.Visit(u.Operand);
			}
			else
			{
				if (nodeType != ExpressionType.Not)
				{
					throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
				}
				this.m_needsClosing.Push(true);
				this.SqlQuery.Append("not( ");
				this.Visit(u.Operand);
				this.SqlQuery.Append(" )");
				this.m_needsClosing.Pop();
			}
			return u;
		}

		private Expression VisitWhere(MethodCallExpression m)
		{
			this.m_processingState = ProcessingState.Where;
			if (m.Arguments[0].NodeType == ExpressionType.Call)
			{
				this.Visit(m.Arguments[0]);
			}
			this.SqlQuery.AppendFormat(" {0} ", ExpressionUtility.GetSqlQueryExpressionOperator(ExpressionType.AndAlso));
			LambdaExpression lambdaExpression = (LambdaExpression)ExpressionUtility.StripQuotes(m.Arguments[1]);
			this.Visit(lambdaExpression.Body);
			return m;
		}
	}
}