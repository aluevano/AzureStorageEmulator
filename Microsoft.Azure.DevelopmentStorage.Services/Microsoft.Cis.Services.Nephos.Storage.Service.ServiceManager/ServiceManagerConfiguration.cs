using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public sealed class ServiceManagerConfiguration
	{
		private const int KB = 1024;

		private int streamCopyBufferSize = 65536;

		public int StreamCopyBufferSize
		{
			get
			{
				return this.streamCopyBufferSize;
			}
			set
			{
				if (value < 1024 || value > 10485760)
				{
					throw new ArgumentOutOfRangeException("value", "streamCopyBufferSize must be between 1KB and 10MB");
				}
				this.streamCopyBufferSize = value;
			}
		}

		public ServiceManagerConfiguration()
		{
		}
	}
}