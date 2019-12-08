using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[DataContract]
	public class IPAddressRange : IComparable<IPAddressRange>, ICloneable
	{
		private readonly static char subnetMaskDelimiter;

		[DataMember]
		public IPAddress EndingIPAddress
		{
			get;
			private set;
		}

		[DataMember]
		public IPAddress StartingIPAddress
		{
			get;
			private set;
		}

		static IPAddressRange()
		{
			IPAddressRange.subnetMaskDelimiter = '/';
		}

		public IPAddressRange()
		{
		}

		public IPAddressRange(IPAddress startAddress, IPAddress endAddress)
		{
			if (startAddress == null)
			{
				throw new ArgumentNullException("startAddress");
			}
			if (endAddress == null)
			{
				throw new ArgumentNullException("endAddress");
			}
			this.StartingIPAddress = startAddress;
			this.EndingIPAddress = endAddress;
			if (startAddress.CompareTo(endAddress) > 0)
			{
				throw new ArgumentOutOfRangeException("startAddress", this.StartingIPAddress, "The startAddress must not exceed the endAddress.");
			}
		}

		public IPAddressRange(long startAddressValue, long endAddressValue) : this(IPAddressRange.GetIPV4Address(startAddressValue), IPAddressRange.GetIPV4Address(endAddressValue))
		{
		}

		public static IPAddressRange BinarySearchRanges(IPAddress ipAddress, List<IPAddressRange> addressRanges)
		{
			if (ipAddress.AddressFamily != AddressFamily.InterNetwork || addressRanges == null)
			{
				return null;
			}
			int num = 0;
			int count = addressRanges.Count - 1;
			while (num <= count)
			{
				int num1 = num + (count - num) / 2;
				IPAddressRange item = addressRanges[num1];
				if (ipAddress.CompareTo(item.StartingIPAddress) >= 0)
				{
					if (ipAddress.CompareTo(item.EndingIPAddress) <= 0)
					{
						return item;
					}
					num = num1 + 1;
				}
				else
				{
					count = num1 - 1;
				}
			}
			return null;
		}

		public object Clone()
		{
			IPAddressRange pAddressRange = new IPAddressRange()
			{
				StartingIPAddress = this.StartingIPAddress,
				EndingIPAddress = this.EndingIPAddress
			};
			return pAddressRange;
		}

		public int CompareTo(IPAddressRange other)
		{
			int num = this.StartingIPAddress.CompareTo(other.StartingIPAddress);
			if (num != 0)
			{
				return num;
			}
			return this.EndingIPAddress.CompareTo(other.EndingIPAddress);
		}

		public bool Contains(IPAddress ipAddress)
		{
			if (ipAddress.CompareTo(this.StartingIPAddress) < 0)
			{
				return false;
			}
			return ipAddress.CompareTo(this.EndingIPAddress) <= 0;
		}

		private static IPAddress GetIPV4Address(long addressValue)
		{
			byte[] numArray = new byte[] { (byte)(addressValue >> 24), (byte)(addressValue >> 16), (byte)(addressValue >> 8), (byte)addressValue };
			return new IPAddress(numArray);
		}

		private static long GetIPV4AddressValue(IPAddress address)
		{
			byte[] addressBytes = address.GetAddressBytes();
			if ((int)addressBytes.Length != 4)
			{
				throw new FormatException(string.Concat("IPv4 format expected: ", address.ToString()));
			}
			return (long)(((ulong)addressBytes[0] << 24) + ((ulong)addressBytes[1] << 16) + ((ulong)addressBytes[2] << 8) + (ulong)addressBytes[3]);
		}

		public static bool IsContainedInRange(IPAddress ipAddress, IPAddressRange addressRange, out IPAddressRange containedRange)
		{
			return IPAddressRange.IsContainedInRanges(ipAddress, new List<IPAddressRange>()
			{
				addressRange
			}, out containedRange);
		}

		public static bool IsContainedInRanges(IPAddress ipAddress, List<IPAddressRange> addressRanges, out IPAddressRange containedRange)
		{
			bool flag;
			containedRange = null;
			if (ipAddress.AddressFamily != AddressFamily.InterNetwork || addressRanges == null)
			{
				return false;
			}
			List<IPAddressRange>.Enumerator enumerator = addressRanges.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					IPAddressRange current = enumerator.Current;
					if (!current.Contains(ipAddress))
					{
						continue;
					}
					containedRange = current;
					flag = true;
					return flag;
				}
				return false;
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return flag;
		}

		public static void MakeSortedUniqueAddressRanges(ref List<IPAddressRange> addressRanges)
		{
			if (addressRanges == null || addressRanges.Count <= 1)
			{
				return;
			}
			addressRanges.Sort();
			List<IPAddressRange> pAddressRanges = new List<IPAddressRange>();
			IPAddressRange item = addressRanges[0];
			for (int i = 1; i < addressRanges.Count; i++)
			{
				if (addressRanges[i].StartingIPAddress.CompareTo(item.EndingIPAddress) > 0)
				{
					pAddressRanges.Add(item);
					item = addressRanges[i];
				}
				else if (addressRanges[i].EndingIPAddress.CompareTo(item.EndingIPAddress) > 0)
				{
					item.EndingIPAddress = addressRanges[i].EndingIPAddress;
				}
			}
			pAddressRanges.Add(item);
			addressRanges = pAddressRanges;
		}

		public static IPAddressRange ParseIPV4(string addressString)
		{
			IPAddress pAddress;
			int num;
			if (!addressString.Contains(IPAddressRange.subnetMaskDelimiter.ToString()))
			{
				char[] chrArray = new char[] { '.' };
				if ((int)addressString.Split(chrArray).Length < 4)
				{
					throw new FormatException(string.Concat("IPv4 dotted-quad notation expected: ", addressString));
				}
				IPAddress pAddress1 = IPAddress.Parse(addressString);
				return new IPAddressRange(pAddress1, pAddress1);
			}
			char[] chrArray1 = new char[] { IPAddressRange.subnetMaskDelimiter };
			string[] strArrays = addressString.Split(chrArray1);
			if ((int)strArrays.Length != 2)
			{
				throw new FormatException(string.Concat("Invalid CIDR notation: ", addressString));
			}
			string str = strArrays[0];
			char[] chrArray2 = new char[] { '.' };
			if ((int)str.Split(chrArray2).Length < 4)
			{
				throw new FormatException(string.Concat("IPv4 dotted-quad notation expected: ", addressString));
			}
			if (!IPAddress.TryParse(strArrays[0], out pAddress))
			{
				throw new FormatException(string.Concat("Invalid ip specified: ", addressString));
			}
			if (!int.TryParse(strArrays[1], out num))
			{
				throw new FormatException(string.Concat("Invalid network prefix specified: ", addressString));
			}
			if (num > 30)
			{
				throw new FormatException(string.Concat("Invalid network prefix specified: ", addressString));
			}
			uint num1 = (uint)(~(-1 >> (num & 31)));
			long pV4AddressValue = (long)(IPAddressRange.GetIPV4AddressValue(pAddress) & (ulong)num1);
			long num2 = (long)(pV4AddressValue + (ulong)(~num1));
			long num3 = pV4AddressValue + (long)1;
			return new IPAddressRange(num3, num2 - (long)1);
		}

		public static IPAddressRange ParseIPV4(string startAddressString, string endAddressString)
		{
			char[] chrArray = new char[] { '.' };
			if ((int)startAddressString.Split(chrArray).Length < 4)
			{
				throw new FormatException(string.Concat("IPv4 dotted-quad notation expected: ", startAddressString));
			}
			char[] chrArray1 = new char[] { '.' };
			if ((int)endAddressString.Split(chrArray1).Length < 4)
			{
				throw new FormatException(string.Concat("IPv4 dotted-quad notation expected: ", endAddressString));
			}
			return new IPAddressRange(IPAddress.Parse(startAddressString), IPAddress.Parse(endAddressString));
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] startingIPAddress = new object[] { this.StartingIPAddress, this.EndingIPAddress };
			return string.Format(invariantCulture, "{0}-{1}", startingIPAddress);
		}
	}
}