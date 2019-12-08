using System;
using System.Security.Cryptography;

namespace Microsoft.Cis.Services.Nephos.Common
{
	internal sealed class HMACCryptoCache : ObjectCache<HMAC>
	{
		private const int hmacCacheSize = 200;

		private readonly static HMACCryptoCache instance;

		public static HMACCryptoCache Instance
		{
			get
			{
				return HMACCryptoCache.instance;
			}
		}

		static HMACCryptoCache()
		{
			HMACCryptoCache.instance = new HMACCryptoCache(200);
		}

		private HMACCryptoCache(int maxSize) : base(maxSize)
		{
		}

		public HMAC Acquire(byte[] key)
		{
			HMAC hMAC = this.Acquire();
			if (hMAC == null)
			{
				return new HMACSHA256FISMACompliant(key);
			}
			hMAC.Key = key;
			return hMAC;
		}
	}
}