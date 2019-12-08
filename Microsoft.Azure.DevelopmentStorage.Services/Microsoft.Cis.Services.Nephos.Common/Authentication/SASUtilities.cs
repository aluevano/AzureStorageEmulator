using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class SASUtilities
	{
		private readonly static string[] supportedDateTimeFormats;

		static SASUtilities()
		{
			string[] strArrays = new string[] { "yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'", "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffff'Z'", "yyyy'-'MM'-'dd'T'HH':'mm'Z'", "yyyy'-'MM'-'dd" };
			SASUtilities.supportedDateTimeFormats = strArrays;
		}

		public SASUtilities()
		{
		}

		public static bool ComputeSignatureAndCompare(byte[] stringToSign, byte[] key, string expectedSignature)
		{
			bool flag;
			NephosAssertionException.Assert(key != null);
			using (HMAC hMAC = HMACCryptoCache.Instance.Acquire(key))
			{
				byte[] numArray = hMAC.ComputeHash(stringToSign);
				NephosAssertionException.Assert((int)numArray.Length == hMAC.HashSize / 8);
				flag = AuthenticationManager.AreSignaturesEqual(Convert.ToBase64String(numArray), expectedSignature);
			}
			return flag;
		}

		public static byte[] ComputeSignedKey(byte[] stringToSign, byte[] key)
		{
			byte[] numArray;
			NephosAssertionException.Assert((stringToSign == null ? false : (int)stringToSign.Length > 0));
			NephosAssertionException.Assert(key != null);
			using (HMACSHA512 hMACSHA512 = new HMACSHA512(key))
			{
				byte[] numArray1 = hMACSHA512.ComputeHash(stringToSign);
				NephosAssertionException.Assert((int)numArray1.Length == hMACSHA512.HashSize / 8);
				numArray = numArray1;
			}
			return numArray;
		}

		public static List<SASIdentifier> DecodeSASIdentifiers(string sasIdentifiersValue)
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			if (sasIdentifiersValue == null)
			{
				throw new ArgumentNullException("sasIdentifiersValue");
			}
			SASUtilities.ValidateACIIEncoding(sasIdentifiersValue);
			sasIdentifiersValue = aSCIIEncoding.GetString(Convert.FromBase64String(sasIdentifiersValue));
			List<SASIdentifier> sASIdentifiers = new List<SASIdentifier>();
			using (StringReader stringReader = new StringReader(sasIdentifiersValue))
			{
				string str = null;
				while (true)
				{
					string str1 = stringReader.ReadLine();
					str = str1;
					if (str1 == null)
					{
						break;
					}
					str = str.Trim();
					if (str.Length != 0)
					{
						string str2 = aSCIIEncoding.GetString(Convert.FromBase64String(str));
						SASIdentifier sASIdentifier = new SASIdentifier();
						sASIdentifier.Decode(str2);
						sASIdentifiers.Add(sASIdentifier);
					}
				}
			}
			return sASIdentifiers;
		}

		public static string EncodeSASIdentifiers(List<SASIdentifier> sasIdentifiers)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (SASIdentifier sasIdentifier in sasIdentifiers)
			{
				if (stringBuilder.Length != 0)
				{
					stringBuilder.Append(Environment.NewLine);
				}
				stringBuilder.Append(Convert.ToBase64String(sasIdentifier.Encode()));
			}
			return Convert.ToBase64String((new ASCIIEncoding()).GetBytes(stringBuilder.ToString()));
		}

		public static string EncodeSASPermission(SASPermission permission)
		{
			string[] strArrays = new string[] { ((permission & SASPermission.Read) == SASPermission.Read ? "r" : ""), ((permission & SASPermission.Write) == SASPermission.Write ? "w" : ""), ((permission & SASPermission.Process) == SASPermission.Process ? "p" : ""), ((permission & SASPermission.Add) == SASPermission.Add ? "a" : ""), ((permission & SASPermission.Create) == SASPermission.Create ? "c" : ""), ((permission & SASPermission.Update) == SASPermission.Update ? "u" : ""), ((permission & SASPermission.Delete) == SASPermission.Delete ? "d" : ""), ((permission & SASPermission.List) == SASPermission.List ? "l" : "") };
			return string.Concat(strArrays);
		}

		public static string EncodeSASPermissionWithOrder(SASPermission permission)
		{
			string[] strArrays = new string[] { ((permission & SASPermission.Read) == SASPermission.Read ? "r" : ""), ((permission & SASPermission.Add) == SASPermission.Add ? "a" : ""), ((permission & SASPermission.Create) == SASPermission.Create ? "c" : ""), ((permission & SASPermission.Update) == SASPermission.Update ? "u" : ""), ((permission & SASPermission.Write) == SASPermission.Write ? "w" : ""), ((permission & SASPermission.Process) == SASPermission.Process ? "p" : ""), ((permission & SASPermission.Delete) == SASPermission.Delete ? "d" : ""), ((permission & SASPermission.List) == SASPermission.List ? "l" : "") };
			return string.Concat(strArrays);
		}

		public static string EncodeTime(DateTime datetime)
		{
			return datetime.ToString(SASUtilities.supportedDateTimeFormats[1]);
		}

		public static SASExtraPermission? ParseExtraPermission(string extraPermission)
		{
			SASExtraPermission? nullable = null;
			if (string.IsNullOrEmpty(extraPermission))
			{
				return nullable;
			}
			for (int i = 0; i < extraPermission.Length; i++)
			{
				if (extraPermission[i] != 'b')
				{
					throw new FormatException(string.Concat("Unexpected character ", extraPermission[i], " in extra permission"));
				}
				nullable = SASUtilities.ValidateAndAddExtraPermission(nullable, SASExtraPermission.BypassAccountNetworkAcls, i);
			}
			return nullable;
		}

		public static FileSASAccessLevel ParseFileSASAccessLevel(string signedResource)
		{
			if (string.IsNullOrEmpty(signedResource))
			{
				throw new ArgumentException("signedResource");
			}
			FileSASAccessLevel fileSASAccessLevel = FileSASAccessLevel.None;
			string str = signedResource;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "f")
				{
					fileSASAccessLevel = FileSASAccessLevel.File;
				}
				else
				{
					if (str1 != "s")
					{
						throw new FormatException(string.Concat("Unknown signed resource ", signedResource));
					}
					fileSASAccessLevel = FileSASAccessLevel.Share;
				}
				return fileSASAccessLevel;
			}
			throw new FormatException(string.Concat("Unknown signed resource ", signedResource));
		}

		public static SASAccessLevel ParseSasAccessLevel(string signedResource)
		{
			if (string.IsNullOrEmpty(signedResource))
			{
				throw new ArgumentException("signedResource");
			}
			SASAccessLevel sASAccessLevel = SASAccessLevel.None;
			string str = signedResource;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "b")
				{
					sASAccessLevel = SASAccessLevel.Blob;
				}
				else
				{
					if (str1 != "c")
					{
						throw new FormatException(string.Concat("Unknown signed resource ", signedResource));
					}
					sASAccessLevel = SASAccessLevel.Container;
				}
				return sASAccessLevel;
			}
			throw new FormatException(string.Concat("Unknown signed resource ", signedResource));
		}

		public static SASPermission ParseSASPermission(string text)
		{
			SASPermission sASPermission = SASPermission.None;
			string str = text;
			int num = 0;
			while (true)
			{
				if (num >= str.Length)
				{
					return sASPermission;
				}
				char chr = str[num];
				if (chr > 'l')
				{
					switch (chr)
					{
						case 'p':
						{
							sASPermission |= SASPermission.Process;
							break;
						}
						case 'q':
						{
							throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
						}
						case 'r':
						{
							sASPermission |= SASPermission.Read;
							break;
						}
						default:
						{
							switch (chr)
							{
								case 'u':
								{
									sASPermission |= SASPermission.Update;
									break;
								}
								case 'v':
								{
									throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
								}
								case 'w':
								{
									sASPermission |= SASPermission.Write;
									break;
								}
								default:
								{
									throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
								}
							}
							break;
						}
					}
				}
				else
				{
					switch (chr)
					{
						case 'a':
						{
							sASPermission |= SASPermission.Add;
							break;
						}
						case 'b':
						{
							throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
						}
						case 'c':
						{
							sASPermission |= SASPermission.Create;
							break;
						}
						case 'd':
						{
							sASPermission |= SASPermission.Delete;
							break;
						}
						default:
						{
							if (chr == 'l')
							{
								sASPermission |= SASPermission.List;
								break;
							}
							else
							{
								throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
							}
						}
					}
				}
				num++;
			}
			throw new FormatException(string.Concat("Invalid signed permission. permission = ", text));
		}

		public static SasProtocol ParseSignedProtocol(string spr)
		{
			if (string.IsNullOrWhiteSpace(spr))
			{
				throw new FormatException("Invalid signedProtocol");
			}
			if (spr.Equals("http"))
			{
				throw new FormatException("Invalid signed protocol. Specifying only http is not allowed");
			}
			if (spr.Equals("https"))
			{
				return SasProtocol.Https;
			}
			if (!spr.Equals("http,https") && !spr.Equals("https,http"))
			{
				throw new FormatException(string.Concat("Invalid signedProtocol ", spr));
			}
			return SasProtocol.All;
		}

		public static IPAddressRange ParseSip(string sip)
		{
			if (string.IsNullOrWhiteSpace(sip))
			{
				throw new FormatException("sip is optional but cannot be empty");
			}
			string[] strArrays = sip.Split(new char[] { '-' });
			if ((int)strArrays.Length > 2)
			{
				throw new FormatException("sip only supports one range");
			}
			if ((int)strArrays.Length != 2)
			{
				return IPAddressRange.ParseIPV4(sip);
			}
			return IPAddressRange.ParseIPV4(strArrays[0], strArrays[1]);
		}

		[SuppressMessage("Anvil.RdUsage!TimeUtc", "27102", Justification="We eventually set the kind using DateTime.SpecifyKind.")]
		public static DateTime ParseTime(string datetime)
		{
			DateTime dateTime = DateTime.ParseExact(datetime, SASUtilities.supportedDateTimeFormats, null, DateTimeStyles.None);
			return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
		}

		[SuppressMessage("Anvil.RdUsage!TimeUtc", "27102", Justification="We eventually set the kind using DateTime.SpecifyKind.")]
		public static bool TryParseTime(string datetime, out DateTime result)
		{
			bool flag = DateTime.TryParseExact(datetime, SASUtilities.supportedDateTimeFormats, null, DateTimeStyles.None, out result);
			if (flag)
			{
				result = DateTime.SpecifyKind(result, DateTimeKind.Utc);
			}
			return flag;
		}

		public static bool ValidateACIIEncoding(string text)
		{
			ASCIIEncoding encoding = Encoding.GetEncoding(Encoding.ASCII.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback) as ASCIIEncoding;
			try
			{
				encoding.GetByteCount(text);
			}
			catch (EncoderFallbackException encoderFallbackException1)
			{
				EncoderFallbackException encoderFallbackException = encoderFallbackException1;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] index = new object[] { encoderFallbackException.Index };
				throw new FormatException(string.Format(invariantCulture, "Character at index {0} is not valid ASCII", index), encoderFallbackException);
			}
			return true;
		}

		private static SASExtraPermission? ValidateAndAddExtraPermission(SASExtraPermission? currExtraPermissions, SASExtraPermission newExtraPermission, int position)
		{
			if (currExtraPermissions.HasValue)
			{
				if ((currExtraPermissions.Value & newExtraPermission) == newExtraPermission)
				{
					object[] objArray = new object[] { "Invalid duplicate extra permission. Error at index ", position, " of ", currExtraPermissions };
					throw new FormatException(string.Concat(objArray));
				}
				newExtraPermission |= currExtraPermissions.Value;
			}
			return new SASExtraPermission?(newExtraPermission);
		}

		public static void ValidatePermissionOrdering(string permission, SASPermission validPermissions)
		{
			char chr;
			if (string.IsNullOrEmpty(permission))
			{
				throw new ArgumentException(permission);
			}
			SASPermission sASPermission = SASPermission.None;
			int num = 0;
			string str = permission;
			int num1 = 0;
			while (true)
			{
				if (num1 >= str.Length)
				{
					return;
				}
				chr = str[num1];
				char chr1 = chr;
				if (chr1 > 'l')
				{
					switch (chr1)
					{
						case 'p':
						{
							if ((sASPermission & SASPermission.Process) == SASPermission.Process)
							{
								object[] objArray = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
								throw new FormatException(string.Concat(objArray));
							}
							sASPermission |= SASPermission.Process;
							break;
						}
						case 'q':
						{
							throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
						}
						case 'r':
						{
							if ((sASPermission & SASPermission.Read) == SASPermission.Read || (sASPermission & (SASPermission.Write | SASPermission.Add | SASPermission.Update | SASPermission.Process | SASPermission.Delete | SASPermission.List | SASPermission.Create)) != SASPermission.None)
							{
								object[] objArray1 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
								throw new FormatException(string.Concat(objArray1));
							}
							sASPermission |= SASPermission.Read;
							break;
						}
						default:
						{
							switch (chr1)
							{
								case 'u':
								{
									if ((sASPermission & SASPermission.Update) == SASPermission.Update || (sASPermission & (SASPermission.Process | SASPermission.Delete | SASPermission.List)) != SASPermission.None)
									{
										object[] objArray2 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
										throw new FormatException(string.Concat(objArray2));
									}
									sASPermission |= SASPermission.Update;
									break;
								}
								case 'v':
								{
									throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
								}
								case 'w':
								{
									if ((sASPermission & SASPermission.Write) == SASPermission.Write || (sASPermission & (SASPermission.Update | SASPermission.Process | SASPermission.Delete | SASPermission.List)) != SASPermission.None)
									{
										object[] objArray3 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
										throw new FormatException(string.Concat(objArray3));
									}
									sASPermission |= SASPermission.Write;
									break;
								}
								default:
								{
									throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
								}
							}
							break;
						}
					}
				}
				else
				{
					switch (chr1)
					{
						case 'a':
						{
							if ((sASPermission & SASPermission.Add) == SASPermission.Add || (sASPermission & (SASPermission.Write | SASPermission.Update | SASPermission.Process | SASPermission.Delete | SASPermission.List | SASPermission.Create)) != SASPermission.None)
							{
								object[] objArray4 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
								throw new FormatException(string.Concat(objArray4));
							}
							sASPermission |= SASPermission.Add;
							break;
						}
						case 'b':
						{
							throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
						}
						case 'c':
						{
							if ((sASPermission & SASPermission.Create) == SASPermission.Create || (sASPermission & (SASPermission.Update | SASPermission.Process | SASPermission.Delete | SASPermission.List)) != SASPermission.None)
							{
								object[] objArray5 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
								throw new FormatException(string.Concat(objArray5));
							}
							sASPermission |= SASPermission.Create;
							break;
						}
						case 'd':
						{
							if ((sASPermission & SASPermission.Delete) == SASPermission.Delete || (sASPermission & SASPermission.List) != SASPermission.None)
							{
								object[] objArray6 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
								throw new FormatException(string.Concat(objArray6));
							}
							sASPermission |= SASPermission.Delete;
							break;
						}
						default:
						{
							if (chr1 == 'l')
							{
								if ((sASPermission & SASPermission.List) == SASPermission.List)
								{
									object[] objArray7 = new object[] { "Invalid order of permission. Error at index ", num, " of ", permission };
									throw new FormatException(string.Concat(objArray7));
								}
								sASPermission |= SASPermission.List;
								break;
							}
							else
							{
								throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
							}
						}
					}
				}
				if ((sASPermission & ~validPermissions) != SASPermission.None)
				{
					object[] objArray8 = new object[] { "Character ", chr, " is an invalid permission for ", validPermissions };
					throw new FormatException(string.Concat(objArray8));
				}
				num++;
				num1++;
			}
			throw new FormatException(string.Concat("Unexpected character ", chr, " in permission"));
		}

		public static bool ValidateUnicodeEncoding(string text)
		{
			UnicodeEncoding encoding = Encoding.GetEncoding(Encoding.Unicode.CodePage, EncoderFallback.ExceptionFallback, DecoderFallback.ExceptionFallback) as UnicodeEncoding;
			try
			{
				encoding.GetByteCount(text);
			}
			catch (EncoderFallbackException encoderFallbackException1)
			{
				EncoderFallbackException encoderFallbackException = encoderFallbackException1;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] index = new object[] { encoderFallbackException.Index };
				throw new FormatException(string.Format(invariantCulture, "Character at index {0} is not valid Unicode", index), encoderFallbackException);
			}
			return true;
		}
	}
}