using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class GetPageRangeListOperationContext
	{
		public long EndOffset
		{
			get;
			set;
		}

		public bool IsContinuing
		{
			get;
			set;
		}

		public GetPageRangeListOperationContext(bool isContinuing, long endOffset)
		{
			this.IsContinuing = isContinuing;
			this.EndOffset = endOffset;
		}
	}
}