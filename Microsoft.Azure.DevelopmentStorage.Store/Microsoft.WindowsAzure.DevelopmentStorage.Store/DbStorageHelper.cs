using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.IO;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public static class DbStorageHelper
	{
		internal static byte[] GetByteArrayFromStream(Stream inputStream, long contentLength, bool isPutBlockOrPutPage, bool isLargeBlockBlobRequest)
		{
			byte[] array;
			long num = (isLargeBlockBlobRequest ? (long)268435456 : (long)67108864);
			try
			{
				if (inputStream == null)
				{
					throw new ArgumentNullException("inputStream", "inputStream cannot be null");
				}
				if (contentLength == (long)0 && isPutBlockOrPutPage)
				{
					throw new ArgumentOutOfRangeException("inputStream length cannot be zero");
				}
				if (contentLength > num)
				{
					throw new BlobContentTooLargeException(new long?(num));
				}
				byte[] numArray = new byte[checked((IntPtr)contentLength)];
				long num1 = contentLength;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					while (num1 > (long)0)
					{
						int num2 = inputStream.Read(numArray, 0, (int)num1);
						if (num2 <= 0)
						{
							continue;
						}
						memoryStream.Write(numArray, 0, num2);
						num1 -= (long)num2;
					}
					array = memoryStream.ToArray();
				}
			}
			catch (Exception exception)
			{
				SqlExceptionManager.ReThrowException(exception);
				throw;
			}
			return array;
		}

		internal static byte[] GetByteArrayFromStream(Stream inputStream, out long contentLength, bool isBlock, bool isLargeBlockBlobRequest)
		{
			byte[] array;
			long num = (isLargeBlockBlobRequest ? (long)268435456 : (long)67108864);
			try
			{
				if (inputStream == null)
				{
					throw new ArgumentNullException("inputStream", "inputStream cannot be null");
				}
				if (inputStream.Length > num)
				{
					throw new BlobContentTooLargeException(new long?(num), string.Format("Request size exceeded limit of {0} bytes", num), null);
				}
				byte[] numArray = new byte[checked((IntPtr)inputStream.Length)];
				int num1 = 0;
				long length = inputStream.Length;
				using (MemoryStream memoryStream = new MemoryStream())
				{
					do
					{
						length -= (long)num1;
						num1 = inputStream.Read(numArray, 0, (int)length);
						if (num1 <= 0)
						{
							continue;
						}
						memoryStream.Write(numArray, 0, num1);
					}
					while (num1 > 0);
					contentLength = memoryStream.Length;
					if (isBlock && contentLength == (long)0)
					{
						throw new ArgumentOutOfRangeException("inputStream length cannot be zero");
					}
					array = memoryStream.ToArray();
				}
			}
			catch (Exception exception)
			{
				SqlExceptionManager.ReThrowException(exception);
				throw;
			}
			return array;
		}

		internal static CrcStream GetStreamFromByteArray(byte[] inputBytes)
		{
			CrcStream crcMemoryStream;
			try
			{
				if (inputBytes == null)
				{
					throw new ArgumentNullException("inputBytes", "inputBytes cannot be null");
				}
				crcMemoryStream = new CrcMemoryStream(inputBytes);
			}
			catch (Exception exception)
			{
				SqlExceptionManager.ReThrowException(exception);
				throw;
			}
			return crcMemoryStream;
		}
	}
}