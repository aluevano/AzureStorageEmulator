using AsyncHelper;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class MetadataEncoding
	{
		private const string StartCharacterRegex = "_|[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}]";

		private const string IdentifierCharacterRegexForSept09 = "[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}]";

		private static Regex MetadataKeyRegexForSept09;

		private static LateBoundMethod validateMetadataMethod;

		static MetadataEncoding()
		{
			MetadataEncoding.MetadataKeyRegexForSept09 = new Regex("^(_|[\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}])([\\p{Lu}\\p{Ll}\\p{Lt}\\p{Lm}\\p{Lo}\\p{Nl}\\p{Mn}\\p{Mc}\\p{Nd}\\p{Pc}\\p{Cf}])*$", RegexOptions.Compiled);
			MetadataEncoding.validateMetadataMethod = typeof(WebHeaderCollection).GetMethod("CheckBadChars", BindingFlags.Static | BindingFlags.NonPublic).GetLateBoundMethod();
		}

		private static Encoding CreateAsciiEncoder()
		{
			return Encoding.GetEncoding(Encoding.ASCII.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback);
		}

		public static void Decode(byte[] metadata, NameValueCollection container)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}
			container.Clear();
			string str = null;
			try
			{
				str = MetadataEncoding.CreateAsciiEncoder().GetString(metadata);
			}
			catch (DecoderFallbackException decoderFallbackException1)
			{
				DecoderFallbackException decoderFallbackException = decoderFallbackException1;
				string str1 = string.Join(" ", Array.ConvertAll<byte, string>(decoderFallbackException.BytesUnknown, (byte b) => string.Format(CultureInfo.InvariantCulture, "{0:X2}", new object[] { b })));
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] index = new object[] { str1, decoderFallbackException.Index };
				throw new MetadataFormatException(string.Format(invariantCulture, "Failed to decode metadata because bytes at index {1} are not valid ASCII. Unknown bytes: '{0}'", index), decoderFallbackException);
			}
			using (StringReader stringReader = new StringReader(str))
			{
				string str2 = null;
				while (true)
				{
					string str3 = stringReader.ReadLine();
					str2 = str3;
					if (str3 == null)
					{
						break;
					}
					str2 = str2.Trim();
					if (str2.Length != 0)
					{
						char[] chrArray = new char[] { ':' };
						string[] strArrays = str2.Split(chrArray, 2);
						if ((int)strArrays.Length != 2)
						{
							CultureInfo cultureInfo = CultureInfo.InvariantCulture;
							object[] objArray = new object[] { str2 };
							throw new MetadataFormatException(string.Format(cultureInfo, "Metadata is incorrectly formatted: '{0}'", objArray));
						}
						string str4 = strArrays[0];
						string str5 = strArrays[1];
						if (str4.Length == 0)
						{
							CultureInfo invariantCulture1 = CultureInfo.InvariantCulture;
							object[] objArray1 = new object[] { str2 };
							throw new MetadataFormatException(string.Format(invariantCulture1, "Metadata key is empty: '{0}'", objArray1));
						}
						container.Set(str4, str5);
					}
				}
			}
		}

		public static byte[] Encode(NameValueCollection metadata)
		{
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}
			int length = 0;
			string[] allKeys = metadata.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				length = length + str.Length + 1 + metadata[str].Length + Environment.NewLine.Length;
			}
			byte[] numArray = new byte[length];
			int bytes = 0;
			Encoding encoding = MetadataEncoding.CreateAsciiEncoder();
			try
			{
				for (int j = 0; j < metadata.Count; j++)
				{
					string item = metadata.Keys[j];
					try
					{
						bytes += encoding.GetBytes(item, 0, item.Length, numArray, bytes);
					}
					catch (EncoderFallbackException encoderFallbackException)
					{
						throw new MetadataFormatException(string.Concat("Metadata key is not valid ASCII: ", item), encoderFallbackException);
					}
					bytes += encoding.GetBytes(":", 0, 1, numArray, bytes);
					string item1 = metadata[j];
					try
					{
						bytes += encoding.GetBytes(item1, 0, item1.Length, numArray, bytes);
					}
					catch (EncoderFallbackException encoderFallbackException1)
					{
						throw new MetadataFormatException(string.Concat("Metadata value is not valid ASCII: ", item1), encoderFallbackException1);
					}
					bytes += encoding.GetBytes(Environment.NewLine, 0, Environment.NewLine.Length, numArray, bytes);
				}
			}
			catch (ArgumentException argumentException)
			{
				NephosAssertionException.Fail("Byte array not long enough to encode metadata", argumentException);
			}
			return numArray;
		}

		public static byte[] EncodeStringCollection(ICollection<string> metadata)
		{
			byte[] array;
			int num = 2147483647;
			if (metadata == null)
			{
				throw new ArgumentNullException("metadata");
			}
			using (MemoryStream memoryStream = null)
			{
				try
				{
					memoryStream = new MemoryStream();
					BinaryWriter binaryWriter = null;
					try
					{
						binaryWriter = new BinaryWriter(memoryStream);
						binaryWriter.Write((uint)metadata.Count);
						foreach (string metadatum in metadata)
						{
							string empty = metadatum ?? string.Empty;
							empty = (empty.Length <= num ? empty : empty.Substring(0, num));
							try
							{
								byte[] bytes = Encoding.UTF8.GetBytes(empty);
								binaryWriter.Write((uint)((int)bytes.Length));
								binaryWriter.Write(bytes);
							}
							catch (EncoderFallbackException encoderFallbackException)
							{
								throw new MetadataFormatException(string.Format("String value [{0}] isn't valid UTF-8", empty), encoderFallbackException);
							}
							catch (IOException oException)
							{
								throw new IOException("Can't write the encoded string to memory stream", oException);
							}
						}
						array = memoryStream.ToArray();
					}
					finally
					{
						if (binaryWriter != null)
						{
							binaryWriter.Dispose();
							memoryStream = null;
						}
					}
				}
				catch (ArgumentException argumentException)
				{
					throw new ArgumentException("Can't convert the string collection to byte array", argumentException);
				}
			}
			return array;
		}

		public static void EnsureMetadataEntryIsValid(string metadataKey, string metadataValue, string version)
		{
			if (!MetadataEncoding.IsValidMetadataKey(metadataKey, version))
			{
				throw new MetadataFormatException(string.Format("The metadata key {0} is not valid for version '{1}'.", metadataKey, version));
			}
			if (!MetadataEncoding.IsValidMetadataValue(metadataValue, version))
			{
				throw new MetadataFormatException(string.Format("The metadata value {0} is not valid for version '{1}'.", metadataValue, version));
			}
		}

		public static int GetMetadataLengthWithAsciiEncoding(NameValueCollection metadata)
		{
			int byteCount = 0;
			int num = 1;
			Encoding encoding = MetadataEncoding.CreateAsciiEncoder();
			string[] allKeys = metadata.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				try
				{
					byteCount = byteCount + encoding.GetByteCount(str) + num + encoding.GetByteCount(metadata[str]) + Environment.NewLine.Length;
				}
				catch (EncoderFallbackException encoderFallbackException1)
				{
					EncoderFallbackException encoderFallbackException = encoderFallbackException1;
					throw new MetadataFormatException(string.Format("Metadata pair {0}:{1} has an invalid char:{2}", str, metadata[str], encoderFallbackException.CharUnknown), encoderFallbackException);
				}
			}
			return byteCount;
		}

		public static bool IsValidMetadataKey(string metadataKey, string version)
		{
			bool flag;
			try
			{
				LateBoundMethod lateBoundMethod = MetadataEncoding.validateMetadataMethod;
				object[] objArray = new object[] { metadataKey, false };
				lateBoundMethod(null, objArray);
				if (VersioningHelper.IsPreSeptember09OrInvalidVersion(version))
				{
					return true;
				}
				return MetadataEncoding.MetadataKeyRegexForSept09.IsMatch(metadataKey);
			}
			catch (ArgumentException argumentException)
			{
				flag = false;
			}
			return flag;
		}

		public static bool IsValidMetadataValue(string metadataValue, string version)
		{
			bool flag;
			try
			{
				LateBoundMethod lateBoundMethod = MetadataEncoding.validateMetadataMethod;
				object[] objArray = new object[] { metadataValue, true };
				lateBoundMethod(null, objArray);
				return true;
			}
			catch (ArgumentException argumentException)
			{
				flag = false;
			}
			return flag;
		}

		public static void Validate(string text)
		{
			if (text == null)
			{
				throw new ArgumentNullException("text");
			}
			try
			{
				MetadataEncoding.CreateAsciiEncoder().GetByteCount(text);
			}
			catch (EncoderFallbackException encoderFallbackException1)
			{
				EncoderFallbackException encoderFallbackException = encoderFallbackException1;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] index = new object[] { encoderFallbackException.Index };
				throw new FormatException(string.Format(invariantCulture, "Character at index {0} is not valid ASCII", index), encoderFallbackException);
			}
		}

		public static void WriteMetadataToXml(XmlWriter xmlWriter, NameValueCollection metadata, bool isWritingRootMetadataElementEvenIfMetadataIsEmpty, string version)
		{
			if (xmlWriter == null)
			{
				throw new ArgumentNullException("xmlWriter");
			}
			if (metadata == null || metadata.Count <= 0)
			{
				if (isWritingRootMetadataElementEvenIfMetadataIsEmpty)
				{
					xmlWriter.WriteStartElement("Metadata");
					xmlWriter.WriteEndElement();
				}
				return;
			}
			xmlWriter.WriteStartElement("Metadata");
			string[] allKeys = metadata.AllKeys;
			for (int i = 0; i < (int)allKeys.Length; i++)
			{
				string str = allKeys[i];
				string item = metadata[str];
				if (!MetadataEncoding.IsValidMetadataKey(str, version))
				{
					xmlWriter.WriteElementString("x-ms-invalid-name", str);
				}
				else if (MetadataEncoding.IsValidMetadataValue(item, version))
				{
					xmlWriter.WriteElementString(str, metadata[str]);
				}
				else
				{
					xmlWriter.WriteElementString("x-ms-invalid-value", item);
				}
			}
			xmlWriter.WriteEndElement();
		}
	}
}