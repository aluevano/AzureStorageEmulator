using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MeasurementEvents
{
	internal static class DoubleConverter
	{
		private readonly static DoubleConverter.Conversion<bool> FromBoolean;

		private readonly static DoubleConverter.Conversion<byte> FromByte;

		private readonly static DoubleConverter.Conversion<char> FromChar;

		private readonly static DoubleConverter.Conversion<DateTime> FromDateTime;

		private readonly static DoubleConverter.Conversion<decimal> FromDecimal;

		private readonly static DoubleConverter.Conversion<double> FromDouble;

		private readonly static DoubleConverter.Conversion<short> FromInt16;

		private readonly static DoubleConverter.Conversion<int> FromInt32;

		private readonly static DoubleConverter.Conversion<long> FromInt64;

		private readonly static DoubleConverter.Conversion<sbyte> FromSByte;

		private readonly static DoubleConverter.Conversion<float> FromSingle;

		private readonly static DoubleConverter.Conversion<ushort> FromUInt16;

		private readonly static DoubleConverter.Conversion<uint> FromUInt32;

		private readonly static DoubleConverter.Conversion<ulong> FromUInt64;

		private readonly static DoubleConverter.Conversion<object> FromObject;

		private readonly static DoubleConverter.Conversion<TimeSpan> FromTimeSpan;

		static DoubleConverter()
		{
			DoubleConverter.FromBoolean = (bool val) => {
				if (!val)
				{
					return 0;
				}
				return 1;
			};
			DoubleConverter.FromByte = (byte val) => (double)val;
			DoubleConverter.FromChar = (char val) => (double)val;
			DoubleConverter.FromDateTime = (DateTime val) => (double)val.ToFileTimeUtc();
			DoubleConverter.FromDecimal = (decimal val) => (double)((double)val);
			DoubleConverter.FromDouble = (double val) => val;
			DoubleConverter.FromInt16 = (short val) => (double)val;
			DoubleConverter.FromInt32 = (int val) => (double)val;
			DoubleConverter.FromInt64 = (long val) => (double)val;
			DoubleConverter.FromSByte = (sbyte val) => (double)val;
			DoubleConverter.FromSingle = (float val) => (double)val;
			DoubleConverter.FromUInt16 = (ushort val) => (double)val;
			DoubleConverter.FromUInt32 = (uint val) => (double)((float)val);
			DoubleConverter.FromUInt64 = (ulong val) => (double)((float)val);
			DoubleConverter.FromObject = (object val) => Convert.ToDouble(val, CultureInfo.InvariantCulture);
			DoubleConverter.FromTimeSpan = (TimeSpan val) => val.TotalSeconds;
		}

		public static DoubleConverter.Conversion<TInput> GetConversion<TInput>()
		{
			Delegate fromBoolean;
			Type type = typeof(TInput);
			if (type.IsEnum)
			{
				return DoubleConverter.GetEnumConversion<TInput>();
			}
			switch (Type.GetTypeCode(type))
			{
				case TypeCode.Boolean:
				{
					fromBoolean = DoubleConverter.FromBoolean;
					break;
				}
				case TypeCode.Char:
				{
					fromBoolean = DoubleConverter.FromChar;
					break;
				}
				case TypeCode.SByte:
				{
					fromBoolean = DoubleConverter.FromSByte;
					break;
				}
				case TypeCode.Byte:
				{
					fromBoolean = DoubleConverter.FromByte;
					break;
				}
				case TypeCode.Int16:
				{
					fromBoolean = DoubleConverter.FromInt16;
					break;
				}
				case TypeCode.UInt16:
				{
					fromBoolean = DoubleConverter.FromUInt16;
					break;
				}
				case TypeCode.Int32:
				{
					fromBoolean = DoubleConverter.FromInt32;
					break;
				}
				case TypeCode.UInt32:
				{
					fromBoolean = DoubleConverter.FromUInt32;
					break;
				}
				case TypeCode.Int64:
				{
					fromBoolean = DoubleConverter.FromInt64;
					break;
				}
				case TypeCode.UInt64:
				{
					fromBoolean = DoubleConverter.FromUInt64;
					break;
				}
				case TypeCode.Single:
				{
					fromBoolean = DoubleConverter.FromSingle;
					break;
				}
				case TypeCode.Double:
				{
					fromBoolean = DoubleConverter.FromDouble;
					break;
				}
				case TypeCode.Decimal:
				{
					fromBoolean = DoubleConverter.FromDecimal;
					break;
				}
				case TypeCode.DateTime:
				{
					fromBoolean = DoubleConverter.FromDateTime;
					break;
				}
				default:
				{
					if (typeof(TimeSpan) != type)
					{
						fromBoolean = new DoubleConverter.Conversion<TInput>((TInput val) => DoubleConverter.FromObject(val));
						break;
					}
					else
					{
						fromBoolean = DoubleConverter.FromTimeSpan;
						break;
					}
				}
			}
			return fromBoolean as DoubleConverter.Conversion<TInput>;
		}

		public static DoubleConverter.Conversion<TInput> GetEnumConversion<TInput>()
		{
			DoubleConverter.Conversion<TInput> fromSByte;
			switch (Type.GetTypeCode(typeof(TInput)))
			{
				case TypeCode.SByte:
				{
					fromSByte = (TInput val) => DoubleConverter.FromSByte(Convert.ToSByte(val));
					break;
				}
				case TypeCode.Byte:
				{
					fromSByte = (TInput val) => DoubleConverter.FromByte(Convert.ToByte(val));
					break;
				}
				case TypeCode.Int16:
				{
					fromSByte = (TInput val) => DoubleConverter.FromInt16(Convert.ToInt16(val));
					break;
				}
				case TypeCode.UInt16:
				{
					fromSByte = (TInput val) => DoubleConverter.FromUInt16(Convert.ToUInt16(val));
					break;
				}
				case TypeCode.Int32:
				{
					fromSByte = (TInput val) => DoubleConverter.FromInt32(Convert.ToInt32(val));
					break;
				}
				case TypeCode.UInt32:
				{
					fromSByte = (TInput val) => DoubleConverter.FromUInt32(Convert.ToUInt32(val));
					break;
				}
				case TypeCode.Int64:
				{
					fromSByte = (TInput val) => DoubleConverter.FromInt64(Convert.ToInt64(val));
					break;
				}
				case TypeCode.UInt64:
				{
					fromSByte = (TInput val) => DoubleConverter.FromUInt64(Convert.ToUInt64(val));
					break;
				}
				default:
				{
					fromSByte = null;
					break;
				}
			}
			return fromSByte;
		}

		public delegate double Conversion<TInput>(TInput val);
	}
}