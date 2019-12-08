using System;
using System.Linq.Expressions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class PointQueryTracker
	{
		private bool hasSeenPK;

		private bool expectingPKOp;

		private string pk;

		private bool hasSeenRK;

		private bool expectingRKOp;

		private string rk;

		private bool isPointQuery;

		private bool isAnded;

		public bool IsPointQuery
		{
			get
			{
				if (!this.isAnded || !this.isPointQuery || this.expectingPKOp || this.expectingRKOp || !this.hasSeenPK)
				{
					return false;
				}
				return this.hasSeenRK;
			}
		}

		public string PartitionKey
		{
			get
			{
				return this.pk;
			}
		}

		public string RowKey
		{
			get
			{
				return this.rk;
			}
		}

		public PointQueryTracker()
		{
			this.hasSeenPK = false;
			this.expectingPKOp = false;
			this.hasSeenRK = false;
			this.expectingRKOp = false;
			this.isPointQuery = true;
			this.isAnded = false;
		}

		public void AddKey(string key)
		{
			if (this.isPointQuery)
			{
				string str = key;
				string str1 = str;
				if (str != null)
				{
					if (str1 == "PartitionKey")
					{
						if (this.hasSeenPK)
						{
							this.isPointQuery = false;
							return;
						}
						this.hasSeenPK = true;
						this.expectingPKOp = true;
						return;
					}
					if (str1 == "RowKey")
					{
						if (this.hasSeenRK)
						{
							this.isPointQuery = false;
							return;
						}
						this.hasSeenRK = true;
						this.expectingRKOp = true;
						return;
					}
				}
				this.isPointQuery = false;
			}
		}

		public void AddOperator(ExpressionType expType)
		{
			if (this.isPointQuery)
			{
				if (expType == ExpressionType.AndAlso)
				{
					this.isAnded = true;
					return;
				}
				if (!this.expectingPKOp && !this.expectingRKOp)
				{
					this.isPointQuery = false;
				}
				else if (expType != ExpressionType.Equal)
				{
					this.isPointQuery = false;
				}
				if (this.expectingPKOp && this.expectingRKOp)
				{
					this.isPointQuery = false;
				}
			}
		}

		public void AddValue(object val)
		{
			if (!(val is string) || !this.isPointQuery)
			{
				this.isPointQuery = false;
				return;
			}
			if (this.expectingPKOp)
			{
				this.pk = val as string;
				this.expectingPKOp = false;
				return;
			}
			if (!this.expectingRKOp)
			{
				this.isPointQuery = false;
				return;
			}
			this.rk = val as string;
			this.expectingRKOp = false;
		}
	}
}