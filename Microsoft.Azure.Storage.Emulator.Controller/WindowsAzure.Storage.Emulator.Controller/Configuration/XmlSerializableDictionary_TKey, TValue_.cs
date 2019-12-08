using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller.Configuration
{
	[Serializable]
	public class XmlSerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, IXmlSerializable
	{
		public XmlSerializableDictionary()
		{
		}

		public XmlSchema GetSchema()
		{
			return null;
		}

		public void ReadXml(XmlReader reader)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TValue));
			bool isEmptyElement = reader.IsEmptyElement;
			reader.Read();
			if (isEmptyElement)
			{
				return;
			}
			while (reader.NodeType != XmlNodeType.EndElement)
			{
				TValue tValue = (TValue)xmlSerializer.Deserialize(reader);
				PropertyInfo property = tValue.GetType().GetProperty("Name");
				base.Add((TKey)property.GetValue(tValue, null), tValue);
			}
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer)
		{
			XmlSerializer xmlSerializer = new XmlSerializer(typeof(TKey));
			XmlSerializer xmlSerializer1 = new XmlSerializer(typeof(TValue));
			foreach (TKey key in base.Keys)
			{
				xmlSerializer1.Serialize(writer, base[key]);
			}
		}
	}
}