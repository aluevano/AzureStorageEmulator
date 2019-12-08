using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum SASPermission
	{
		None = 0,
		Read = 1,
		Write = 2,
		Add = 4,
		Update = 8,
		Process = 16,
		Queue = 29,
		Delete = 32,
		Table = 45,
		List = 64,
		Blob = 99,
		File = 99,
		Create = 128,
		FileWithCreate = 227,
		BlobWithAddAndCreate = 231
	}
}