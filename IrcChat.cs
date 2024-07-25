using System.Net.Sockets;
using twitchbot;
using Microsoft.Xna.Framework;
using System.Globalization;

namespace TwitchChatIRC
{
	public class IrcChat
	{
		private IList<string> msg => Library.Messages;
		private IList<Color> color => Library.MsgColor;

		private TcpClient tcpClient;
		private StreamReader reader;
		private StreamWriter writer;

		private string Message;
		private string Username;
		private string Raw;
		private Color UserColor;

		public async Task ConnectAsync(string username, string oauthToken, string channel)
		{
			tcpClient = new TcpClient("irc.chat.twitch.tv", 6667);
			reader = new StreamReader(tcpClient.GetStream());
			writer = new StreamWriter(tcpClient.GetStream()) { NewLine = "\r\n", AutoFlush = true };

			// Authenticate
			await writer.WriteLineAsync($"PASS {oauthToken}");
			await writer.WriteLineAsync($"NICK {username}");
			await writer.WriteLineAsync($"JOIN #{channel}");

			await writer.WriteLineAsync("CAP REQ :twitch.tv/membership");
			await writer.WriteLineAsync("CAP REQ :twitch.tv/tags");
			await writer.WriteLineAsync("CAP REQ :twitch.tv/commands");

			// Start reading messages
			await ReadMessagesAsync(msg, color);
		}

		private async Task ReadMessagesAsync(IList<string> msgBuffer, IList<Color> msgColor)
		{
			while (tcpClient.Connected)
			{
				string message = await reader.ReadLineAsync();
				if (message != null)
				{
					if (!message.Contains("USERSTATE") && !message.Contains("ROOMSTATE"))
					{
						Message = UserData.ChatMessage(message);
						Username = "";
						try
						{
							if (message.Contains('!'))
							{
								Username = message.Substring(1, message.IndexOf('!') - 1);
							}
							UserColor = ColorHelper.ParseColor(UserData.UsernameColor(message));
						}
						catch
						{
							UserColor = Color.Black;
						}
						finally
						{
							Library.Instance.AddMessage(Username + ": " + Message, UserColor);
						}
					}
				}
			}
		}

		public void Disconnect()
		{
			writer?.Close();
			reader?.Close();
			tcpClient?.Close();
		}
	}

	public static class ColorHelper
	{
		public static Color ParseColor(string colorString)
		{
			if (string.IsNullOrEmpty(colorString))
				throw new ArgumentException("Color string cannot be null or empty.");

			// Handle named colors
			if (Enum.TryParse(colorString, true, out Color namedColor))
			{
				return namedColor;
			}

			// Handle hexadecimal values
			if (colorString.StartsWith("#"))
			{
				colorString = colorString.TrimStart('#');
				if (colorString.Length == 6)
				{
					return new Color(
						int.Parse(colorString.Substring(0, 2), NumberStyles.HexNumber),
						int.Parse(colorString.Substring(2, 2), NumberStyles.HexNumber),
						int.Parse(colorString.Substring(4, 2), NumberStyles.HexNumber)
					);
				}
				else if (colorString.Length == 8)
				{
					return new Color(
						int.Parse(colorString.Substring(0, 2), NumberStyles.HexNumber),
						int.Parse(colorString.Substring(2, 2), NumberStyles.HexNumber),
						int.Parse(colorString.Substring(4, 2), NumberStyles.HexNumber),
						int.Parse(colorString.Substring(6, 2), NumberStyles.HexNumber)
					);
				}
			}

			// Handle RGB values
			string[] rgb = colorString.Split(',');
			if (rgb.Length == 3)
			{
				return new Color(
					int.Parse(rgb[0]),
					int.Parse(rgb[1]),
					int.Parse(rgb[2])
				);
			}
			else if (rgb.Length == 4)
			{
				return new Color(
					int.Parse(rgb[0]),
					int.Parse(rgb[1]),
					int.Parse(rgb[2]),
					int.Parse(rgb[3])
				);
			}

			throw new ArgumentException("Invalid color string format.");
		}
	}
}