using System.Media;

namespace Daytimer.Dialogs
{
	public class DialogHelpers
	{
		public static void PlaySound(MessageType MessageIcon)
		{
			SystemSound sound = GetSound(MessageIcon);

			if (sound != null)
				sound.Play();
		}

		private static SystemSound GetSound(MessageType type)
		{
			switch (type)
			{
				case MessageType.Information:
					return SystemSounds.Asterisk;

				case MessageType.Question:
					return SystemSounds.Question;

				case MessageType.Shield:
					return SystemSounds.Beep;

				case MessageType.Error:
					return SystemSounds.Hand;

				case MessageType.Exclamation:
					return SystemSounds.Exclamation;

				default:
					return null;
			}
		}
	}
}
