using System;
using System.Runtime.InteropServices;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class BillingNativeMethods
	{
		public BillingNativeMethods()
		{
		}

		[DllImport("XTableDLL.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern bool IsIntranetRequest(string clientIPAddress);

		[DllImport("XTableDLL.dll", CallingConvention=CallingConvention.StdCall, CharSet=CharSet.Unicode, ExactSpelling=false)]
		internal static extern void UpdateTransactionMetrics(string accountName, string accountVersion, string requestType, string transactionType, string objectType, string tenant, string blobName, string containerName, string subscription, string resourceGroup, bool isManagedBySrp, uint internalReponseStatus, uint userResponseStatus, string clientIPAddress, string clientIPv6Address, int regionID, bool IsIntranetRequest, long ingressBandwidth, long egressBandwidth, uint e2eLatency, uint serverLatency, bool isTransactionCounted, uint transactionCount, long ioCount);
	}
}