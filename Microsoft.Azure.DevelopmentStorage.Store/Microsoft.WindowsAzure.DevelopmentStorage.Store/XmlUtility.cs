using Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest;
using Microsoft.UtilityComputing;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class XmlUtility
	{
		public XmlUtility()
		{
		}

		internal static void AddObjectsToDictionaryFromXml(XElement xmlRoot, UtilityRow entity, bool shouldIncludeNull)
		{
			Dictionary<string, object> strs = new Dictionary<string, object>();
			XmlUtility.AddObjectsToDictionaryFromXml(xmlRoot, entity.ColumnValues, shouldIncludeNull);
		}

		internal static void AddObjectsToDictionaryFromXml(XElement xmlRoot, IDictionary<string, object> dictionary, bool shouldIncludeNull)
		{
			foreach (XElement xElement in xmlRoot.Elements())
			{
				string localName = xElement.Name.LocalName;
				object valueFromXml = null;
				if (xElement.Attribute("IsNull") == null)
				{
					valueFromXml = XmlUtility.GetValueFromXml(xElement.Attribute("SqlType").Value, xElement.Value);
				}
				if (valueFromXml == null && !shouldIncludeNull)
				{
					continue;
				}
				dictionary[localName] = valueFromXml;
			}
		}

		internal static string GetSqlTypeFromObject(object o, out string xmlFormattedObject)
		{
			xmlFormattedObject = null;
			if (o == null)
			{
				o = "";
			}
			string name = o.GetType().Name;
			string str = name;
			if (name != null)
			{
				switch (str)
				{
					case "Int32":
					{
						xmlFormattedObject = XmlConvert.ToString((int)o);
						return "int";
					}
					case "Int64":
					{
						xmlFormattedObject = XmlConvert.ToString((long)o);
						return "bigint";
					}
					case "DateTime":
					{
						DateTime sqlBoundedDateTime = DevelopmentStorageDbDataContext.GetSqlBoundedDateTime((DateTime)o);
						xmlFormattedObject = XmlConvert.ToString(sqlBoundedDateTime, XmlDateTimeSerializationMode.Utc);
						return "datetime";
					}
					case "Guid":
					{
						xmlFormattedObject = XmlConvert.ToString((Guid)o);
						return "uniqueidentifier";
					}
					case "Double":
					{
						xmlFormattedObject = XmlConvert.ToString((double)o);
						return "float(53)";
					}
					case "Boolean":
					{
						xmlFormattedObject = ((bool)o ? "1" : "0");
						return "bit";
					}
					case "Byte[]":
					{
						if ((int)(o as byte[]).Length > 65536)
						{
							throw new TableServiceGeneralException(TableServiceError.PropertyValueTooLarge, null);
						}
						xmlFormattedObject = Convert.ToBase64String((byte[])o);
						return "varbinary(max)";
					}
					case "String":
					{
						if (2 * o.ToString().Length > 65536)
						{
							throw new TableServiceGeneralException(TableServiceError.PropertyValueTooLarge, null);
						}
						xmlFormattedObject = DevelopmentStorageDbDataContext.EncodeDataString(o.ToString());
						return "nvarchar(max)";
					}
				}
			}
			throw new NotImplementedException();
		}

		internal static object GetValueFromXml(string sqlType, string val)
		{
			string str = sqlType;
			string str1 = str;
			if (str != null)
			{
				switch (str1)
				{
					case "int":
					{
						return XmlConvert.ToInt32(val);
					}
					case "bigint":
					{
						return XmlConvert.ToInt64(val);
					}
					case "datetime":
					{
						return DevelopmentStorageDbDataContext.CheckSqlBoundsAndReturnAptDate(XmlConvert.ToDateTime(val, XmlDateTimeSerializationMode.Utc));
					}
					case "uniqueidentifier":
					{
						return XmlConvert.ToGuid(val);
					}
					case "float(53)":
					{
						return XmlConvert.ToDouble(val);
					}
					case "bit":
					{
						return XmlConvert.ToBoolean(val);
					}
					case "varbinary(max)":
					{
						return Convert.FromBase64String(val);
					}
					case "nvarchar(max)":
					{
						return DevelopmentStorageDbDataContext.DecodeDataString(val);
					}
				}
			}
			throw new ArgumentException();
		}

		internal static XElement GetXmlFromUtilityRow(UtilityRow entity)
		{
			string str;
			XElement xElement = new XElement("Properties");
			if (entity.ColumnValues.Count > 252)
			{
				throw new TableServiceGeneralException(TableServiceError.TooManyProperties, null);
			}
			foreach (KeyValuePair<string, object> columnValue in entity.ColumnValues)
			{
				string key = columnValue.Key;
				string str1 = key;
				if (key != null && (str1 == "PartitionKey" || str1 == "RowKey"))
				{
					continue;
				}
				if (columnValue.Key.Length > 255)
				{
					throw new TableServiceGeneralException(TableServiceError.PropertyNameTooLong, null);
				}
				if (columnValue.Value is DateTime)
				{
					DateTime value = (DateTime)columnValue.Value;
					if (value < DevStoreTableServiceConstants.MIN_DATETIME_VALUE)
					{
						throw new ArgumentOutOfRangeException(string.Format("DateTime Current value is [{0}] but the values should be at least 1/1/1601.", value));
					}
				}
				string sqlTypeFromObject = XmlUtility.GetSqlTypeFromObject(columnValue.Value, out str);
				XElement xElement1 = new XElement(columnValue.Key, str);
				xElement1.Add(new XAttribute("SqlType", sqlTypeFromObject));
				if (string.IsNullOrWhiteSpace(str))
				{
					xElement1.Add(new XAttribute(XNamespace.Xml + "space", "preserve"));
				}
				if (columnValue.Value == null)
				{
					xElement1.Add(new XAttribute("IsNull", "true"));
				}
				xElement.Add(xElement1);
			}
			return xElement;
		}

		internal static XElement MergeXmlProperties(UtilityRow deltaRow, XElement existingRowXml)
		{
			UtilityRow value = deltaRow.Clone();
			UtilityRow utilityRow = new UtilityRow();
			XmlUtility.AddObjectsToDictionaryFromXml(existingRowXml, utilityRow, true);
			foreach (KeyValuePair<string, object> columnValue in utilityRow.ColumnValues)
			{
				if (value.ColumnValues.ContainsKey(columnValue.Key) && value[columnValue.Key] != null)
				{
					continue;
				}
				value[columnValue.Key] = columnValue.Value;
			}
			return XmlUtility.GetXmlFromUtilityRow(value);
		}
	}
}