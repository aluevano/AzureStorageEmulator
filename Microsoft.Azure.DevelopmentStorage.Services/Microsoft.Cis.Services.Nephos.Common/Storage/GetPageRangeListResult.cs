using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class GetPageRangeListResult : IGetPageRangeListResult
	{
		public DateTime BlobLastModificationTime
		{
			get;
			set;
		}

		public long ContentLength
		{
			get;
			set;
		}

		public IPageRangeCollection PageRangeCollection
		{
			get
			{
				return JustDecompileGenerated_get_PageRangeCollection();
			}
			set
			{
				JustDecompileGenerated_set_PageRangeCollection(value);
			}
		}

		private IPageRangeCollection JustDecompileGenerated_PageRangeCollection_k__BackingField;

		public IPageRangeCollection JustDecompileGenerated_get_PageRangeCollection()
		{
			return this.JustDecompileGenerated_PageRangeCollection_k__BackingField;
		}

		public void JustDecompileGenerated_set_PageRangeCollection(IPageRangeCollection value)
		{
			this.JustDecompileGenerated_PageRangeCollection_k__BackingField = value;
		}

		public GetPageRangeListResult(IPageRangeCollection pageRangeCollection)
		{
			this.PageRangeCollection = pageRangeCollection;
		}
	}
}