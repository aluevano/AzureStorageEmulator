using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ProviderInjection
	{
		private bool injectionEnabled;

		private bool returnNumberOfSubBlocksCreated;

		private int maxSubBlockSize = -1;

		private int numberOfSubBlocksCreated;

		public bool InjectionEnabled
		{
			get
			{
				return this.injectionEnabled;
			}
			set
			{
				this.injectionEnabled = value;
			}
		}

		public int MaxSubBlockSize
		{
			get
			{
				return this.maxSubBlockSize;
			}
			set
			{
				this.maxSubBlockSize = value;
			}
		}

		public int NumberOfSubBlocksCreated
		{
			get
			{
				return this.numberOfSubBlocksCreated;
			}
			set
			{
				this.numberOfSubBlocksCreated = value;
			}
		}

		public bool ReturnNumberOfSubBlocksCreated
		{
			get
			{
				return this.returnNumberOfSubBlocksCreated;
			}
			set
			{
				this.returnNumberOfSubBlocksCreated = value;
			}
		}

		public ProviderInjection()
		{
		}
	}
}