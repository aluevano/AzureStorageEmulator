using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class GetBlockListOperationContext
	{
		public string BlockListType
		{
			get;
			internal set;
		}

		public GetBlockListOperationContext()
		{
		}

		public GetBlockListOperationContext(string blockListType)
		{
			this.BlockListType = blockListType;
		}
	}
}