﻿using System;
using System.IO;
using System.Text;

namespace DDay.iCal.Serialization.iCalendar
{
	public class PeriodSerializer :
		EncodableDataTypeSerializer
	{
		#region Overrides

		public override Type TargetType
		{
			get { return typeof(Period); }
		}

		public override string SerializeToString(object obj)
		{
			IPeriod p = obj as IPeriod;
			ISerializerFactory factory = GetService<ISerializerFactory>();

			if (p != null && factory != null)
			{
				// Push the period onto the serialization context stack
				SerializationContext.Push(p);

				try
				{
					IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
					IStringSerializer timeSpanSerializer = factory.Build(typeof(TimeSpan), SerializationContext) as IStringSerializer;
					if (dtSerializer != null && timeSpanSerializer != null)
					{
						StringBuilder sb = new StringBuilder();

						// Serialize the start time                    
						sb.Append(dtSerializer.SerializeToString(p.StartTime));

						// Serialize the duration
						if (p.Duration != null)
						{
							sb.Append("/");
							sb.Append(timeSpanSerializer.SerializeToString(p.Duration));
						}

						// Encode the value as necessary
						return Encode(p, sb.ToString());
					}
				}
				finally
				{
					// Pop the period off the serialization context stack
					SerializationContext.Pop();
				}
			}
			return null;
		}

		public override object Deserialize(TextReader tr)
		{
			string value = tr.ReadToEnd();

			IPeriod p = CreateAndAssociate() as IPeriod;
			ISerializerFactory factory = GetService<ISerializerFactory>();
			if (p != null && factory != null)
			{
				IStringSerializer dtSerializer = factory.Build(typeof(IDateTime), SerializationContext) as IStringSerializer;
				IStringSerializer durationSerializer = factory.Build(typeof(TimeSpan), SerializationContext) as IStringSerializer;
				if (dtSerializer != null && durationSerializer != null)
				{
					// Decode the value as necessary
					value = Decode(p, value);

					string[] values = value.Split('/');
					if (values.Length != 2)
						return false;

					StringReader reader = new StringReader(values[0]);
					p.StartTime = dtSerializer.Deserialize(reader) as IDateTime;
					reader.Dispose();
					reader = new StringReader(values[1]);
					p.EndTime = dtSerializer.Deserialize(reader) as IDateTime;
					reader.Dispose();

					if (p.EndTime == null)
					{
						reader = new StringReader(values[1]);
						p.Duration = (TimeSpan)durationSerializer.Deserialize(reader);
						reader.Dispose();
					}

					// Only return an object if it has been deserialized correctly.
					if (p.StartTime != null && p.Duration != null)
						return p;
				}
			}

			return null;
		}

		#endregion
	}
}
