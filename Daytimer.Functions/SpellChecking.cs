using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Daytimer.Functions
{
	public class SpellChecking
	{
		private static BitmapImage _cut;
		private static BitmapImage _copy;
		private static BitmapImage _paste;
		private static BitmapImage _undo;
		private static BitmapImage _redo;

		private static bool _imagesInitialized = false;

		private static void InitializeImages()
		{
			if (_imagesInitialized)
				return;

			_imagesInitialized = true;

			_cut = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/cut.png", UriKind.Absolute));
			_cut.Freeze();

			_copy = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/copy.png", UriKind.Absolute));
			_copy.Freeze();

			_paste = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/paste_sml.png", UriKind.Absolute));
			_paste.Freeze();

			_undo = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/undo.png", UriKind.Absolute));
			_undo.Freeze();

			_redo = new BitmapImage(new Uri("pack://application:,,,/Daytimer.Images;component/Images/redo.png", UriKind.Absolute));
			_redo.Freeze();
		}

		/// <summary>
		/// A standard text-editor context menu.
		/// </summary>
		/// <returns></returns>
		private static ContextMenu GetContextMenu()
		{
			InitializeImages();

			ContextMenu cm = new ContextMenu();

			MenuItem m1 = new MenuItem();
			m1.Header = "Cu_t";
			m1.InputGestureText = "Ctrl+X";
			m1.Command = ApplicationCommands.Cut;
			m1.Icon = new Image() { Source = _cut };

			MenuItem m2 = new MenuItem();
			m2.Header = "_Copy";
			m2.InputGestureText = "Ctrl+C";
			m2.Command = ApplicationCommands.Copy;
			m2.Icon = new Image() { Source = _copy };

			MenuItem m3 = new MenuItem();
			m3.Header = "_Paste";
			m3.InputGestureText = "Ctrl+V";
			m3.Command = ApplicationCommands.Paste;
			m3.Icon = new Image() { Source = _paste };

			MenuItem m4 = new MenuItem();
			m4.Header = "_Undo";
			m4.InputGestureText = "Ctrl+Z";
			m4.Command = ApplicationCommands.Undo;
			m4.Icon = new Image { Source = _undo };

			MenuItem m5 = new MenuItem();
			m5.Header = "_Redo";
			m5.InputGestureText = "Ctrl+Y";
			m5.Command = ApplicationCommands.Redo;
			m5.Icon = new Image { Source = _redo };

			cm.Items.Add(m1);
			cm.Items.Add(m2);
			cm.Items.Add(m3);
			cm.Items.Add(new Separator());
			cm.Items.Add(m4);
			cm.Items.Add(m5);

			return cm;
		}

		private static MenuItem NoSuggestionsItem()
		{
			MenuItem nosuggestions = new MenuItem();
			nosuggestions.IsEnabled = false;
			nosuggestions.FontWeight = FontWeights.Bold;
			nosuggestions.Header = "(No Spelling Suggestions)";

			return nosuggestions;
		}

		private static MenuItem SpellCheckCorrectionItem(string text, IInputElement target)
		{
			MenuItem mi = new MenuItem();
			mi.Header = text;
			mi.FontWeight = FontWeights.Bold;
			mi.Command = EditingCommands.CorrectSpellingError;
			mi.CommandParameter = text;
			mi.CommandTarget = target;

			return mi;
		}

		private static void HandleSpellCheckTextBox(object sender, ContextMenuEventArgs e)
		{
			TextBox textBox = sender as TextBox;

			int caretIndex, cmdIndex;
			SpellingError spellingError;

			textBox.ContextMenu = GetContextMenu();
			caretIndex = textBox.CaretIndex;

			cmdIndex = 0;
			spellingError = textBox.GetSpellingError(caretIndex);

			if (spellingError != null)
			{
				foreach (string str in spellingError.Suggestions)
					textBox.ContextMenu.Items.Insert(cmdIndex++, SpellCheckCorrectionItem(str, textBox));

				if (cmdIndex == 0)
					textBox.ContextMenu.Items.Insert(cmdIndex++, NoSuggestionsItem());

				textBox.ContextMenu.Items.Insert(cmdIndex++, new Separator());

				MenuItem ignoreAllMI = new MenuItem();
				ignoreAllMI.Header = "_Ignore All";
				ignoreAllMI.Command = EditingCommands.IgnoreSpellingError;
				ignoreAllMI.CommandTarget = textBox;
				textBox.ContextMenu.Items.Insert(cmdIndex++, ignoreAllMI);

				MenuItem addToDictMI = new MenuItem();
				addToDictMI.Header = "_Add to Dictionary";
				addToDictMI.Click += addToDictMI_Click;
				addToDictMI.CommandTarget = textBox;
				addToDictMI.Tag = textBox.Text.Substring(textBox.GetSpellingErrorStart(caretIndex), textBox.GetSpellingErrorLength(caretIndex));
				textBox.ContextMenu.Items.Insert(cmdIndex++, addToDictMI);

				textBox.ContextMenu.Items.Insert(cmdIndex, new Separator());
			}
		}

		/// <summary>
		/// User-created custom dictionary.
		/// </summary>
		public static string CustomDictionaryLocation = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
			+ "\\Daytimer\\CustomDictionary.lex";

		/// <summary>
		/// Built-in dictionary.
		/// </summary>
		private const string BuiltinDictionaryLocation = "pack://application:,,,/Daytimer.Functions;component/Dictionary.lex";

		private static void addToDictMI_Click(object sender, RoutedEventArgs e)
		{
			//
			// Word not already in dictionary.
			//
			string word = (sender as MenuItem).Tag.ToString();
			AddWordToDictionary(word);
		}

		public static void AddWordToDictionary(string word)
		{
			//
			// Ensure directory exists.
			//
			string folder = CustomDictionaryLocation.Remove(CustomDictionaryLocation.LastIndexOf('\\'));

			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			//
			// Add dictionary entry to file.
			//
			File.AppendAllText(CustomDictionaryLocation, word + "\r\n");

			//
			// Ignore any instances of this word. We don't want to
			// reload the dictionary for speed purposes.
			//
			//if (sender != null)
			//{
			//	TextBox target = ((sender as MenuItem).CommandTarget as TextBox);
			//	EditingCommands.IgnoreSpellingError.Execute(word, target);

			//	//
			//	// We have to reload dictionaries for any other text
			//	// elements in the application.
			//	//
			//	foreach (TextBox each in ControlledTextBoxes)
			//	{
			//		if (each != target)
			//		{
			//			each.SpellCheck.CustomDictionaries.Clear();
			//			each.SpellCheck.CustomDictionaries.Add(new Uri(DictionaryLocation, UriKind.Absolute));
			//		}
			//	}
			//}
			//else
			//{
			foreach (TextBoxBase each in ControlledTextBoxes)
			{
				if (each.SpellCheck.CustomDictionaries.Count > 1)
					each.SpellCheck.CustomDictionaries[1] = new Uri(CustomDictionaryLocation, UriKind.Absolute);
				else
					each.SpellCheck.CustomDictionaries.Add(new Uri(CustomDictionaryLocation, UriKind.Absolute));
			}
			//}
		}

		public static void DeleteWordFromDictionary(string word)
		{
			//
			// Read old entries
			//
			string[] entries = File.ReadAllLines(CustomDictionaryLocation);
			int length = entries.Length;
			string[] newentries = new string[length - 1];
			int index = Array.IndexOf(entries, word);

			//
			// Copy new entries, excluding deleted word
			//
			Array.Copy(entries, 0, newentries, 0, index);
			Array.Copy(entries, index + 1, newentries, index, length - index - 1);
			entries = null;

			//
			// Write new dictionary
			//
			File.WriteAllLines(CustomDictionaryLocation, newentries);

			//
			// Reload text boxes
			//
			foreach (TextBoxBase each in ControlledTextBoxes)
			{
				if (each.SpellCheck.CustomDictionaries.Count > 1)
					each.SpellCheck.CustomDictionaries[1] = new Uri(CustomDictionaryLocation, UriKind.Absolute);
			}
		}

		/// <summary>
		/// An array of all TextBox controls; allows for commands to
		/// be applied application-wide.
		/// </summary>
		private static List<TextBoxBase> ControlledTextBoxes = new List<TextBoxBase>();

		public static void HandleSpellChecking(TextBox textBox)
		{
			textBox.SpellCheck.IsEnabled = _isSpellCheckingEnabled;

			ControlledTextBoxes.Add(textBox);

			textBox.ContextMenuOpening += HandleSpellCheckTextBox;
			textBox.Unloaded += textBox_Unloaded;

			textBox.SpellCheck.CustomDictionaries.Add(new Uri(BuiltinDictionaryLocation, UriKind.Absolute));

			if (File.Exists(CustomDictionaryLocation))
				textBox.SpellCheck.CustomDictionaries.Add(new Uri(CustomDictionaryLocation, UriKind.Absolute));
		}

		private static void textBox_Unloaded(object sender, RoutedEventArgs e)
		{
			TextBoxBase _sender = sender as TextBoxBase;

			_sender.ContextMenuOpening -= HandleSpellCheckTextBox;
			_sender.Unloaded -= textBox_Unloaded;
			ControlledTextBoxes.Remove(_sender);

			if (_sender == FocusedRTB)
				FocusedRTB = null;
		}

		private static void HandleSpellCheckRichTextBox(object sender, ContextMenuEventArgs e)
		{
			RichTextBox textBox = sender as RichTextBox;

			TextPointer caretIndex;
			int cmdIndex;
			SpellingError spellingError;

			textBox.ContextMenu = GetContextMenu();
			caretIndex = textBox.CaretPosition;

			cmdIndex = 0;
			spellingError = textBox.GetSpellingError(caretIndex);

			if (spellingError != null)
			{
				foreach (string str in spellingError.Suggestions)
					textBox.ContextMenu.Items.Insert(cmdIndex++, SpellCheckCorrectionItem(str, textBox));

				if (cmdIndex == 0)
					textBox.ContextMenu.Items.Insert(cmdIndex++, NoSuggestionsItem());

				textBox.ContextMenu.Items.Insert(cmdIndex++, new Separator());

				MenuItem ignoreAllMI = new MenuItem();
				ignoreAllMI.Header = "_Ignore All";
				ignoreAllMI.Command = EditingCommands.IgnoreSpellingError;
				ignoreAllMI.CommandTarget = textBox;
				textBox.ContextMenu.Items.Insert(cmdIndex++, ignoreAllMI);

				MenuItem addToDictMI = new MenuItem();
				addToDictMI.Header = "_Add to Dictionary";
				addToDictMI.Click += addToDictMI_Click;
				addToDictMI.CommandTarget = textBox;

				TextRange range = textBox.GetSpellingErrorRange(caretIndex);

				addToDictMI.Tag = range.Text;
				textBox.ContextMenu.Items.Insert(cmdIndex++, addToDictMI);

				textBox.ContextMenu.Items.Insert(cmdIndex, new Separator());
			}
		}

		public static RichTextBox FocusedRTB = null;

		/// <summary>
		/// Handle spell-checking of a RichTextBox, optionally setting text editing UI to operate
		/// on the RichTextBox.
		/// </summary>
		/// <param name="textBox"></param>
		/// <param name="setFocusedRtb"></param>
		public static void HandleSpellChecking(RichTextBox textBox, bool setFocusedRtb = true)
		{
			if (setFocusedRtb)
				FocusedRTB = textBox;

			textBox.SpellCheck.IsEnabled = _isSpellCheckingEnabled;

			ControlledTextBoxes.Add(textBox);

			textBox.ContextMenuOpening += HandleSpellCheckRichTextBox;
			textBox.Unloaded += textBox_Unloaded;

			textBox.SpellCheck.CustomDictionaries.Add(new Uri(BuiltinDictionaryLocation, UriKind.Absolute));

			if (File.Exists(CustomDictionaryLocation))
				textBox.SpellCheck.CustomDictionaries.Add(new Uri(CustomDictionaryLocation, UriKind.Absolute));
		}

		private static bool _isSpellCheckingEnabled = Settings.SpellCheckingEnabled;

		/// <summary>
		/// Enable or disable global spell checking.
		/// </summary>
		/// <param name="value">True if spell checking should be enabled; otherwise false.</param>
		public static void EnableSpellChecking(bool value)
		{
			if (_isSpellCheckingEnabled == value)
				return;

			_isSpellCheckingEnabled = value;

			foreach (TextBoxBase each in ControlledTextBoxes)
				each.SpellCheck.IsEnabled = value;
		}
	}
}
