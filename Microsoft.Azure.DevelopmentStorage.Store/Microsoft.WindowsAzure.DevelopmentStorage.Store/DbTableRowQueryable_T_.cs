using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class DbTableRowQueryable<T> : IOrderedQueryable<T>, IQueryable<T>, IEnumerable<T>, IOrderedQueryable, IQueryable, IEnumerable
	{
		public Type ElementType
		{
			get
			{
				return typeof(T);
			}
		}

		public System.Linq.Expressions.Expression Expression
		{
			get
			{
				return JustDecompileGenerated_get_Expression();
			}
			set
			{
				JustDecompileGenerated_set_Expression(value);
			}
		}

		private System.Linq.Expressions.Expression JustDecompileGenerated_Expression_k__BackingField;

		public System.Linq.Expressions.Expression JustDecompileGenerated_get_Expression()
		{
			return this.JustDecompileGenerated_Expression_k__BackingField;
		}

		private void JustDecompileGenerated_set_Expression(System.Linq.Expressions.Expression value)
		{
			this.JustDecompileGenerated_Expression_k__BackingField = value;
		}

		public IQueryProvider Provider
		{
			get
			{
				return JustDecompileGenerated_get_Provider();
			}
			set
			{
				JustDecompileGenerated_set_Provider(value);
			}
		}

		private IQueryProvider JustDecompileGenerated_Provider_k__BackingField;

		public IQueryProvider JustDecompileGenerated_get_Provider()
		{
			return this.JustDecompileGenerated_Provider_k__BackingField;
		}

		private void JustDecompileGenerated_set_Provider(IQueryProvider value)
		{
			this.JustDecompileGenerated_Provider_k__BackingField = value;
		}

		public DbTableRowQueryable(IQueryProvider provider)
		{
			this.Expression = System.Linq.Expressions.Expression.Constant(this);
			this.Provider = provider;
		}

		public DbTableRowQueryable(IQueryProvider provider, System.Linq.Expressions.Expression expression)
		{
			this.Expression = expression;
			this.Provider = provider;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.Provider.Execute<IEnumerator<T>>(this.Expression);
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}