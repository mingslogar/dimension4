using System;
using System.Windows.Media;

namespace Daytimer.Controls.Panes.Notes
{
	class NotesFunctions
	{
		private static string[] _sectionColors = new string[] { 
			"#9AC0E6", "#F3D275", "#8AD2A0", "#F4A6A6", 
			"#D6A6D3", "#99D0DF", "#F1B87F", "#F2A8D1", 
			"#9FB2E1", "#B4AFDF", "#D4B298", "#C1DA82", 
			"#ABD58B", "#88D4C2", "#F1B5B5", "#C3C9CF" };

		private static int _lastUsedSectionColor = -1;

		private static string[] _notebookColors = new string[] {
			"#8AB6E2", "#F1CA5D", "#7ACC93", "#F39B9B",
			"#D399CF", "#87C9D9", "#F1B86F", "#EB9FC9",
			"#96ABDE", "#AAA5DB", "#CDA485", "#B4D367",
			"#9DCD78", "#75CDB8", "#F0AAAA", "#B9C0C7"
		};

		private static int _lastUsedNotebookColor = -1;

		/// <summary>
		/// Generates a new section color.
		/// </summary>
		/// <returns></returns>
		public static Color GenerateSectionColor()
		{
			if (_lastUsedSectionColor == -1)
				_lastUsedSectionColor = new Random().Next();

			return (Color)ColorConverter.ConvertFromString(_sectionColors[_lastUsedSectionColor++ % _sectionColors.Length]);
		}

		/// <summary>
		/// Generates a new notebook color.
		/// </summary>
		/// <returns></returns>
		public static Color GenerateNotebookColor()
		{
			if (_lastUsedNotebookColor == -1)
				_lastUsedNotebookColor = new Random().Next();

			return (Color)ColorConverter.ConvertFromString(_notebookColors[_lastUsedNotebookColor++ % _notebookColors.Length]);
		}
	}
}
