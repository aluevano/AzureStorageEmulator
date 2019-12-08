using System;

namespace AsyncHelper
{
	public interface IPerfTimer
	{
		TimeSpan Elapsed
		{
			get;
		}

		bool IsRunning
		{
			get;
		}

		TimeSpan Remaining(TimeSpan total);

		void Reset();

		void Start();

		void Stop();
	}
}