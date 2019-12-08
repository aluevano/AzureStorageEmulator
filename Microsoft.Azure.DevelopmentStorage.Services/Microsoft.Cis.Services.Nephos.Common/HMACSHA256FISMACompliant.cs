using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class HMACSHA256FISMACompliant : HMAC
	{
		private const uint PROV_RSA_FULL = 1;

		private const uint CRYPT_VERIFYCONTEXT = 4026531840;

		private const byte PLAINTEXTKEYBLOB = 8;

		private const byte CUR_BLOB_VERSION = 2;

		private const uint CALG_RC2 = 26114;

		private const uint CALG_HMAC = 32777;

		private const uint CALG_MD5 = 32771;

		private const uint CALG_SHA_256 = 32780;

		private const uint HP_HMAC_INFO = 5;

		private const uint HP_HASHVAL = 2;

		private const uint CRYPT_IPSEC_HMAC_KEY = 256;

		private IntPtr m_hProv;

		private IntPtr m_hKey;

		private byte[] m_hashValue;

		public override byte[] Key
		{
			get
			{
				return (byte[])this.KeyValue.Clone();
			}
			set
			{
				this.SetKey(value);
				this.KeyValue = (byte[])value.Clone();
			}
		}

		public HMACSHA256FISMACompliant(byte[] key)
		{
			this.InitContext();
			this.SetKey(key);
			this.HashSizeValue = 256;
		}

		protected override void Dispose(bool disposing)
		{
			this.ReleaseKey();
			this.ReleaseContext();
			base.Dispose(disposing);
		}

		protected override void HashCore(byte[] rgb, int ibStart, int cbSize)
		{
			IntPtr intPtr;
			if (this.m_hashValue != null)
			{
				throw new ArgumentException("HashValue must be null.");
			}
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptCreateHash(this.m_hProv, 32777, this.m_hKey, 0, out intPtr))
			{
				this.ProcessPInvokeFailure("CryptCreateHash");
			}
			HMACSHA256FISMACompliant.HMAC_INFO hMACINFO = new HMACSHA256FISMACompliant.HMAC_INFO()
			{
				HashAlgid = 32780
			};
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptSetHashParam(intPtr, 5, hMACINFO, 0))
			{
				this.ProcessPInvokeFailure("CryptSetHashParam");
			}
			byte[] numArray = rgb;
			if (ibStart > 0)
			{
				numArray = new byte[cbSize];
				Array.Copy(rgb, ibStart, numArray, 0, cbSize);
			}
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptHashData(intPtr, numArray, cbSize, 0))
			{
				this.ProcessPInvokeFailure("CryptHashData");
			}
			this.m_hashValue = new byte[32];
			int length = (int)this.m_hashValue.Length;
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptGetHashParam(intPtr, 2, this.m_hashValue, ref length, 0))
			{
				this.ProcessPInvokeFailure("CryptGetHashParam");
			}
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptDestroyHash(intPtr))
			{
				this.ProcessPInvokeFailure("CryptDestroyHash");
			}
		}

		protected override byte[] HashFinal()
		{
			byte[] numArray = (byte[])this.m_hashValue.Clone();
			if (this.m_hashValue != null)
			{
				Array.Clear(this.m_hashValue, 0, (int)this.m_hashValue.Length);
			}
			this.m_hashValue = null;
			return numArray;
		}

		private void InitContext()
		{
			if (this.m_hProv == IntPtr.Zero && !HMACSHA256FISMACompliant.NativeMethods.CryptAcquireContext(out this.m_hProv, null, null, 1, -268435456))
			{
				this.ProcessPInvokeFailure("CryptAcquireContext");
			}
		}

		public override void Initialize()
		{
		}

		private void ProcessPInvokeFailure(string methodName)
		{
			Win32Exception win32Exception = new Win32Exception();
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] objArray = new object[] { methodName, win32Exception.ErrorCode, win32Exception.NativeErrorCode, win32Exception.Message };
			error.Log("{0} failed with ErrorCode = {1}, NativeErrorCode = {2} and Message = {3}.", objArray);
			throw win32Exception;
		}

		private void ReleaseContext()
		{
			if (this.m_hProv != IntPtr.Zero)
			{
				if (!HMACSHA256FISMACompliant.NativeMethods.CryptReleaseContext(this.m_hProv, 0))
				{
					this.ProcessPInvokeFailure("CryptReleaseContext");
				}
				this.m_hProv = IntPtr.Zero;
			}
		}

		private void ReleaseKey()
		{
			if (this.m_hKey != IntPtr.Zero)
			{
				if (!HMACSHA256FISMACompliant.NativeMethods.CryptDestroyKey(this.m_hKey))
				{
					this.ProcessPInvokeFailure("CryptDestroyKey");
				}
				this.m_hKey = IntPtr.Zero;
			}
		}

		private void SetKey(byte[] key)
		{
			this.ReleaseKey();
			HMACSHA256FISMACompliant.BLOBHEADER length = new HMACSHA256FISMACompliant.BLOBHEADER();
			byte[] numArray = new byte[Marshal.SizeOf(length) + (int)key.Length];
			length.bType = 8;
			length.bVersion = 2;
			length.aiKeyAlg = 26114;
			length.keyLength = (uint)key.Length;
			BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream(numArray));
			binaryWriter.Write(length.bType);
			binaryWriter.Write(length.bVersion);
			binaryWriter.Write(length.reserved);
			binaryWriter.Write(length.aiKeyAlg);
			binaryWriter.Write(length.keyLength);
			binaryWriter.Write(key);
			if (!HMACSHA256FISMACompliant.NativeMethods.CryptImportKey(this.m_hProv, numArray, (int)numArray.Length, IntPtr.Zero, 256, out this.m_hKey))
			{
				this.ProcessPInvokeFailure("CryptImportKey");
			}
		}

		public struct BLOBHEADER
		{
			public byte bType;

			public byte bVersion;

			public short reserved;

			[CLSCompliant(false)]
			public uint aiKeyAlg;

			[CLSCompliant(false)]
			public uint keyLength;
		}

		public class HMAC_INFO
		{
			[CLSCompliant(false)]
			public uint HashAlgid;

			private IntPtr pbInnerString;

			private uint cbInnerString;

			private IntPtr pbOuterString;

			private uint cbOuterString;

			public HMAC_INFO()
			{
			}
		}

		private static class NativeMethods
		{
			[DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptAcquireContext(out IntPtr hProv, string pszContainer, string pszProvider, uint dwProvType, uint dwFlags);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptCreateHash(IntPtr hProv, uint Algid, IntPtr hKey, uint dwFlags, out IntPtr phHash);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptDestroyHash(IntPtr phHash);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptDestroyKey(IntPtr hKey);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptGetHashParam(IntPtr hHash, uint dwParam, byte[] data, ref int pdwDataLen, uint dwFlags);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptHashData(IntPtr hHash, byte[] data, int dataLen, uint flags);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptImportKey(IntPtr hProv, byte[] pbData, int dwDataLen, IntPtr hPubKey, uint dwFlags, out IntPtr phKey);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptReleaseContext(IntPtr hProv, uint dwFlags);

			[DllImport("advapi32.dll", CharSet=CharSet.None, ExactSpelling=false, SetLastError=true)]
			public static extern bool CryptSetHashParam(IntPtr hHash, uint dwParam, [In] HMACSHA256FISMACompliant.HMAC_INFO hmacInfo, uint dwFlags);
		}
	}
}