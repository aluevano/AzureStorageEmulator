using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class AuxStreamGroupInfo
	{
		public int SequenceNumber
		{
			get;
			set;
		}

		public IntPtr StreamGroupHandle
		{
			get;
			set;
		}

		public AuxStreamGroupInfo()
		{
		}
	}
}