using MeasurementEvents;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class NephosBaseMeasurementEvent<T> : MeasurementEvent<T>, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : NephosBaseMeasurementEvent<T>
	{
		private string accountName;

		private RequestOrigin origin;

		private bool isAdmin;

		private long errorResponseBytes;

		private int itemsCount;

		private int batchOperationCount;

		[MeasurementEventParameter]
		public string AccountName
		{
			get
			{
				return this.accountName;
			}
			set
			{
				this.accountName = value;
			}
		}

		public virtual int BatchItemsCount
		{
			get
			{
				return this.batchOperationCount;
			}
			set
			{
				this.batchOperationCount = value;
			}
		}

		[MeasurementEventParameter]
		public long ErrorResponseBytes
		{
			get
			{
				return this.errorResponseBytes;
			}
			set
			{
				this.errorResponseBytes = value;
			}
		}

		[MeasurementEventParameter]
		public bool IsAdmin
		{
			get
			{
				return this.isAdmin;
			}
			set
			{
				this.isAdmin = value;
			}
		}

		public virtual int ItemsReturnedCount
		{
			get
			{
				return this.itemsCount;
			}
			set
			{
				this.itemsCount = value;
			}
		}

		public string OperationName
		{
			get
			{
				return JustDecompileGenerated_get_OperationName();
			}
			set
			{
				JustDecompileGenerated_set_OperationName(value);
			}
		}

		private string JustDecompileGenerated_OperationName_k__BackingField;

		public string JustDecompileGenerated_get_OperationName()
		{
			return this.JustDecompileGenerated_OperationName_k__BackingField;
		}

		private void JustDecompileGenerated_set_OperationName(string value)
		{
			this.JustDecompileGenerated_OperationName_k__BackingField = value;
		}

		public virtual string OperationPartitionKey
		{
			get
			{
				return null;
			}
		}

		public string OperationStatus
		{
			get
			{
				return base.Status.ToString();
			}
		}

		[MeasurementEventParameter]
		public RequestOrigin Origin
		{
			get
			{
				return this.origin;
			}
			set
			{
				this.origin = value;
			}
		}

		protected NephosBaseMeasurementEvent(string operationName)
		{
			this.OperationName = operationName;
		}

		public virtual string GetObjectKey()
		{
			return string.Empty;
		}

		public virtual string GetObjectType()
		{
			return string.Empty;
		}
	}
}