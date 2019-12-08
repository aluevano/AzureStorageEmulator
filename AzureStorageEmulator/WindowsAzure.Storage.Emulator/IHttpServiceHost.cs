using System;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	internal interface IHttpServiceHost : IDisposable
	{
		string UrlBase
		{
			get;
			set;
		}

		bool IsRunning();

		void Restart();

		void Start();

		void Stop();
	}
}