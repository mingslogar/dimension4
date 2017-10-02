namespace Daytimer.Logic.Generic
{
	public enum FreeBusyStatus : byte
	{
		Free,
		WorkingElsewhere,
		Tentative,
		Busy,
		OutOfOffice
	}

	public enum EventStatus : byte
	{
		Tentative,
		Confirmed,
		Cancelled
	}

	public enum TodoStatus : byte
	{
		NeedsAction,
		Completed,
		InProcess,
		Cancelled
	}

	public enum FrequencyPattern : byte
	{
		None,
		Secondly,
		Minutely,
		Hourly,
		Daily,
		Weekly,
		Monthly,
		Yearly
	}

	public enum OccurrencePattern
	{
		None = int.MinValue,
		Last = -1,
		SecondToLast = -2,
		ThirdToLast = -3,
		FourthToLast = -4,
		FifthToLast = -5,
		First = 1,
		Second = 2,
		Third = 3,
		Fourth = 4,
		Fifth = 5
	}

	public enum FrequencyEnd : byte
	{
		None,
		Count,
		Date
	}

	public enum Priority : byte
	{
		Low,
		Normal,
		High
	}
}
