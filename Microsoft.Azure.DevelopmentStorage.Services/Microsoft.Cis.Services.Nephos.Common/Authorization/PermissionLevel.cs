using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	[Flags]
	public enum PermissionLevel
	{
		Read = 1,
		Write = 2,
		ReadWrite = 3,
		Delete = 4,
		ReadDelete = 5,
		WriteDelete = 6,
		ReadWriteDelete = 7,
		ReadAcl = 8,
		WriteAcl = 16,
		ReadAclWriteAcl = 24,
		FullControl = 32,
		Owner = 256
	}
}