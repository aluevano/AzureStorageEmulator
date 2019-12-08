using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface IServiceEntry : IDisposable
	{
		void Initialize(IServiceEntrySink sink);

		void Start();

		void Stop();
	}
}