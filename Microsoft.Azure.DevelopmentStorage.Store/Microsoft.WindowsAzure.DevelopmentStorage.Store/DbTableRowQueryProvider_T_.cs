using Microsoft.Cis.Services.Nephos.Table.Service.Providers.XTable;
using Microsoft.UtilityComputing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class DbTableRowQueryProvider<T> : IQueryProvider
	{
		private DbTableDataContext _context;

		private string _accountName;

		private string _userResourceName;

		private KeyBounds sasKeyBounds;

		private bool checkForReadPermission;

		public bool CheckForReadPermission
		{
			get
			{
				return this.checkForReadPermission;
			}
			set
			{
				this.checkForReadPermission = value;
			}
		}

		internal PointQueryTracker PointQuery
		{
			get;
			private set;
		}

		internal KeyBounds SASKeyBounds
		{
			get
			{
				return this.sasKeyBounds;
			}
			set
			{
				this.sasKeyBounds = value;
			}
		}

		public DbTableRowQueryProvider(DbTableDataContext context, string accountName, string userResourceName)
		{
			this._context = context;
			this._accountName = accountName;
			this._userResourceName = userResourceName;
		}

		public IQueryable CreateQuery(Expression expression)
		{
			Type type = expression.Type;
			Type[] genericArguments = expression.Type.GetGenericArguments();
			int num = 0;
			if (num < (int)genericArguments.Length)
			{
				type = genericArguments[num];
			}
			Type type1 = typeof(DbTableRowQueryable<>).MakeGenericType(new Type[] { type });
			Type[] typeArray = new Type[] { typeof(IQueryProvider), typeof(Expression) };
			ConstructorInfo constructor = type1.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, typeArray, null);
			object[] objArray = new object[] { this, expression };
			return (IQueryable)constructor.Invoke(objArray);
		}

		public IQueryable<TResult> CreateQuery<TResult>(Expression expression)
		{
			return new DbTableRowQueryable<TResult>(this, expression);
		}

		public object Execute(Expression expression)
		{
			string str;
			IEnumerator<T> enumerator = this.Execute<IEnumerator<T>>(expression);
			if (!enumerator.MoveNext())
			{
				str = "Result contains no elements.";
			}
			else
			{
				object current = enumerator.Current;
				if (!enumerator.MoveNext())
				{
					return current;
				}
				str = "Result contains more than one element.";
			}
			throw new InvalidOperationException(str);
		}

		public TResult Execute<TResult>(Expression expression)
		{
			PointQueryTracker pointQuery;
			if (this.CheckForReadPermission && this._context != null)
			{
				bool flag = TableResourceContainer.IsUtilityTables(this._userResourceName);
				this._context.CheckPermission(this._userResourceName, flag, true, UpdateKind.None);
			}
			if (TableResourceContainer.IsUtilityTables(this._userResourceName))
			{
				string str = null;
				if (this._context.ContinuationToken != null)
				{
					this._context.ContinuationToken.TryGetValue("TableName", out str);
				}
				DevStoreTableQueryVisitor devStoreTableQueryVisitor = new DevStoreTableQueryVisitor(this._accountName, expression, str, TableDataContextHelper.MaxRowCount);
				devStoreTableQueryVisitor.TranslateQuery();
				if (this._context.IsBatchRequest && (!devStoreTableQueryVisitor.IsPointQuery.HasValue || !devStoreTableQueryVisitor.IsPointQuery.HasValue || !devStoreTableQueryVisitor.IsPointQuery.Value))
				{
					throw new NotImplementedException();
				}
				string str1 = devStoreTableQueryVisitor.SqlQuery.ToString();
				object[] array = devStoreTableQueryVisitor.Parameters.ToArray();
				List<TableContainer> list = this._context.m_dbContext.ExecuteQuery<TableContainer>(str1, array).ToList<TableContainer>();
				if (devStoreTableQueryVisitor.TakeCount >= 0 && devStoreTableQueryVisitor.TakeCount < list.Count)
				{
					TableContainer item = list[list.Count - 1];
					this._context.ContinuationToken = new Dictionary<string, string>()
					{
						{ "NextTableName", item.CasePreservedTableName }
					};
					this._context.ContinuationTokenAvailableCallback(this._context.ContinuationToken);
					list.RemoveAt(list.Count - 1);
				}
				return (TResult)DbUtilityResourceBuilder.GetUtilityTableEnumerator(list.GetEnumerator());
			}
			LinqToXmlTranslator linqToXmlTranslator = new LinqToXmlTranslator(this._accountName, this._userResourceName, expression, this._context.ContinuationToken, TableDataContextHelper.MaxRowCount, this._context.IsBatchRequest, this.SASKeyBounds);
			linqToXmlTranslator.TranslateQuery();
			if (linqToXmlTranslator.PointQuery.IsPointQuery)
			{
				pointQuery = linqToXmlTranslator.PointQuery;
			}
			else
			{
				pointQuery = null;
			}
			this.PointQuery = pointQuery;
			if (this._context.IsBatchRequest && this.PointQuery == null)
			{
				throw new NotSupportedException("We support retrieving only a single resource via a batch.");
			}
			string str2 = linqToXmlTranslator.XmlQuery.ToString();
			object[] objArray = (
				from ParameterRecord  in linqToXmlTranslator.Parameters
				select r.Value).ToArray<object>();
			List<TableRow> tableRows = this._context.m_dbContext.ExecuteQuery<TableRow>(str2, objArray).ToList<TableRow>();
			if (linqToXmlTranslator.TakeCount >= 0 && linqToXmlTranslator.TakeCount < tableRows.Count)
			{
				TableRow tableRow = tableRows[tableRows.Count - 1];
				this._context.ContinuationToken = new Dictionary<string, string>();
				this._context.ContinuationToken["NextPartitionKey"] = DevelopmentStorageDbDataContext.DecodeKeyString(tableRow.PartitionKey);
				this._context.ContinuationToken["NextRowKey"] = DevelopmentStorageDbDataContext.DecodeKeyString(tableRow.RowKey);
				this._context.ContinuationTokenAvailableCallback(this._context.ContinuationToken);
				tableRows.RemoveAt(tableRows.Count - 1);
			}
			if (linqToXmlTranslator.ProjectedPropertyCount < 0)
			{
				return (TResult)DbUtilityResourceBuilder.GetUtilityRowEnumerator(tableRows.GetEnumerator());
			}
			return (TResult)DbUtilityResourceBuilder.GetProjectedWrapperEnumerator(tableRows.GetEnumerator(), linqToXmlTranslator.ProjectedPropertyCount, linqToXmlTranslator.PropertyListName, this._userResourceName, linqToXmlTranslator.ProjectedProperties);
		}
	}
}