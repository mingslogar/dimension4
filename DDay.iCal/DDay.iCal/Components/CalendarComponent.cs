using DDay.iCal.Serialization;
using DDay.iCal.Serialization.iCalendar;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace DDay.iCal
{
	/// <summary>
	/// This class is used by the parsing framework for iCalendar components.
	/// Generally, you should not need to use this class directly.
	/// </summary>
#if !SILVERLIGHT
	[Serializable]
#endif
	[DebuggerDisplay("Component: {Name}")]
	public class CalendarComponent :
		CalendarObject,
		ICalendarComponent
	{
		#region Static Public Methods

		#region LoadFromStream(...)

		#region LoadFromStream(Stream s) variants

		/// <summary>
		/// Loads an iCalendar component (Event, Todo, Journal, etc.) from an open stream.
		/// </summary>
		static public ICalendarComponent LoadFromStream(Stream stream)
		{
			return LoadFromStream(stream, Encoding.UTF8);
		}

		#endregion

		#region LoadFromStream(Stream s, Encoding e) variants

		static public ICalendarComponent LoadFromStream(Stream stream, Encoding encoding)
		{
			return LoadFromStream(stream, encoding, new ComponentSerializer());
		}

		static public T LoadFromStream<T>(Stream stream, Encoding encoding)
			where T : ICalendarComponent
		{
			ComponentSerializer serializer = new ComponentSerializer();
			object obj = LoadFromStream(stream, encoding, serializer);
			if (obj is T)
				return (T)obj;
			return default(T);
		}

		static public ICalendarComponent LoadFromStream(Stream stream, Encoding encoding, ISerializer serializer)
		{
			return serializer.Deserialize(stream, encoding) as ICalendarComponent;
		}

		#endregion

		#region LoadFromStream(TextReader tr) variants

		static public ICalendarComponent LoadFromStream(TextReader textReader)
		{
			string text = textReader.ReadToEnd();
			textReader.Close();

			byte[] memoryBlock = Encoding.UTF8.GetBytes(text);
			MemoryStream ms = new MemoryStream(memoryBlock);
			ICalendarComponent iCalComponent = LoadFromStream(ms, Encoding.UTF8);
			ms.Dispose();
			return iCalComponent;
		}

		static public T LoadFromStream<T>(TextReader textReader) where T : ICalendarComponent
		{
			object obj = LoadFromStream(textReader);
			if (obj is T)
				return (T)obj;
			return default(T);
		}

		#endregion

		#endregion

		#endregion

		#region Private Fields

		private ICalendarPropertyList m_Properties;

		#endregion

		#region ICalendarPropertyList Members

		/// <summary>
		/// Returns a list of properties that are associated with the iCalendar object.
		/// </summary>
		virtual public ICalendarPropertyList Properties
		{
			get { return m_Properties; }
			protected set
			{
				this.m_Properties = value;
			}
		}

		#endregion

		#region Constructors

		public CalendarComponent() : base() { Initialize(); }
		public CalendarComponent(string name) : base(name) { Initialize(); }

		private void Initialize()
		{
			m_Properties = new CalendarPropertyList(this, true);
		}

		#endregion

		#region Overrides

		protected override void OnDeserializing(StreamingContext context)
		{
			base.OnDeserializing(context);

			Initialize();
		}

		public override void CopyFrom(ICopyable obj)
		{
			base.CopyFrom(obj);

			ICalendarComponent c = obj as ICalendarComponent;
			if (c != null)
			{
				Properties.Clear();
				foreach (ICalendarProperty p in c.Properties)
					Properties.Add(p.Copy<ICalendarProperty>());
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Adds a property to this component.
		/// </summary>
		virtual public void AddProperty(string name, string value)
		{
			CalendarProperty p = new CalendarProperty(name, value);
			AddProperty(p);
		}

		/// <summary>
		/// Adds a property to this component.
		/// </summary>
		virtual public void AddProperty(ICalendarProperty iCalendarProperty)
		{
			iCalendarProperty.Parent = this;
			Properties.Set(iCalendarProperty.Name, iCalendarProperty);
		}

		#endregion
	}
}
