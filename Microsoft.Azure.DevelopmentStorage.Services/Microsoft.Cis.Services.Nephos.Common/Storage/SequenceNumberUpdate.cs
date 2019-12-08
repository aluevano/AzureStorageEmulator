using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class SequenceNumberUpdate : ISequenceNumberUpdate
	{
		private SequenceNumberUpdateType updateType;

		private long sequenceNumber;

		public long SequenceNumber
		{
			get
			{
				return JustDecompileGenerated_get_SequenceNumber();
			}
			set
			{
				JustDecompileGenerated_set_SequenceNumber(value);
			}
		}

		public long JustDecompileGenerated_get_SequenceNumber()
		{
			return this.sequenceNumber;
		}

		public void JustDecompileGenerated_set_SequenceNumber(long value)
		{
			this.sequenceNumber = value;
		}

		public SequenceNumberUpdateType UpdateType
		{
			get
			{
				return JustDecompileGenerated_get_UpdateType();
			}
			set
			{
				JustDecompileGenerated_set_UpdateType(value);
			}
		}

		public SequenceNumberUpdateType JustDecompileGenerated_get_UpdateType()
		{
			return this.updateType;
		}

		public void JustDecompileGenerated_set_UpdateType(SequenceNumberUpdateType value)
		{
			this.updateType = value;
		}

		public SequenceNumberUpdate(SequenceNumberUpdateType updateType, long sequenceNumber)
		{
			if (updateType == SequenceNumberUpdateType.None)
			{
				throw new ArgumentException("updateType");
			}
			if (sequenceNumber < (long)0)
			{
				throw new ArgumentException("sequenceNumber");
			}
			this.updateType = updateType;
			this.sequenceNumber = sequenceNumber;
		}

		public override string ToString()
		{
			return string.Format("[UpdateType = {0}, SequenceNumber = {1}]", this.UpdateType, this.SequenceNumber);
		}
	}
}