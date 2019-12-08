using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum CopyBlobType2
	{
		CopyBlobType_None,
		CopyBlobType_Block,
		CopyBlobType_Page,
		CopyBlobType_Internet,
		CopyBlobType_File,
		CopyBlobType_PageXIO,
		CopyBlobType_Append
	}
}