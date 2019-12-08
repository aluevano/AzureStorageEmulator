using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public abstract class Logger<T>
	where T : class
	{
		private static T instance;

		private static object syncObj;

		[SuppressMessage("Microsoft.Design", "CA1000:DoNotDeclareStaticMembersOnGenericTypes", Justification="This is for convenience of getting the logger instance.")]
		public static T Instance
		{
			get
			{
				if (Logger<T>.instance == null)
				{
					lock (Logger<T>.syncObj)
					{
						if (Logger<T>.instance == null)
						{
							T t = LoggerFactory.CreateLogger<T>();
							Thread.MemoryBarrier();
							Logger<T>.instance = t;
						}
					}
				}
				return Logger<T>.instance;
			}
		}

		static Logger()
		{
			Logger<T>.syncObj = new object();
		}

		protected Logger()
		{
		}
	}
}