using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class ContainerStats : ICloneable
	{
		public ulong ContainerUsageInGB
		{
			get;
			set;
		}

		public ContainerStats()
		{
		}

		public object Clone()
		{
			return new ContainerStats()
			{
				ContainerUsageInGB = this.ContainerUsageInGB
			};
		}

		public override string ToString()
		{
			return string.Format("(ContainerUsageInGB:{0})", this.ContainerUsageInGB);
		}
	}
}