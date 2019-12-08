using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class Utils
	{
		public const string PerfCountersFormatString = "PerfCounters: Account={0} Operation={1} on Container={2} with Status={3} RequestHeaderSize={4} RequestSize={5} ResponseHeaderSize={6} ResponseSize={7} ErrorResponseByte={8} TimeInMs={9} ProcessingTimeInMs={10} UserTimeoutInMs={11} OperationTimeoutInMs={12} MaxAllowedTimeoutInMs={13} SlaUsedInMs={14} ReadLatencyInMs={15} WriteLatencyInMs={16} ClientIP={17} UserAgent='{18}' RequestVersion='{19}' ProcessorVersionUsed='{20}' RequestUrl='{21}' ClientRequestId='{22}' MeasurementStatus={23} HttpStatusCode={24} TotalFeTimeInMs={25} TotalTableServerTimeInMs={26} TotalTableServerRoundTripCount={27} TotalPartitionWaitTimeInMs={28} TotalXStreamTimeInMs={29} TotalXStreamRoundTripCount={30} SmbOpLockBreakLatency={31} LastTableServerInstanceName={32} LastTableServerErrorCode={33} TotalAccountCacheWaitTimeInMs={34} TotalContainerCacheWaitTimeInMs={35} InternalStatus={36} RequestContentType='{37}' ResponseContentType='{38}' PartitionKey='{39}' ItemsReturned='{40}' BatchOperationCount='{41}' LastXStreamErrorCode={42} AuthenticationType='{43}' AccountConcurrentReq='{44}' OverallConcurrentReq='{45}' Range='{46}' EntityType='{47}' {48} LastTSPartition={49} TotalXCacheTimeInMs={50} TotalXCacheRoundTripCount={51}";

		public static string AdjustStringLength(string input, int length)
		{
			int num = input.Length;
			if (num >= length)
			{
				return input.Substring(num - length);
			}
			return string.Concat(new string('A', length - num), input);
		}

		public static int ExecuteShellProcess(string processName, string arguments, ref string output, out bool operationTimedOut)
		{
			int exitCode;
			output = "";
			int num = 60;
			operationTimedOut = false;
			using (Process process = new Process())
			{
				process.StartInfo.FileName = processName;
				process.StartInfo.UseShellExecute = false;
				process.StartInfo.CreateNoWindow = true;
				process.StartInfo.RedirectStandardError = true;
				process.StartInfo.RedirectStandardOutput = true;
				process.StartInfo.Arguments = arguments;
				string str = "";
				process.OutputDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs evt) => str = string.Concat(str, evt.Data));
				string str1 = "";
				process.ErrorDataReceived += new DataReceivedEventHandler((object sender, DataReceivedEventArgs evt) => str1 = string.Concat(str1, evt.Data));
				process.Start();
				process.BeginOutputReadLine();
				process.BeginErrorReadLine();
				while (!process.HasExited)
				{
					int num1 = num - 1;
					num = num1;
					if (num1 < 0)
					{
						break;
					}
					Thread.Sleep(TimeSpan.FromSeconds(2));
				}
				if (num >= 0)
				{
					output = string.Concat(str, str1);
					exitCode = process.ExitCode;
				}
				else
				{
					operationTimedOut = true;
					exitCode = 0;
				}
			}
			return exitCode;
		}

		public static IPEndPoint ParseIpEndPoint(string value)
		{
			IPEndPoint pEndPoint;
			try
			{
				IPAddress pAddress = IPAddress.Parse(value);
				int num = 0;
				if (pAddress.AddressFamily != AddressFamily.InterNetworkV6)
				{
					if (pAddress.AddressFamily != AddressFamily.InterNetwork)
					{
						throw new FormatException("IPEndpoint must have IPv4 or IPv6 address");
					}
					int num1 = value.IndexOf(':');
					if (num1 > 0)
					{
						num = int.Parse(value.Substring(num1 + 1));
					}
				}
				else
				{
					int num2 = value.IndexOf("]:");
					if (num2 > 0)
					{
						num = int.Parse(value.Substring(num2 + 2));
					}
				}
				pEndPoint = new IPEndPoint(pAddress, num);
			}
			catch (FormatException formatException)
			{
				int num3 = value.IndexOf(':');
				if (num3 < 0)
				{
					throw;
				}
				IPAddress pAddress1 = IPAddress.Parse(value.Substring(0, num3));
				int num4 = int.Parse(value.Substring(num3 + 1));
				pEndPoint = new IPEndPoint(pAddress1, num4);
			}
			return pEndPoint;
		}
	}
}