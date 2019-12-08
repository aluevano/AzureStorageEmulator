using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using System;

namespace Microsoft.UtilityComputing
{
	public static class XfeTableSASAuthorizationManager
	{
		public static void AuthorizeSASRequest(TableSignedAccessAccountIdentifier sasAccountIdentifier, PermissionLevel permissionLevel, SASPermission sasPermissionsRequired, string userTableName, bool isUtilityTableCommand)
		{
			if (sasAccountIdentifier == null)
			{
				return;
			}
			SASPermission signedAccessPermission = sasAccountIdentifier.SignedAccessPermission;
			if (isUtilityTableCommand)
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request.", sasAccountIdentifier.AccountName, sasAccountIdentifier.TableName, null, sasAccountIdentifier, permissionLevel, signedAccessPermission, AuthorizationFailureReason.InvalidOperationSAS);
			}
			if ((sasPermissionsRequired & ~45) != SASPermission.None)
			{
				NephosAssertionException.Fail(string.Format("Signed permission is not well formed. Signed permission: {0}", sasPermissionsRequired));
			}
			if (!string.Equals(sasAccountIdentifier.TableName, userTableName.ToLower(), StringComparison.Ordinal))
			{
				throw new NephosUnauthorizedAccessException("Signed access not supported for this request as table name did not match", sasAccountIdentifier.AccountName, sasAccountIdentifier.TableName, null, sasAccountIdentifier, permissionLevel, signedAccessPermission, AuthorizationFailureReason.InvalidOperationSAS);
			}
			if ((sasPermissionsRequired & signedAccessPermission) != sasPermissionsRequired)
			{
				throw new NephosUnauthorizedAccessException("Signed access insufficient permission", sasAccountIdentifier.AccountName, sasAccountIdentifier.TableName, null, sasAccountIdentifier, permissionLevel, signedAccessPermission, AuthorizationFailureReason.PermissionMismatch);
			}
		}

		public static void CheckSignedAccessKeyBoundary(TableSignedAccessAccountIdentifier sasAccountIdentifier, string partitionKey, string rowKey)
		{
			if (sasAccountIdentifier == null)
			{
				return;
			}
			if (sasAccountIdentifier.StartingPartitionKey != null)
			{
				if (partitionKey == null)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
				int num = string.Compare(partitionKey, sasAccountIdentifier.StartingPartitionKey, StringComparison.Ordinal);
				if (num < 0)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
				if (num == 0 && sasAccountIdentifier.StartingRowKey != null && string.Compare(rowKey, sasAccountIdentifier.StartingRowKey) < 0)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
			}
			else if (partitionKey == null && sasAccountIdentifier.StartingRowKey != null && string.Compare(rowKey, sasAccountIdentifier.StartingRowKey) < 0)
			{
				throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
			}
			if (sasAccountIdentifier.EndingPartitionKey != null)
			{
				if (partitionKey == null)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
				int num1 = string.Compare(partitionKey, sasAccountIdentifier.EndingPartitionKey, StringComparison.Ordinal);
				if (num1 > 0)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
				if (num1 == 0 && sasAccountIdentifier.EndingRowKey != null && string.Compare(rowKey, sasAccountIdentifier.EndingRowKey) > 0)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
				}
			}
			else if (partitionKey == null && sasAccountIdentifier.EndingRowKey != null && string.Compare(rowKey, sasAccountIdentifier.EndingRowKey) > 0)
			{
				throw new NephosUnauthorizedAccessException("Signed access insufficient permission as keys are out of bounds");
			}
		}

		private static int KeyCompare(string lhsPartitionKey, string lhsRowKey, bool lhsNullIsMin, string rhsPartitionKey, string rhsRowKey, bool rhsNullIsMin)
		{
			if (lhsPartitionKey == null)
			{
				if (rhsPartitionKey == null && rhsNullIsMin == lhsNullIsMin)
				{
					return 0;
				}
				if (!lhsNullIsMin)
				{
					return 1;
				}
				return -1;
			}
			if (rhsPartitionKey == null)
			{
				if (!rhsNullIsMin)
				{
					return -1;
				}
				return 1;
			}
			int num = string.Compare(lhsPartitionKey, rhsPartitionKey, StringComparison.Ordinal);
			if (num == 0)
			{
				if (lhsRowKey == null)
				{
					if (rhsRowKey == null && rhsNullIsMin == lhsNullIsMin)
					{
						return 0;
					}
					if (!lhsNullIsMin)
					{
						return 1;
					}
					return -1;
				}
				if (rhsRowKey == null)
				{
					if (!rhsNullIsMin)
					{
						return -1;
					}
					return 1;
				}
				num = string.Compare(lhsRowKey, rhsRowKey, StringComparison.Ordinal);
			}
			return num;
		}
	}
}