using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.UtilityComputing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Services.Providers;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class LinqToXmlTranslator
	{
		private Expression m_linqExpression;

		private string _accountName;

		private string _tableName;

		private Dictionary<string, string> _continuationToken;

		private int m_recursionCount;

		private bool isKeyAccess;

		private PointQueryTracker pointQueryTracker = new PointQueryTracker();

		private ProcessingState m_processingState = ProcessingState.Where;

		private bool m_isBatchRequest;

		private bool m_HasOrderBy;

		private static string[] m_keyColumnNames;

		private Expression m_PreviousExpression;

		private bool m_hasVisitiedBinary = true;

		private Stack<bool> m_needsClosing = new Stack<bool>();

		private int paramIndex;

		private int m_orderByCount;

		private Stack<OpenTypePropertySqlMetaData> m_OpenTypeProperties = new Stack<OpenTypePropertySqlMetaData>();

		internal int MaxRowCount
		{
			get;
			private set;
		}

		private StringBuilder OrderByClause
		{
			get;
			set;
		}

		public List<ParameterRecord> Parameters
		{
			get;
			private set;
		}

		private StringBuilder PartialClause
		{
			get;
			set;
		}

		internal PointQueryTracker PointQuery
		{
			get
			{
				return this.pointQueryTracker;
			}
		}

		internal string[] ProjectedProperties
		{
			get;
			private set;
		}

		internal int ProjectedPropertyCount
		{
			get;
			private set;
		}

		internal string PropertyListName
		{
			get;
			private set;
		}

		internal KeyBounds SASKeyBounds
		{
			get;
			private set;
		}

		internal int TakeCount
		{
			get;
			private set;
		}

		private StringBuilder WhereClause
		{
			get;
			set;
		}

		public StringBuilder XmlQuery
		{
			get;
			private set;
		}

		static LinqToXmlTranslator()
		{
			LinqToXmlTranslator.m_keyColumnNames = new string[] { "PartitionKey", "RowKey", "Timestamp" };
		}

		internal LinqToXmlTranslator(string accountName, string tableName, Expression linqExpression, Dictionary<string, string> continuationToken, int maxRowCount, bool isBatchRequest, KeyBounds sasKeyBounds)
		{
			this.m_linqExpression = linqExpression;
			this.MaxRowCount = maxRowCount;
			this.XmlQuery = new StringBuilder();
			this.WhereClause = new StringBuilder();
			this.OrderByClause = new StringBuilder();
			this.PartialClause = new StringBuilder();
			this.Parameters = new List<ParameterRecord>();
			this._accountName = accountName;
			this._tableName = tableName;
			this._continuationToken = continuationToken;
			this.TakeCount = -1;
			this.ProjectedPropertyCount = -1;
			this.m_isBatchRequest = isBatchRequest;
			this.m_recursionCount = 0;
			this.SASKeyBounds = sasKeyBounds;
		}

		private void CloseBooleanExpression(ExpressionType expressionType, Expression childExpression)
		{
			if (this.m_processingState == ProcessingState.Where && this.m_needsClosing.Count > 0 && this.m_needsClosing.Peek())
			{
				if (childExpression.NodeType == ExpressionType.Convert)
				{
					childExpression = ((UnaryExpression)childExpression).Operand;
				}
				ExpressionType expressionType1 = expressionType;
				switch (expressionType1)
				{
					case ExpressionType.And:
					case ExpressionType.AndAlso:
					{
					Label0:
						if (childExpression is BinaryExpression || childExpression.NodeType == ExpressionType.Not)
						{
							break;
						}
						if (childExpression.NodeType == ExpressionType.Call)
						{
							return;
						}
						this.PartialClause.Append(" <> 0 ");
						break;
					}
					default:
					{
						switch (expressionType1)
						{
							case ExpressionType.Not:
							case ExpressionType.Or:
							case ExpressionType.OrElse:
							{
								goto Label0;
							}
							case ExpressionType.NotEqual:
							{
								break;
							}
							default:
							{
								return;
							}
						}
						break;
					}
				}
			}
		}

		private string GetCaseStatementToCheckSqlType(string propName, string sqlType)
		{
			string str = sqlType;
			if (str != null && str == "int")
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { propName };
				return string.Format(invariantCulture, "CASE WHEN ( Data.value('(/Properties/{0}/@SqlType)[1]','nvarchar(max)') = 'int' OR Data.value('(/Properties/{0}/@SqlType)[1]','nvarchar(max)') = 'bigint' )", objArray);
			}
			CultureInfo cultureInfo = CultureInfo.InvariantCulture;
			object[] objArray1 = new object[] { propName, sqlType };
			return string.Format(cultureInfo, "CASE WHEN ( Data.value('(/Properties/{0}/@SqlType)[1]','nvarchar(max)') = '{1}' )", objArray1);
		}

		private string GetSqlQueryExpressionOperator(ExpressionType nodeType)
		{
			this.pointQueryTracker.AddOperator(nodeType);
			return ExpressionUtility.GetSqlQueryExpressionOperator(nodeType);
		}

		private bool HasOpenPropertyOnStack()
		{
			return this.m_OpenTypeProperties.Count > 0;
		}

		private void InsertOpenTypeFilters(object val, string sqlParam)
		{
			OpenTypePropertySqlMetaData sqlQueryExpressionOperator = this.m_OpenTypeProperties.Pop();
			string sqlTypeForClrType = ExpressionUtility.GetSqlTypeForClrType(val.GetType());
			string str = string.Format("( 1 = CASE WHEN ( Data.exist('(/Properties/{0})[1]') = 1 ) THEN", sqlQueryExpressionOperator.PropertyName);
			str = string.Concat(str, string.Format("\n {0} THEN", this.GetCaseStatementToCheckSqlType(sqlQueryExpressionOperator.PropertyName, sqlTypeForClrType)));
			if (sqlQueryExpressionOperator.ComparisonOperator == null)
			{
				sqlQueryExpressionOperator.ComparisonOperator = this.GetSqlQueryExpressionOperator(ExpressionType.Equal);
			}
			object[] propertyName = new object[] { sqlQueryExpressionOperator.PropertyName, sqlTypeForClrType, sqlQueryExpressionOperator.ComparisonOperator, sqlParam };
			str = string.Concat(str, string.Format("\n CASE WHEN ( Data.value('(/Properties/{0})[1]','{1}') {2} {3} ) THEN 1 ELSE 0", propertyName));
			str = string.Concat(str, " END END END )");
			this.PartialClause.Insert(sqlQueryExpressionOperator.QueryIndex, str);
		}

		private void InsertOpenTypeFiltersWhenRhs(object val)
		{
			OpenTypePropertySqlMetaData openTypePropertySqlMetaDatum = this.m_OpenTypeProperties.Pop();
			string sqlTypeForClrType = ExpressionUtility.GetSqlTypeForClrType(val.GetType());
			string str = string.Format("( CASE WHEN ( Data.exist('(/Properties/{0})[1]') = 1 ) THEN", openTypePropertySqlMetaDatum.PropertyName);
			str = string.Concat(str, string.Format("\n {0} THEN", this.GetCaseStatementToCheckSqlType(openTypePropertySqlMetaDatum.PropertyName, sqlTypeForClrType)));
			str = string.Concat(str, string.Format("\n Data.value('(/Properties/{0})[1]','{1}' )", openTypePropertySqlMetaDatum.PropertyName, sqlTypeForClrType));
			str = string.Concat(str, " END END)");
			this.PartialClause.Insert(openTypePropertySqlMetaDatum.QueryIndex, str);
		}

		private bool IsCompareToMethodCall(Expression expression)
		{
			if (expression.NodeType != ExpressionType.Call)
			{
				return false;
			}
			MethodCallExpression methodCallExpression = expression as MethodCallExpression;
			return string.Equals(methodCallExpression.Method.Name, "Compare", StringComparison.OrdinalIgnoreCase);
		}

		private void ProcessBinaryExpressionOperator(ExpressionType nodeType)
		{
			OpenTypePropertySqlMetaData openTypePropertySqlMetaDatum;
			if (this.m_OpenTypeProperties.Count > 0)
			{
				openTypePropertySqlMetaDatum = this.m_OpenTypeProperties.Peek();
			}
			else
			{
				openTypePropertySqlMetaDatum = null;
			}
			OpenTypePropertySqlMetaData openTypePropertySqlMetaDatum1 = openTypePropertySqlMetaDatum;
			string sqlQueryExpressionOperator = this.GetSqlQueryExpressionOperator(nodeType);
			if (openTypePropertySqlMetaDatum1 != null)
			{
				openTypePropertySqlMetaDatum1.ComparisonOperator = sqlQueryExpressionOperator;
				return;
			}
			this.PartialClause.Append(string.Concat(" ", sqlQueryExpressionOperator, " "));
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
			string str1 = null;
			str1 = (value == null ? item.Value as string : value.Name);
			this.ProcessMemberAccess(str1);
		}

		protected void ProcessMemberAccess(string propertyName)
		{
			if (this.m_processingState != ProcessingState.Where)
			{
				if (this.m_processingState != ProcessingState.OrderBy)
				{
					throw new NotSupportedException("The GetValue method call is only supported for 'Where' and 'OrderBy'");
				}
				if (this.m_orderByCount == 0 && propertyName != "PartitionKey" || this.m_orderByCount == 1 && propertyName != "RowKey" || this.m_orderByCount >= 2)
				{
					throw new NotSupportedException("OrderBy is not supported.");
				}
				this.PartialClause.Append(propertyName);
				this.m_orderByCount++;
				this.isKeyAccess = true;
				return;
			}
			this.pointQueryTracker.AddKey(propertyName);
			string str = propertyName;
			string str1 = str;
			if (str != null && (str1 == "PartitionKey" || str1 == "RowKey" || str1 == "Timestamp"))
			{
				this.PartialClause.Append(propertyName);
				this.isKeyAccess = true;
				return;
			}
			Stack<OpenTypePropertySqlMetaData> mOpenTypeProperties = this.m_OpenTypeProperties;
			OpenTypePropertySqlMetaData openTypePropertySqlMetaDatum = new OpenTypePropertySqlMetaData()
			{
				QueryIndex = this.PartialClause.Length,
				PropertyName = propertyName
			};
			mOpenTypeProperties.Push(openTypePropertySqlMetaDatum);
			this.isKeyAccess = false;
		}

		private int ProcessProjectionProperties(string propertyListName)
		{
			this.PropertyListName = propertyListName;
			string[] strArrays = propertyListName.Split(new char[] { ',' });
			this.ProjectedProperties = strArrays;
			this.ProjectedPropertyCount = (int)strArrays.Length;
			if (this.ProjectedPropertyCount > 255)
			{
				throw new XStoreArgumentException("Projected property count exceeds the maximum");
			}
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				if (strArrays1[i].Length > 255)
				{
					throw new XStoreArgumentException("projected property name is too large.");
				}
			}
			return this.ProjectedPropertyCount;
		}

		protected Expression RecordCallerAndVisit(Expression exp, Expression parentExpression)
		{
			Expression mPreviousExpression = this.m_PreviousExpression;
			Expression expression = null;
			try
			{
				this.m_PreviousExpression = parentExpression;
				expression = this.Visit(exp);
			}
			finally
			{
				this.m_PreviousExpression = mPreviousExpression;
			}
			return expression;
		}

		protected void TranslateExpression()
		{
			try
			{
				this.Visit(this.m_linqExpression);
			}
			catch (ArgumentException argumentException)
			{
				throw;
			}
			catch (NotSupportedException notSupportedException)
			{
				throw;
			}
			catch (Exception exception)
			{
				throw;
			}
		}

		internal void TranslateQuery()
		{
			StringBuilder whereClause = this.WhereClause;
			string[] strArrays = new string[] { "AccountName = '", this._accountName, "' and TableName = '", this._tableName, "'" };
			whereClause.Append(string.Concat(strArrays));
			this.WhereClause.AppendLine();
			if (this._continuationToken != null)
			{
				string str = string.Concat("@p", this.paramIndex);
				this.paramIndex++;
				this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this._continuationToken["PartitionKey"]), false));
				if (this._continuationToken.Count != 1)
				{
					string str1 = string.Concat("@p", this.paramIndex);
					this.paramIndex++;
					this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this._continuationToken["RowKey"]), false));
					this.WhereClause.AppendFormat("AND ((PartitionKey = {0} AND RowKey >= {1})\r\n     OR PartitionKey > {0})", str, str1);
					this.WhereClause.AppendLine();
				}
				else
				{
					this.WhereClause.Append(string.Concat("AND PartitionKey >= ", str));
					this.WhereClause.AppendLine();
				}
			}
			if (this.SASKeyBounds != null)
			{
				string str2 = null;
				string str3 = null;
				if (this.SASKeyBounds.MinPartitionKey != null)
				{
					str2 = string.Concat("@p", this.paramIndex);
					this.paramIndex++;
					this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this.SASKeyBounds.MinPartitionKey), false));
					if (this.SASKeyBounds.MinRowKey == null)
					{
						this.WhereClause.AppendFormat("AND PartitionKey >= {0}", str2);
						this.WhereClause.AppendLine();
					}
					else
					{
						str3 = string.Concat("@p", this.paramIndex);
						this.paramIndex++;
						this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this.SASKeyBounds.MinRowKey), false));
						this.WhereClause.AppendFormat("AND ( PartitionKey > {0} OR (PartitionKey = {0} AND RowKey >= {1}) )", str2, str3);
						this.WhereClause.AppendLine();
					}
				}
				if (this.SASKeyBounds.MaxPartitionKey != null)
				{
					str2 = string.Concat("@p", this.paramIndex);
					this.paramIndex++;
					this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this.SASKeyBounds.MaxPartitionKey), false));
					if (this.SASKeyBounds.MaxRowKey != null)
					{
						str3 = string.Concat("@p", this.paramIndex);
						this.paramIndex++;
						this.Parameters.Add(new ParameterRecord(DevelopmentStorageDbDataContext.EncodeKeyString(this.SASKeyBounds.MaxRowKey), false));
						this.WhereClause.AppendFormat("AND ( PartitionKey < {0} OR (PartitionKey = {0} AND RowKey <= {1}) )", str2, str3);
						this.WhereClause.AppendLine();
					}
					this.WhereClause.AppendFormat("AND PartitionKey <= {0}", str2);
					this.WhereClause.AppendLine();
				}
			}
			this.TranslateExpression();
			if (this.TakeCount < 0)
			{
				this.TakeCount = this.MaxRowCount;
			}
			if (!this.m_HasOrderBy)
			{
				this.OrderByClause.Append("PartitionKey, RowKey");
			}
			int takeCount = this.TakeCount + 1;
			this.XmlQuery.Append(string.Concat("SELECT TOP ", takeCount.ToString(), " *\n"));
			this.XmlQuery.Append("FROM TableRow");
			this.XmlQuery.AppendLine();
			this.XmlQuery.Append("WHERE ");
			this.XmlQuery.Append(this.WhereClause.ToString());
			this.XmlQuery.AppendLine();
			this.XmlQuery.Append("ORDER BY ");
			this.XmlQuery.Append(this.OrderByClause.ToString());
		}

		protected Expression Visit(Expression exp)
		{
			Expression expression;
			LinqToXmlTranslator linqToXmlTranslator = this;
			int mRecursionCount = linqToXmlTranslator.m_recursionCount;
			int num = mRecursionCount;
			linqToXmlTranslator.m_recursionCount = mRecursionCount + 1;
			if (num > 100)
			{
				throw new XStoreArgumentException("Max number of LINQ expression recursive elements reached.");
			}
			try
			{
				if (exp == null)
				{
					return exp;
				}
				else
				{
					switch (exp.NodeType)
					{
						case ExpressionType.Add:
						case ExpressionType.AddChecked:
						case ExpressionType.And:
						case ExpressionType.AndAlso:
						case ExpressionType.ArrayIndex:
						case ExpressionType.Coalesce:
						case ExpressionType.Divide:
						case ExpressionType.Equal:
						case ExpressionType.ExclusiveOr:
						case ExpressionType.GreaterThan:
						case ExpressionType.GreaterThanOrEqual:
						case ExpressionType.LeftShift:
						case ExpressionType.LessThan:
						case ExpressionType.LessThanOrEqual:
						case ExpressionType.Modulo:
						case ExpressionType.Multiply:
						case ExpressionType.MultiplyChecked:
						case ExpressionType.NotEqual:
						case ExpressionType.Or:
						case ExpressionType.OrElse:
						case ExpressionType.Power:
						case ExpressionType.RightShift:
						case ExpressionType.Subtract:
						case ExpressionType.SubtractChecked:
						{
							expression = this.VisitBinary((BinaryExpression)exp);
							break;
						}
						case ExpressionType.ArrayLength:
						case ExpressionType.Convert:
						case ExpressionType.ConvertChecked:
						case ExpressionType.Negate:
						case ExpressionType.UnaryPlus:
						case ExpressionType.NegateChecked:
						case ExpressionType.Not:
						case ExpressionType.Quote:
						case ExpressionType.TypeAs:
						{
							expression = this.VisitUnary((UnaryExpression)exp);
							break;
						}
						case ExpressionType.Call:
						{
							expression = this.VisitMethodCall((MethodCallExpression)exp);
							break;
						}
						case ExpressionType.Conditional:
						case ExpressionType.Invoke:
						case ExpressionType.ListInit:
						case ExpressionType.New:
						case ExpressionType.NewArrayInit:
						case ExpressionType.NewArrayBounds:
						case ExpressionType.Parameter:
						{
							throw new NotSupportedException(string.Format("NodeType {0} is not supported.", exp.NodeType.ToString()));
						}
						case ExpressionType.Constant:
						{
							expression = this.VisitConstant((ConstantExpression)exp);
							break;
						}
						case ExpressionType.Lambda:
						{
							expression = this.VisitLambda((LambdaExpression)exp);
							break;
						}
						case ExpressionType.MemberAccess:
						{
							expression = this.VisitMemberAccess((MemberExpression)exp);
							break;
						}
						case ExpressionType.MemberInit:
						{
							expression = this.VisitMemberInit((MemberInitExpression)exp);
							break;
						}
						default:
						{
							throw new NotSupportedException(string.Format("NodeType {0} is not supported.", exp.NodeType.ToString()));
						}
					}
				}
			}
			finally
			{
				this.m_recursionCount--;
			}
			return expression;
		}

		protected Expression VisitAndOrCall(MethodCallExpression m, ExpressionType expType)
		{
			this.m_needsClosing.Push(true);
			this.PartialClause.Append(" ( ");
			this.RecordCallerAndVisit(m.Arguments[0], m);
			this.CloseBooleanExpression(ExpressionType.AndAlso, m.Arguments[0]);
			this.PartialClause.Append(" ");
			this.PartialClause.Append(this.GetSqlQueryExpressionOperator(expType));
			this.PartialClause.Append(" ");
			this.RecordCallerAndVisit(m.Arguments[1], m);
			this.CloseBooleanExpression(ExpressionType.AndAlso, m.Arguments[1]);
			this.PartialClause.Append(" ) ");
			this.m_needsClosing.Pop();
			return m;
		}

		protected Expression VisitBinary(BinaryExpression b)
		{
			int num = 0;
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
			if (!this.m_needsClosing.Peek() && !this.m_hasVisitiedBinary)
			{
				this.m_hasVisitiedBinary = true;
				if (b.Right.NodeType == ExpressionType.Constant && b.Right.Type == typeof(object))
				{
					this.RecordCallerAndVisit(b.Left, null);
					this.m_needsClosing.Pop();
					return b;
				}
			}
			bool flag = false;
			bool flag1 = false;
			int count = -1;
			int count1 = -1;
			this.PartialClause.Append("( ");
			this.isKeyAccess = false;
			bool methodCall = this.IsCompareToMethodCall(b.Left);
			if (!methodCall)
			{
				if (!this.m_needsClosing.Peek())
				{
					ExpressionUtility.ValidateIsSimpleExpression(b.Left);
					flag = ExpressionUtility.IsConstantExpressionEquivalent(b.Left);
				}
				this.RecordCallerAndVisit(b.Left, b);
				this.CloseBooleanExpression(b.NodeType, b.Left);
			}
			else
			{
				Expression item = ((MethodCallExpression)b.Left).Arguments[0];
				ExpressionUtility.ValidateIsSimpleExpression(item);
				flag = ExpressionUtility.IsConstantExpressionEquivalent(item);
				this.RecordCallerAndVisit(item, b.Left);
			}
			if (b.Left is ConstantExpression || b.Left.NodeType == ExpressionType.Convert)
			{
				num++;
			}
			if (flag)
			{
				count = this.Parameters.Count - 1;
			}
			this.ProcessBinaryExpressionOperator(b.NodeType);
			if (!methodCall)
			{
				this.RecordCallerAndVisit(b.Right, b);
				if (!this.m_needsClosing.Peek())
				{
					flag1 = ExpressionUtility.IsConstantExpressionEquivalent(b.Right);
				}
				this.CloseBooleanExpression(b.NodeType, b.Right);
			}
			else
			{
				this.RecordCallerAndVisit(((MethodCallExpression)b.Left).Arguments[1], b.Left);
				if (!this.m_needsClosing.Peek())
				{
					flag1 = ExpressionUtility.IsConstantExpressionEquivalent(((MethodCallExpression)b.Left).Arguments[1]);
				}
			}
			if (!this.m_needsClosing.Peek() && flag ^ flag1)
			{
				throw new NotImplementedException();
			}
			if (b.Right is ConstantExpression || b.Right.NodeType == ExpressionType.Convert)
			{
				num++;
			}
			if (flag1)
			{
				count1 = this.Parameters.Count - 1;
			}
			if (flag || flag1)
			{
				int num1 = -1;
				if (count != -1 && this.Parameters[count].IsStringValue)
				{
					num1 = count;
				}
				else if (count1 != -1 && this.Parameters[count1].IsStringValue)
				{
					num1 = count1;
				}
				if (num1 != -1)
				{
					string value = this.Parameters[num1].Value as string;
					if (value != null)
					{
						if (!this.isKeyAccess)
						{
							this.Parameters[num1].Value = DevelopmentStorageDbDataContext.EncodeDataString(value);
						}
						else
						{
							this.Parameters[num1].Value = DevelopmentStorageDbDataContext.EncodeKeyString(value);
						}
					}
				}
			}
			this.PartialClause.Append(")");
			this.m_needsClosing.Pop();
			if ((b.NodeType == ExpressionType.GreaterThan || b.NodeType == ExpressionType.GreaterThanOrEqual || b.NodeType == ExpressionType.LessThan || b.NodeType == ExpressionType.LessThanOrEqual || b.NodeType == ExpressionType.Equal || b.NodeType == ExpressionType.NotEqual) && num < 1)
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] mLinqExpression = new object[] { this.m_linqExpression };
				throw new NotSupportedException(string.Format(invariantCulture, "Query filter '{0}' not supported. A constant should be provided in a binary expression.", mLinqExpression));
			}
			return b;
		}

		protected Expression VisitConstant(ConstantExpression c)
		{
			if (!(c.Value is IQueryable))
			{
				if (c.Value == null)
				{
					throw new ArgumentException("Nulls are not supported in a query.", "c");
				}
				if (!this.HasOpenPropertyOnStack())
				{
					this.PartialClause.Append(string.Concat("@p", this.paramIndex));
				}
				else
				{
					this.InsertOpenTypeFilters(c.Value, string.Concat("@p", this.paramIndex));
				}
				this.paramIndex++;
				this.pointQueryTracker.AddValue(c.Value);
				if (c.Value is DateTime)
				{
					DateTime sql2005DateTime = DevelopmentStorageDbDataContext.GetSql2005DateTime((DateTime)c.Value);
					this.Parameters.Add(new ParameterRecord(XmlConvert.ToString(sql2005DateTime, XmlDateTimeSerializationMode.Utc), false));
				}
				else if (c.Value is Guid)
				{
					this.Parameters.Add(new ParameterRecord(c.Value.ToString(), false));
				}
				else if (!(c.Value is string))
				{
					this.Parameters.Add(new ParameterRecord(c.Value, false));
				}
				else
				{
					this.Parameters.Add(new ParameterRecord(c.Value, true));
				}
			}
			return c;
		}

		protected Expression VisitLambda(LambdaExpression lambda)
		{
			Expression expression = this.RecordCallerAndVisit(lambda.Body, lambda);
			if (expression == lambda.Body)
			{
				return lambda;
			}
			return Expression.Lambda(lambda.Type, expression, lambda.Parameters);
		}

		protected Expression VisitMemberAccess(MemberExpression m)
		{
			if (m.Expression == null || m.Expression.NodeType != ExpressionType.Parameter)
			{
				throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.ToString()));
			}
			this.ProcessMemberAccess(m.Member.Name);
			return m;
		}

		protected Expression VisitMemberInit(MemberInitExpression m)
		{
			int num = 0;
			string value = null;
			foreach (MemberBinding binding in m.Bindings)
			{
				num++;
				if (binding.Member.Name.CompareTo("PropertyNameList") != 0)
				{
					continue;
				}
				value = (string)((ConstantExpression)((MemberAssignment)binding).Expression).Value;
				this.ProcessProjectionProperties(value);
			}
			return m;
		}

		protected Expression VisitMethodCall(MethodCallExpression m)
		{
			if (m.Method.DeclaringType == typeof(Queryable) && string.Equals(m.Method.Name, "Where"))
			{
				return this.VisitWhere(m);
			}
			if (!string.Equals(m.Method.Name, "GetValue"))
			{
				if (string.Equals(m.Method.Name, "Take"))
				{
					return this.VisitTake(m);
				}
				if (string.Equals(m.Method.Name, "OrderBy") || string.Equals(m.Method.Name, "ThenBy"))
				{
					return this.VisitOrderBy(m);
				}
				if (string.Equals(m.Method.Name, "AndAlso") && m.Arguments.Count == 2)
				{
					return this.VisitAndOrCall(m, ExpressionType.AndAlso);
				}
				if (string.Equals(m.Method.Name, "OrElse") && m.Arguments.Count == 2)
				{
					return this.VisitAndOrCall(m, ExpressionType.OrElse);
				}
				if (!string.Equals(m.Method.Name, "Select") || m.Arguments.Count != 2)
				{
					throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
				}
				return this.VisitSelectMethodCall(m);
			}
			bool flag = true;
			bool flag1 = false;
			if (this.m_PreviousExpression != null)
			{
				ExpressionType nodeType = this.m_PreviousExpression.NodeType;
				switch (nodeType)
				{
					case ExpressionType.Equal:
					case ExpressionType.GreaterThan:
					case ExpressionType.GreaterThanOrEqual:
					case ExpressionType.LessThan:
					case ExpressionType.LessThanOrEqual:
					{
						if ((this.m_PreviousExpression as BinaryExpression).Right == m)
						{
							flag1 = true;
							if (!ExpressionUtility.IsConstantExpressionEquivalent((this.m_PreviousExpression as BinaryExpression).Left))
							{
								throw new NotImplementedException();
							}
						}
						flag = false;
						break;
					}
					case ExpressionType.ExclusiveOr:
					case ExpressionType.Invoke:
					case ExpressionType.Lambda:
					case ExpressionType.LeftShift:
					{
						break;
					}
					default:
					{
						if (nodeType != ExpressionType.NotEqual)
						{
							break;
						}
						else
						{
							goto case ExpressionType.LessThanOrEqual;
						}
					}
				}
			}
			this.ProcessMemberAccess(m);
			if (flag && this.HasOpenPropertyOnStack())
			{
				this.InsertOpenTypeFilters(true, "1");
			}
			else if (flag1 && this.HasOpenPropertyOnStack())
			{
				this.InsertOpenTypeFiltersWhenRhs(this.Parameters[this.paramIndex - 1].Value);
			}
			return m;
		}

		private Expression VisitOrderBy(MethodCallExpression m)
		{
			this.m_HasOrderBy = true;
			this.RecordCallerAndVisit(m.Arguments[0], m);
			if (!string.Equals(m.Method.Name, "OrderBy"))
			{
				this.OrderByClause.Append(", ");
			}
			else
			{
				this.m_processingState = ProcessingState.OrderBy;
			}
			this.PartialClause.Clear();
			LambdaExpression lambdaExpression = (LambdaExpression)ExpressionUtility.StripQuotes(m.Arguments[1]);
			this.RecordCallerAndVisit(lambdaExpression.Body, lambdaExpression);
			this.OrderByClause.Append(this.PartialClause.ToString());
			return m;
		}

		protected Expression VisitSelectMethodCall(MethodCallExpression m)
		{
			if (m.Arguments[0].NodeType == ExpressionType.Call)
			{
				this.Visit(m.Arguments[0]);
			}
			this.m_processingState = ProcessingState.Select;
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
			this.RecordCallerAndVisit(m.Arguments[0], m);
			return m;
		}

		protected Expression VisitUnary(UnaryExpression u)
		{
			ExpressionType nodeType = u.NodeType;
			if (nodeType == ExpressionType.Convert)
			{
				this.RecordCallerAndVisit(u.Operand, u);
			}
			else
			{
				if (nodeType != ExpressionType.Not)
				{
					throw new NotSupportedException(string.Format("The unary operator '{0}' is not supported", u.NodeType));
				}
				this.m_needsClosing.Push(true);
				this.PartialClause.Append("not( ");
				this.RecordCallerAndVisit(u.Operand, u);
				this.CloseBooleanExpression(u.NodeType, u.Operand);
				this.PartialClause.Append(" )");
				this.m_needsClosing.Pop();
			}
			return u;
		}

		private Expression VisitWhere(MethodCallExpression m)
		{
			this.m_hasVisitiedBinary = false;
			this.m_processingState = ProcessingState.Where;
			if (m.Arguments[0].NodeType == ExpressionType.Call)
			{
				this.RecordCallerAndVisit(m.Arguments[0], m);
			}
			this.WhereClause.AppendFormat(" {0} ", this.GetSqlQueryExpressionOperator(ExpressionType.AndAlso));
			this.PartialClause.Clear();
			LambdaExpression lambdaExpression = (LambdaExpression)ExpressionUtility.StripQuotes(m.Arguments[1]);
			this.RecordCallerAndVisit(lambdaExpression.Body, lambdaExpression);
			this.WhereClause.Append(this.PartialClause.ToString());
			return m;
		}
	}
}