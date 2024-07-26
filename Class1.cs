using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TronBonne;
using TronBonne.UI;
using ColorDialog = System.Windows.Forms.ColorDialog;

namespace TwitchChatIRC
{
	[twitchbot.api.ApiVersion(0, 1)]
	public class Library : ChatInterface
	{
		public static Library Instance;

		public override string Name => "Twitch Chat IRC";
		public override Version Version => new Version(1, 0, 28, 4);

		public string User = "";
		public string OAuth = "";
		public string Channel = "";

		public IrcChat chat;
		Rectangle chatBounds = new Rectangle(0, 0, 200, 300);

		ListBox chatBox;
		Scroll chatScroll;
		Button color;
		Button textColor;
		Button borderColor;
		Button noBgButton;
		Color chatBgColor = Color.Gray;
		Color chatTextColor = Color.White;
		Color chatBorderColor = Color.Black;

		int ticks = 0;
		bool NoChat = false;

		/// <summary>
		/// The pre-built string channel for displaying on the chat log. This is linked with MsgColor List to be used in parallel.
		/// </summary>
		public static IList<string> Messages = new List<string>() { "Beginning of chat:" };
		/// <summary>
		/// The pre-built message Color for displaying chat colors in the chat log. This is to be run in parallel with the Messages List.
		/// </summary>
		public static IList<Color> MsgColor = new List<Color>() { Color.Black };

		public override void Initialize()
		{
			new IrcChat().ConnectAsync(User, OAuth, Channel);
		}

		public override bool LoadContent()
		{
			var v2 = Game1.Consolas.MeasureString("Change background");
			color = new Button("Change background", new Rectangle(0, 0, (int)v2.X, (int)v2.Y), Color.Gray) { active = true, drawMagicPixel = true, innactiveDrawText = true };
			v2 = Game1.Consolas.MeasureString("Text color");
			textColor = new Button("Text color", new Rectangle(0, (int)v2.Y * 1, (int)v2.X, (int)v2.Y), Color.LightGray) { active = true, drawMagicPixel = true, innactiveDrawText = true };
			v2 = Game1.Consolas.MeasureString("Border color");
			borderColor = new Button("Border color", new Rectangle(0, (int)v2.Y * 2, (int)v2.X, (int)v2.Y), Color.LightGray) { active = true, drawMagicPixel = true, innactiveDrawText = true };
			v2 = Game1.Consolas.MeasureString("Draw background");
			noBgButton = new Button("Draw background", new Rectangle(0, (int)v2.Y * 3, (int)v2.X, (int)v2.Y), Color.LightGray) { active = true, drawMagicPixel = true, innactiveDrawText = true };
			Button = new Button[]
			{
				color,
				textColor,
				borderColor,
				noBgButton
			};
			return true;
		}

		public override bool Load()
		{
			Instance = this;
			chatScroll = new Scroll(chatBounds);
			chatBox = new ListBox(chatBounds, chatScroll, Messages.ToArray(), textColor: MsgColor.ToArray());
			return true;
		}

		public override void Update()
		{
			chatBox.content = Messages.ToArray();
			chatBox.textColor = MsgColor.ToArray();
			chatBounds = chatBox.hitbox;
			if (!chatScroll.clicked)
			{
				if (!Element.Resize(ref chatBox.hitbox))
				{
					Mouse.SetCursor(MouseCursor.Arrow);
					chatBox.Update(true);
				}
				else if (chatBounds != chatBox.hitbox)
				{
					ResizeChat();
				}
			}
			chatBox.Update(false);
			if (color.LeftClick())
			{
				var item = new ColorDialog();
				item.ShowDialog();
				var c = item.Color;
				chatBgColor = new Color(c.R, c.G, c.B);
			}
			if (textColor.LeftClick())
			{
				var item = new ColorDialog();
				item.ShowDialog();
				var c = item.Color;
				chatTextColor = new Color(c.R, c.G, c.B);
			}
			if (borderColor.LeftClick())
			{
				var item = new ColorDialog();
				item.ShowDialog();
				var c = item.Color;
				chatBorderColor = new Color(c.R, c.G, c.B);
			}
			if (noBgButton.LeftClick() && ticks == 0)
			{
				NoChat = !NoChat;
				ticks++;
			}
			else if (!noBgButton.LeftClick()) ticks = 0;
		}

		public override void Draw(SpriteBatch sb)
		{
			if (!NoChat)
			{
				sb.Draw(Game1.MagicPixel, chatBox.hitbox, chatBgColor);
			}
			chatBox.Draw(sb, Game1.Consolas, default, chatTextColor, chatBorderColor, 4, 0, 18);
			if (chatBox.hitbox.Contains(Mouse.GetState().Position))
			{
				chatScroll.Draw(sb, Color.Gray);
			}
		}

		public override void Dispose()
		{
		}

		public void AddMessage(string text, Color color)
		{
			if (chatBox == null)
				return;
			var list = TextWrapper.WrapText(Game1.Consolas, text, chatBox.hitbox.Width);
			foreach (var item in list)
			{
				Messages.Add(item);
				MsgColor.Add(color);
			}
			chatScroll.ScrollToCaret(Messages.Count, (int)(chatBox.hitbox.Height / Game1.Consolas.MeasureString("|").Y));
		}

		public void ResizeChat()
		{
		}
	}
}
