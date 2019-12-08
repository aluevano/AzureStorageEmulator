using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class StampSet : HashSet<string>
	{
		private readonly static char[] STAMP_LIST_DELIMITER;

		static StampSet()
		{
			StampSet.STAMP_LIST_DELIMITER = new char[] { ';' };
		}

		public StampSet()
		{
		}

		protected StampSet(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public void AddRange(IEnumerable<string> stamps)
		{
			foreach (string stamp in stamps)
			{
				base.Add(stamp);
			}
		}

		public StampSet Clone()
		{
			StampSet stampSet = new StampSet();
			stampSet.AddRange(this);
			return stampSet;
		}

		public void DeserializeFromString(string stampListString)
		{
			if (string.IsNullOrEmpty(stampListString))
			{
				throw new ArgumentNullException("stampListString");
			}
			string[] strArrays = stampListString.Split(StampSet.STAMP_LIST_DELIMITER, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				base.Add(strArrays[i]);
			}
		}

		public bool Equals(StampSet otherStampSet)
		{
			if (otherStampSet == null)
			{
				throw new ArgumentNullException("otherStampSet");
			}
			return base.SetEquals(otherStampSet);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			base.GetObjectData(info, context);
		}

		public string GetSingleStamp()
		{
			string current;
			HashSet<string>.Enumerator enumerator = base.GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					current = enumerator.Current;
				}
				else
				{
					return null;
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			return current;
		}

		public string SerializeToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = true;
			foreach (string str in this)
			{
				if (!flag)
				{
					stringBuilder.Append(StampSet.STAMP_LIST_DELIMITER);
				}
				stringBuilder.Append(str);
				flag = false;
			}
			return stringBuilder.ToString();
		}
	}
}