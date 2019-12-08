using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Account
{
	public interface IAccountIdentifier
	{
		string AccountName
		{
			get;
		}

		bool IsAdmin
		{
			get;
		}

		bool? IsAnonymous
		{
			get;
		}

		bool IsDeleteAllowed
		{
			get;
		}

		bool IsFullControlAllowed
		{
			get;
		}

		bool IsKeyDisabled
		{
			get;
		}

		bool IsReadAllowed
		{
			get;
		}

		bool IsSecondaryAccess
		{
			get;
			set;
		}

		bool IsWriteAllowed
		{
			get;
		}

		bool IsWriteAllowedBySecretKey
		{
			get;
		}

		SecretKeyPermissions KeyUsedPermissions
		{
			get;
		}

		AccountPermissions Permissions
		{
			get;
		}
	}
}