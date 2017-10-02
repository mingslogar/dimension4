using System;
using System.Collections.Generic;

namespace Modern.FileBrowser
{
	/// <summary>
	/// Provides a method for creating and navigating a history stack.
	/// </summary>
	class History
	{
		public History()
		{

		}

		/// <summary>
		/// The list of all history entries.
		/// </summary>
		private List<string> _storedLocations = new List<string>();

		/// <summary>
		/// The current location within the history list.
		/// </summary>
		private int _currentLocation = 0;

		/// <summary>
		/// Gets a list of all history entries.
		/// </summary>
		public List<string> StoredLocations
		{
			get { return _storedLocations; }
		}

		/// <summary>
		/// Gets the current location, if one exists, else null.
		/// </summary>
		public string CurrentLocation
		{
			get { return (_storedLocations.Count > 0) ? _storedLocations[_currentLocation - 1] : null; }
		}

		/// <summary>
		/// Gets the previous location, if one exists, else null.
		/// </summary>
		public string Back
		{
			get
			{
				if (CanGoBack)
					return _storedLocations[_currentLocation - 2];

				return null;
			}
		}

		/// <summary>
		/// Gets the next location, if one exists, else null.
		/// </summary>
		public string Forward
		{
			get
			{
				if (CanGoForward)
					return _storedLocations[_currentLocation];

				return null;
			}
		}

		/// <summary>
		/// Add a new location to the history stack.
		/// </summary>
		/// <param name="location"></param>
		public void AddEntry(string location)
		{
			if (string.Equals(location, CurrentLocation, StringComparison.InvariantCultureIgnoreCase))
				return;

			_storedLocations.RemoveRange(_currentLocation, _storedLocations.Count - _currentLocation);
			_storedLocations.Add(location);
			_currentLocation++;
		}

		/// <summary>
		/// Gets if the pointer can be moved back in the stack.
		/// </summary>
		public bool CanGoBack
		{
			get { return _currentLocation > 1; }
		}

		/// <summary>
		/// Gets if the pointer can be moved forward in the stack.
		/// </summary>
		public bool CanGoForward
		{
			get { return _currentLocation < _storedLocations.Count; }
		}

		/// <summary>
		/// Gets the previous history entry, or null if none exists.
		/// </summary>
		/// <returns></returns>
		public string GoBack()
		{
			if (CanGoBack)
				return _storedLocations[--_currentLocation - 1];

			return null;
		}

		/// <summary>
		/// Gets the next history entry, or null if none exists.
		/// </summary>
		/// <returns></returns>
		public string GoForward()
		{
			if (CanGoForward)
				return _storedLocations[_currentLocation++];

			return null;
		}
	}
}
