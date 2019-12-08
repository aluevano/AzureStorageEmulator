using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MeasurementEvents
{
	internal sealed class MeasurementEventMetadata<TEvent>
	where TEvent : MeasurementEvent<TEvent>
	{
		private readonly int eventId;

		private readonly string eventName;

		private readonly string eventProducer;

		private readonly Dictionary<string, MeasurementEvent<TEvent>.StringParameterAccessor> stringParams;

		private readonly Dictionary<string, MeasurementEvent<TEvent>.NumericParameterAccessor> numericParams;

		internal int EventId
		{
			get
			{
				return this.eventId;
			}
		}

		public string EventName
		{
			get
			{
				return this.eventName;
			}
		}

		public string EventProducer
		{
			get
			{
				return this.eventProducer;
			}
		}

		internal MeasurementEventMetadata()
		{
			Type type = typeof(TEvent);
			this.eventProducer = type.Assembly.GetName().Name;
			this.eventName = type.FullName;
			PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
			for (int i = 0; i < (int)properties.Length; i++)
			{
				PropertyInfo propertyInfo = properties[i];
				MeasurementEventParameterAttribute customAttribute = (MeasurementEventParameterAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(MeasurementEventParameterAttribute));
				if (customAttribute != null)
				{
					string parameterNameOverride = customAttribute.ParameterNameOverride;
					if (parameterNameOverride == null)
					{
						parameterNameOverride = propertyInfo.Name;
					}
					else if (parameterNameOverride.Length == 0)
					{
						string[] fullName = new string[] { "Measurement event class ", type.FullName, " declares a property named ", propertyInfo.Name, " which is marked with an empty event property name string." };
						throw new MeasurementEventException(string.Concat(fullName));
					}
					MethodInfo getMethod = propertyInfo.GetGetMethod(true);
					if (null == getMethod)
					{
						string[] strArrays = new string[] { "Class ", type.FullName, " has a property ", propertyInfo.Name, " marked with MeasurementEventParameterAttribute, but the property does not have a 'get' accessor." };
						throw new MeasurementEventException(string.Concat(strArrays));
					}
					MethodInfo method = typeof(MeasurementEventMetadata<TEvent>).GetMethod("BuildParameterAccessors", BindingFlags.Instance | BindingFlags.NonPublic);
					Type propertyType = propertyInfo.PropertyType;
					MethodInfo methodInfo = method.MakeGenericMethod(new Type[] { propertyType });
					object[] objArray = new object[] { parameterNameOverride, getMethod };
					methodInfo.Invoke(this, objArray);
				}
			}
		}

		private void BuildParameterAccessors<TProperty>(string paramName, MethodInfo getter)
		{
			MeasurementEventMetadata<TEvent>.PropertyAccessor<TProperty> propertyAccessor = (MeasurementEventMetadata<TEvent>.PropertyAccessor<TProperty>)Delegate.CreateDelegate(typeof(MeasurementEventMetadata<>.PropertyAccessor<TEvent, TProperty>), getter);
			MeasurementEvent<TEvent>.StringParameterAccessor stringParameterAccessor = (TEvent ev) => {
				string str;
				try
				{
					TProperty tProperty = propertyAccessor(ev);
					str = (tProperty == null ? "" : tProperty.ToString());
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					throw new MeasurementEventException(string.Concat(new string[] { "Exception getting property ", propertyAccessor.Method.Name, " of measurement event class ", typeof(TEvent).FullName, " as a string: ", exception.Message }), exception);
				}
				return str;
			};
			DoubleConverter.Conversion<TProperty> conversion = DoubleConverter.GetConversion<TProperty>();
			MeasurementEvent<TEvent>.NumericParameterAccessor numericParameterAccessor = (TEvent ev) => {
				double num;
				try
				{
					num = conversion(propertyAccessor(ev));
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					throw new MeasurementEventException(string.Concat(new string[] { "Exception getting property ", propertyAccessor.Method.Name, " of measurement event class ", typeof(TEvent).FullName, " as a double: ", exception.Message }), exception);
				}
				return num;
			};
			try
			{
				this.stringParams.Add(paramName, stringParameterAccessor);
				this.numericParams.Add(paramName, numericParameterAccessor);
			}
			catch (ArgumentException argumentException1)
			{
				ArgumentException argumentException = argumentException1;
				throw new MeasurementEventException(string.Concat("Measurement event class ", getter.DeclaringType.FullName, " declares more than one parameter named ", paramName), argumentException);
			}
		}

		internal MeasurementEvent<TEvent>.NumericParameterAccessor GetNumericParameterAccessor(string paramName)
		{
			MeasurementEvent<TEvent>.NumericParameterAccessor numericParameterAccessor;
			if (paramName == null)
			{
				return null;
			}
			this.numericParams.TryGetValue(paramName, out numericParameterAccessor);
			return numericParameterAccessor;
		}

		internal MeasurementEvent<TEvent>.StringParameterAccessor GetStringParameterAccessor(string paramName)
		{
			MeasurementEvent<TEvent>.StringParameterAccessor stringParameterAccessor;
			if (paramName == null)
			{
				return null;
			}
			this.stringParams.TryGetValue(paramName, out stringParameterAccessor);
			return stringParameterAccessor;
		}

		private delegate T PropertyAccessor<T>(TEvent ev);
	}
}