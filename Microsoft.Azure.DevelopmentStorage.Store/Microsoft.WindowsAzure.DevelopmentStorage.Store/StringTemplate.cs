using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class StringTemplate
	{
		private string m_regionPrefix = "#";

		private readonly string m_originalTemplate;

		private StringBuilder m_currentString = new StringBuilder();

		private Dictionary<string, StringTemplate> m_childTemplates = new Dictionary<string, StringTemplate>();

		private Dictionary<string, int> m_childTemplateStarts = new Dictionary<string, int>();

		private Regex TemplateStartRegex
		{
			get
			{
				return new Regex(string.Concat("( *)", this.m_regionPrefix, "region (\\w+Template) *\r\n"));
			}
		}

		public string Value
		{
			get
			{
				return this.m_currentString.ToString();
			}
		}

		public StringTemplate(string value) : this(value, "#")
		{
		}

		public StringTemplate(string value, string regionPrefix)
		{
			this.m_originalTemplate = value;
			this.m_currentString.Append(value);
			this.m_regionPrefix = regionPrefix;
		}

		public void AddTemplateSubstitution(string templateName, string value)
		{
			int item = this.m_childTemplateStarts[templateName];
			this.m_currentString.Insert(item, value);
			foreach (string key in this.m_childTemplates.Keys)
			{
				if (this.m_childTemplateStarts[key] < item)
				{
					continue;
				}
				Dictionary<string, int> mChildTemplateStarts = this.m_childTemplateStarts;
				Dictionary<string, int> strs = mChildTemplateStarts;
				string str = key;
				string str1 = str;
				mChildTemplateStarts[str] = strs[str1] + value.Length;
			}
		}

		public StringTemplate GetChildTemplate(string name)
		{
			return new StringTemplate(this.m_childTemplates[name].m_originalTemplate, this.m_regionPrefix);
		}

		private Regex GetTemplateEndRegex(string indentation)
		{
			Regex regex = new Regex(string.Concat("\r\n", indentation, this.m_regionPrefix, "endregion.*\r\n"));
			return regex;
		}

		public void MakeChildTemplates()
		{
			int index = 0;
			for (Match i = this.TemplateStartRegex.Match(this.m_currentString.ToString()); i.Success; i = this.TemplateStartRegex.Match(this.m_currentString.ToString(), index))
			{
				index = i.Index;
				int num = i.Index + i.Length;
				string value = i.Groups[1].Value;
				string stringTemplate = i.Groups[2].Value;
				Regex templateEndRegex = this.GetTemplateEndRegex(value);
				i = templateEndRegex.Match(this.m_currentString.ToString(), num);
				if (!i.Success)
				{
					throw new InvalidOperationException(string.Concat("The template ", stringTemplate, " is not closed"));
				}
				int index1 = i.Index + "\r\n".Length;
				int num1 = i.Index + i.Length;
				this.m_childTemplates[stringTemplate] = new StringTemplate(this.m_currentString.ToString().Substring(num, index1 - num), this.m_regionPrefix);
				this.m_childTemplateStarts[stringTemplate] = index;
				this.m_currentString.Remove(index, num1 - index);
			}
		}

		public void ReplaceTemplateParameter(string templateName, string value)
		{
			if (this.m_childTemplates.Count != 0)
			{
				throw new InvalidOperationException("Cannot replace templates after child templates have been extracted.");
			}
			this.m_currentString = this.m_currentString.Replace(templateName, value);
		}
	}
}