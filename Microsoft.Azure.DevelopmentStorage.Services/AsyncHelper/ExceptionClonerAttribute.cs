using System;

namespace AsyncHelper
{
	[AttributeUsage(AttributeTargets.Class)]
	internal class ExceptionClonerAttribute : Attribute
	{
		private readonly Type exceptionTypeCanClone;

		public Type ExceptionTypeCanClone
		{
			get
			{
				return this.exceptionTypeCanClone;
			}
		}

		public ExceptionClonerAttribute(Type exceptionTypeCanClone)
		{
			this.exceptionTypeCanClone = exceptionTypeCanClone;
		}
	}
}