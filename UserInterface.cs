using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using SharpDX.Direct2D1.Effects;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Timers;
using SharpDX.Direct3D9;

namespace TronBonne.UI
{
	public class ListBox
	{
		public ListBox(Rectangle bounds, Scroll scroll, string[] content, Texture2D[] icon = null, Color[] textColor = null)
		{
			hitbox = bounds;
			this.content = content;
			this.scroll = scroll;
			this.icon = icon;
			this.textColor = textColor;
			selectedPage = this;
		}
		public ListBox(Rectangle bounds, Scroll scroll, Button[] item)
		{
			hitbox = bounds;
			this.scroll = scroll;
			this.item = item;
		}
		public Button[] AddButton(string buttonText, Color color)
		{
			var _item = item.ToList();
			_item.Add(new Button("", default, color) { text2 = buttonText, innactiveDrawText = true });
			return _item.ToArray();
		}
		public bool active = true;
		public Rectangle hitbox;
		public Scroll scroll;
		public Color bgColor = Color.White;
		public string[] content;
		public Texture2D[] icon;
		public Color[] textColor;
		public Button[] item;
		public Button[] tab;
		public ListBox[] page;
		private ListBox selectedPage;
		public int offX, offY;
		public Point MousePosition => Mouse.GetState().Position;
		public void Update(bool canDrag = true)
		{
			if (!active)
				return;
			scroll.parent = hitbox;
			if (canDrag)
			{
				if (hitbox.Contains(MousePosition) && !scroll.clicked)
				{
					hitbox = Element.Drag(hitbox);
				}
			}
			Scroll.KbInteract(scroll, MousePosition.ToVector2());
			Scroll.MouseInteract(scroll, MousePosition.ToVector2(), Mouse.GetState().LeftButton == ButtonState.Pressed);
			Scroll.ScrollInteract(scroll, MousePosition.ToVector2());
		}
		public void Draw(SpriteBatch sb, SpriteFont font, Texture2D backgroundTex, Color color, int xOffset = 0, int yOffset = 0, int height = 42)
		{
			if (!active)
				return;
			if (backgroundTex != default)
				sb.Draw(backgroundTex, hitbox, bgColor);
			for (int n = 0; n < content.Length; n++)
			{
				if (string.IsNullOrWhiteSpace(content[n]))
					return;
				float y = hitbox.Y + n * height - content.Length * height * scroll.value;
				if (y >= hitbox.Top && y <= hitbox.Bottom - height)
				{
					Rectangle box = new Rectangle(hitbox.X, (int)y, 32, 32);
					if (icon != null && icon.Length == content.Length)
					{
						sb.Draw(icon[n], new Vector2(hitbox.X, box.Y + yOffset), Color.White);
					}
					if (textColor == null || textColor.Length != content.Length)
					{
						sb.DrawString(font, content[n], new Vector2(hitbox.X + xOffset, box.Y + yOffset), color);
					}
					else 
					{ 
						sb.DrawString(font, content[n], new Vector2(hitbox.X + xOffset, box.Y + yOffset), Color.White);
					}
				}
			}
		}
		public void Draw(SpriteBatch sb, SpriteFont font, Texture2D backgroundTex, Color color, Color borderColor, int xOffset = 0, int yOffset = 0, int height = 42)
		{
			if (!active)
				return;
			if (backgroundTex != default)
				sb.Draw(backgroundTex, hitbox, bgColor);
			for (int n = 0; n < content.Length; n++)
			{
				if (string.IsNullOrWhiteSpace(content[n]))
					return;
				float y = hitbox.Y + n * height - content.Length * height * scroll.value;
				if (y >= hitbox.Top && y <= hitbox.Bottom - height)
				{
					Rectangle box = new Rectangle(hitbox.X, (int)y, 32, 32);
					if (icon != null && icon.Length == content.Length)
					{
						sb.Draw(icon[n], new Vector2(hitbox.X, box.Y + yOffset), Color.White);
					}
					if (textColor == null || textColor.Length != content.Length)
					{
						sb.DrawString(font, content[n], new Vector2(hitbox.X + xOffset, box.Y + yOffset), color);
					}
					else 
					{ 
						for (int i = -1; i < 2; i++)
						{ 
							for (int j = -1; j < 2; j++)
							{ 
								sb.DrawString(font, content[n], new Vector2(hitbox.X + xOffset + i, box.Y + yOffset + j), borderColor);
							}
						}
						sb.DrawString(font, content[n], new Vector2(hitbox.X + xOffset, box.Y + yOffset), color);
					}
				}
			}
		}
		public void Draw(SpriteBatch sb, SpriteFont font, Texture2D backgroundTex, Color color, Color borderColor, bool drawItems, int xOffset = 0, int yOffset = 0, int height = 42)
		{
			if (!active || item == null)
				return;
			if (backgroundTex != default)
				sb.Draw(backgroundTex, hitbox, bgColor);
			for (int n = 0; n < item.Length; n++)
			{
				float y = hitbox.Y + n * item[n].box.Height - item.Length * item[n].box.Height * scroll.value;
				if (y >= hitbox.Top && y <= hitbox.Bottom - height)
				{
					Rectangle box = new Rectangle(hitbox.X, (int)y, item[n].box.Width, item[n].box.Height);
					item[n].box = box;
					item[n].Draw(item[n].HoverOver());
				}
			}
		}
	}
	public class Scroll
	{
		public Scroll(Rectangle parent)
		{
			this.parent = parent;
		}
		public float value;
		private float x => parent.Right - Width;
		private float y => parent.Top + parent.Height * value;
		public int X => (int)x;
		public int Y => (int)y;
		public Rectangle parent;
		public Rectangle hitbox => new Rectangle(X, Y, Width, Height);
		public const int Width = 12;
		public const int Height = 32;
		public bool clicked;
		private bool flag;
		static int oldValue = 0;
		public static void DirectMouseInteract(Scroll bar, Vector2 mouseScreen, bool mouseLeft)
		{
			if (mouseLeft && bar.hitbox.Contains((int)mouseScreen.X, (int)mouseScreen.Y))
				bar.clicked = true;
			bar.flag = mouseLeft;
			if (!mouseLeft)
				bar.clicked = false;
			if (bar.clicked && bar.flag)
			{
				Vector2 mouse = new Vector2(mouseScreen.X, mouseScreen.Y - bar.parent.Top - Height / 2);
				bar.value = Math.Max(0f, Math.Min(mouse.Y / bar.parent.Height, 1f));
			}
		}
		public static void KbInteract(Scroll bar, Vector2 mouseScreen)
		{
			if (bar.parent.Contains((int)mouseScreen.X, (int)mouseScreen.Y))
			{
				if (Keyboard.GetState().IsKeyDown(Keys.Down))
				{
					if (bar.value * (bar.parent.Height - Height) < bar.parent.Height - Height)
					{
						bar.value += 0.04f;
					}
				}
				if (Keyboard.GetState().IsKeyDown(Keys.Up))
				{
					if (bar.value > 0f)
					{
						bar.value -= 0.04f;
					}
					else bar.value = 0f;
				}
			}
		}
		public static void MouseInteract(Scroll bar, Vector2 mouseScreen, bool mouseLeft)
		{
			if (mouseLeft && bar.hitbox.Contains((int)mouseScreen.X, (int)mouseScreen.Y))
				bar.clicked = true;
			bar.flag = mouseLeft;
			if (!mouseLeft)
				bar.clicked = false;
			if (bar.clicked && bar.flag)
			{
				Vector2 mouse = new Vector2(mouseScreen.X, mouseScreen.Y - bar.parent.Top - Height / 2);
				bar.value = Math.Max(0f, Math.Min(mouse.Y / bar.parent.Height, 1f));
			}
		}
		public static void ScrollInteract(Scroll bar, Vector2 mouseScreen)
		{
			if (bar.parent.Contains((int)mouseScreen.X, (int)mouseScreen.Y))
			{
				if (Mouse.GetState().ScrollWheelValue < oldValue)
				{
					bar.value = Math.Min(1f, bar.value + 0.1f);
				}
				else if (Mouse.GetState().ScrollWheelValue > oldValue)
				{
					bar.value = Math.Max(0f, bar.value - 0.1f);
				}
				oldValue = Mouse.GetState().ScrollWheelValue;
			}
		}
		public void Draw(SpriteBatch sb, Color color)
		{
			sb.Draw(Game1.MagicPixel, hitbox, color);
		}
		public void ScrollToCaret(int totalLines, int visibleLines)
		{
			if (totalLines > visibleLines)
			{
				value = 1f - (float)visibleLines / totalLines;
			}
			else
			{
				value = 0f;
			}
		}
	}
	public class TextBox
	{
		public bool active;
		public string text = "";
		public Color color => active ? color2 * 0.67f : color2 * 0.33f;
		private Color color2 = Color.DodgerBlue;
		public Rectangle box;
		private KeyboardState oldState;
		private KeyboardState keyState => Keyboard.GetState();
		private SpriteBatch sb => Game1.spriteBatch;
		public static Texture2D magicPixel;
		public Point MousePosition => Mouse.GetState().Position;
		public bool MouseLeft => Mouse.GetState().LeftButton == ButtonState.Pressed;
		public static void Initialize(Texture2D magicPixel)
		{
			TextBox.magicPixel = magicPixel;
		}
		public TextBox(Rectangle box, Color color)
		{
			this.box = box;
			this.color2 = color;
		}
		public bool LeftClick()
		{
			return box.Contains(MousePosition) && MouseLeft;
		}
		public bool HoverOver()
		{
			return box.Contains(MousePosition);
		}
		public void UpdateInput()
		{
			if (active)
			{
				foreach (Keys key in keyState.GetPressedKeys())
				{
					if (oldState.IsKeyUp(key))
					{
						if (key == Keys.F3)
							return;
						if (key == Keys.Back)
						{
							if (text.Length > 0)
								text = text.Remove(text.Length - 1);
							oldState = keyState;
							return;
						}
						else if (key == Keys.Space)
							text += " ";
						else if (key == Keys.OemPeriod)
							text += ".";
						else if (text.Length < 24 && key != Keys.OemPeriod)
						{
							string n = key.ToString().ToLower();
							if (n.StartsWith("d") && n.Length == 2)
								n = n.Substring(1);
							text += n;
						}
					}
				}
				oldState = keyState;
			}
		}
		public void DrawText(SpriteFont font, bool drawMagicPixel = false)
		{
			if (!active)
				return;
			if (font != null)
			{
				sb.Draw(magicPixel, box, color);
				sb.DrawString(font, text, new Vector2(box.X + 2, box.Y + 1), Color.White);
			}
			else
			{
				if (drawMagicPixel)
				{
					// Draw background
					//sb.Draw(TextureAssets.MagicPixel.Value, box, color);
				}
				//  Draw text
				//Utils.DrawBorderString(sb, text, new Vector2(box.X + 2, box.Y + 1), Color.White);
			}
		}
	}
	public class Container
	{
		public bool active;
		public bool reserved = false;
		private bool flag;
		public string text = "";
		public Color color => active ? color2 * 0.67f : color2 * 0.33f;
		private Color color2 = Color.DodgerBlue;
		public Rectangle box;
		public Rectangle boxBugFix => new Rectangle(box.X - box.Width, box.Y - box.Height, box.Width, box.Height);
		private KeyboardState oldState;
		private KeyboardState keyState => Keyboard.GetState();
		private SpriteBatch sb => Game1.spriteBatch;
		public static Texture2D magicPixel;
		public Point MousePosition => Mouse.GetState().Position;
		public bool MouseLeft => Mouse.GetState().LeftButton == ButtonState.Pressed;

		public static void Initialize(Texture2D magicPixel)
		{
			Container.magicPixel = magicPixel;
		}

		public Container(Rectangle box, Color color)
		{
			this.box = box;
			this.color2 = color;
		}
		public bool LeftClick()
		{
			return box.Contains(MousePosition) && MouseLeft;
		}
		public bool RightClick()
		{
			return box.Contains(MousePosition) && MouseLeft;
		}
		public bool HoverOver()
		{
			return box.Contains(MousePosition);
		}
		public bool HoverOverBugFix()
		{
			return boxBugFix.Contains(MousePosition);
		}
	}
	public class Button
	{
		public bool active = true;
		public bool innactiveDrawText = false;
		public bool drawMagicPixel = false;
		public string text = "";
		public string text2 = "";
		public int reserved;
		private int tick = 0;
		private Color color2 = Color.DodgerBlue;
		public int offX = 0;
		public int offY = 0;
		public static Texture2D magicPixel;
		public Point MousePosition => Mouse.GetState().Position;
		public bool MouseLeft => Mouse.GetState().LeftButton == ButtonState.Pressed;

		public static void Initialize(Texture2D magicPixel)
		{
			Button.magicPixel = magicPixel;
		}
		private Rectangle boundCorrect => new Rectangle(box.X - box.Width + offX, box.Y - box.Height * 2 + offY, box.Width, box.Height);
		public Color color(bool select = true)
		{
			if (select)
				return boundCorrect.Contains(MousePosition) ? color2 * 0.67f : color2 * 0.33f;
			else
			{
				return Color.White * 0.67f;
			}
		}
		public Rectangle box;            //TODO
		private SpriteBatch sb => Game1.spriteBatch;
		public Texture2D texture;
		public bool LeftClick()
		{
			return active && box.Contains(MousePosition) && MouseLeft;
		}
		public bool LeftClick(Rectangle hitbox)
		{
			return active && hitbox.Contains(MousePosition) && MouseLeft;
		}
		public bool HoverOver()
		{
			return boundCorrect.Contains(MousePosition);
		}
		public bool HoverOver(Rectangle bound)
		{
			return bound.Contains(MousePosition);
		}
		public Button(string text, Rectangle box, Color color)
		{
			this.color2 = color;
			if (texture == null)
				this.texture = magicPixel;
			this.text = text;
			this.box = box;
		}
		public Button(string text, Rectangle box, Texture2D texture = null)
		{
			this.texture = texture;
			if (texture == null)
				this.texture = magicPixel;
			this.text = text;
			this.box = box;
		}
		public void HoverPlaySound(Vector2? position = null)
		{
			if (active && HoverOver(box))
			{
				if (tick == 0)
				{
					//Terraria.Audio.SoundEngine.PlaySound(sound, position);
					tick = 1;
				}
			}
			else tick = 0;
		}
		public void Draw(bool select = true)
		{
			if (!active)
				return;
			if (drawMagicPixel)
			{
				sb.Draw(Game1.MagicPixel, box, color(select));
			}
			sb.DrawString(Game1.Font, text, new Vector2(box.X + 2 + offX, box.Y + 2 + offY), Color.White * 0.90f);
		}
		public void Draw(SpriteFont font, bool select = true)
		{
			if (!active)
				return;
			if (font != null)
			{
				sb.Draw(texture, box, color(select));
				sb.DrawString(font, text, new Vector2(box.X + 2 + offX, box.Y + 2 + offY), Color.White * 0.90f);
			}
			else
			{
				if (drawMagicPixel)
				{
					sb.Draw(Game1.MagicPixel, box, color(select));
				}
				sb.DrawString(Game1.Font, text, new Vector2(box.X + 2 + offX, box.Y + 2 + offY), Color.White * 0.90f);
			}
		}
	}
	public class InputBox
	{
		public bool active;
		public string text = "";
		public Color color
		{
			get { return active ? Color.DodgerBlue * 0.67f : Color.DodgerBlue * 0.33f; }
		}
		public Rectangle box;
		private KeyboardState oldState;
		private KeyboardState keyState
		{
			get { return Keyboard.GetState(); }
		}
		private SpriteBatch sb
		{
			get { return Game1.spriteBatch; }
		}
		public InputBox(Rectangle box)
		{
			this.box = box;
		}
		public Point MousePosition => Mouse.GetState().Position;
		public bool MouseLeft => Mouse.GetState().LeftButton == ButtonState.Pressed;

		public bool LeftClick()
		{
			return box.Contains(MousePosition) && MouseLeft;
		}
		public bool HoverOver()
		{
			return box.Contains(MousePosition);
		}
		public void UpdateInput()
		{
			if (active)
			{
				foreach (Keys key in keyState.GetPressedKeys())
				{
					if (oldState.IsKeyUp(key))
					{
						if (key == Keys.F3)
							return;
						if (key == Keys.Back)
						{
							if (text.Length > 0)
								text = text.Remove(text.Length - 1);
							oldState = keyState;
							return;
						}
						else if (key == Keys.Space)
							text += " ";
						else if (key == Keys.OemPeriod)
							text += ".";
						else if (text.Length < 24 && (key.ToString().StartsWith('D') || key.ToString().Length == 1))
						{
							string n = key.ToString().ToLower();
							if (n.StartsWith("d") && n.Length == 2)
								n = n.Substring(1);
							text += n;
						}
					}
				}
				oldState = keyState;
			}
		}
		public void DrawText(Texture2D background, SpriteFont font, bool drawMagicPixel = false)
		{
			if (background != null && font != null)
			{
				sb.Draw(background, box, color);
				sb.DrawString(font, text, new Vector2(box.X + 2, box.Y + 1), Color.White);
			}
			else
			{
				if (drawMagicPixel)
				{
					sb.Draw(Game1.MagicPixel, box, color);
				}
				sb.DrawString(Game1.Font, text, new Vector2(box.X + 2, box.Y + 1), Color.White);
			}
		}
	}
	public static class Helper
	{
		const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
		const uint MOUSEEVENTF_LEFTUP = 0x0004;
		const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
		const uint MOUSEEVENTF_RIGHTUP = 0x0010;
		[DllImport("user32.dll")]
		static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, uint dwExtraInfo);
		//https://www.pinvoke.net/default.aspx/user32/GetKeyState.html
		internal enum VirtualKeyStates : int
		{
			VK_LBUTTON = 0x01,
			VK_RBUTTON = 0x02,
			VK_CANCEL = 0x03,
			VK_MBUTTON = 0x04,
		}
		static Stopwatch stopwatch => new Stopwatch();
		public static void LeftMouse()
		{
			mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
			var timer = new System.Timers.Timer(10);
			timer.AutoReset = false;
			timer.Elapsed += (object sender, ElapsedEventArgs e) =>
			{
				mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
				timer.Dispose();
			};
			timer.Start();
		}
		public static void RightMouse()
		{
			mouse_event(MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
			mouse_event(MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
		}

		public static Texture2D MagicPixel()
		{
			return null;
			//  System.Drawing.Commons is unsupported
			//MemoryStream mem = new MemoryStream();
			//var bitmap = new System.Drawing.Bitmap(1, 1);
			//using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
			//{
			//    g.FillRectangle(System.Drawing.Brushes.White, 0, 0, 1, 1);
			//    bitmap.Save(mem, ImageFormat.Bmp);
			//}
			//var tex = Texture2D.FromStream(Main.graphics.GraphicsDevice, mem);
			//return tex;
		}
		public static Texture2D FromFile(this Texture2D texture, string path)
		{
			return null;
			//  System.Drawing.Commons is unsupported
			//MemoryStream mem = new MemoryStream();
			//var bitmap = System.Drawing.Bitmap.FromFile(path);
			//bitmap.Save(mem, ImageFormat.Png);
			//var tex = Texture2D.FromStream(Main.graphics.GraphicsDevice, mem);
			//return tex;
		}
	}
	public static class Element
	{
		public static Point MousePosition => Mouse.GetState().Position;
		public static bool MouseLeft => Mouse.GetState().LeftButton == ButtonState.Pressed;

		public static int Width, Height;
		static Point mousePosition => MousePosition;
		static Point tRelative = new Point();
		static Point relative = new Point();

		static bool holdClick = false;
		static bool
			holdW, holdE, holdN, holdS;
		static bool hold = false;

		#region Rectangle


		[Obsolete("Not implemented.")]
		public static void Snap(this Rectangle element)
		{
		}
		public static Rectangle Drag(Rectangle element)
		{
			Point point = mousePosition;
			if (MouseLeft)
			{
				if (element.Contains(point) || holdClick)
				{
					holdClick = true;
					return new Rectangle(mousePosition.X - relative.X, mousePosition.Y - relative.Y, element.Width, element.Height);
				}
			}
			else
			{
				relative = RelativeMouse(element, mousePosition);
				holdClick = false;
			}
			return element;
		}
		public static bool Resize(ref Rectangle element)
		{
			Point point = RelativeMouse(element, mousePosition);
			Point surfaceMouse = mousePosition;

			//Rectangle Hold = new Rectangle(element.Left - 15, element.Top - 15, element.Width + 30, element.Height + 30);
			//if (MouseLeft)
			//	hold = false;
			//if (Hold.Contains(point))
			//	hold = true;
			//if (hold)
			//{
			if (mousePosition.X <= element.Left + 4 && mousePosition.X >= element.Left - 4 || holdW)
			{
				if (mousePosition.Y >= element.Top && mousePosition.Y <= element.Bottom)
				{
					Mouse.SetCursor(MouseCursor.SizeWE);
					if (MouseLeft)
					{
						int resize = (int)(surfaceMouse.X - element.Left);
						element = new Rectangle(element.Left + resize, element.Top, element.Width - resize, element.Height);
						holdW = true;
					}
					if (!MouseLeft/* || element.Width != element.ActualWidth*/)
					{
						if (!MouseLeft)
							holdW = false;
					}
					return true;
				}
			}
			else if (mousePosition.Y <= element.Top + 4 && mousePosition.Y >= element.Top - 4 || holdN)
			{
				if (mousePosition.X <= element.Right && mousePosition.X >= element.Left)
				{
					Mouse.SetCursor(MouseCursor.SizeNS);
					if (MouseLeft)
					{
						int resize = (int)(surfaceMouse.Y - element.Top);
						element = new Rectangle(element.Left, element.Top + resize, element.Width, element.Height - resize);
						holdN = true;
					}
					if (!MouseLeft/* || element.Width != element.ActualWidth*/)
					{
						if (!MouseLeft)
							holdN = false;
					}
					return true;
				}
			}
			else if (mousePosition.X >= element.Right - 4 && mousePosition.X <= element.Right + 4 || holdE)
			{
				if (mousePosition.Y >= element.Top && mousePosition.Y <= element.Bottom)
				{
					Mouse.SetCursor(MouseCursor.SizeWE);
					if (MouseLeft)
					{
						element.Width = surfaceMouse.X - element.Left;
						holdE = true;
					}
					if (!MouseLeft/* || element.Width != element.ActualWidth*/)
					{
						if (!MouseLeft)
							holdE = false;
						tRelative = RelativeMouse(element, mousePosition);
					}
					return true;
				}
			}
			else if (mousePosition.Y >= element.Bottom - 4 && mousePosition.Y <= element.Bottom + 4 || holdS)
			{
				if (mousePosition.X <= element.Right && mousePosition.X >= element.Left)
				{
					Mouse.SetCursor(MouseCursor.SizeNS);
					if (MouseLeft)
					{
						element.Height = surfaceMouse.Y - element.Top;
						holdS = true;
					}
					if (!MouseLeft/* || element.Height != element.ActualHeight*/)
					{
						if (!MouseLeft)
							holdS = false;
						tRelative = RelativeMouse(element, mousePosition);
					}
					return true;
				}
			}
			//}
			return false;
		}
		[Obsolete("Not implemented.")]
		public static void Blur(this Rectangle element, double radius)
		{
			//BlurEffect effect = new BlurEffect();
			//effect.Radius = radius;
			//element.Effect = effect;
		}
		public static Point RelativeMouse(Rectangle element, Point mouse)
		{
			int x = mouse.X - element.Left;
			int y = mouse.Y - element.Top;
			return new Point(x, y);
		}
		public static Point RelativeMouse(this Rectangle element, Point mouse, int Width, int Height)
		{
			int x = Width - element.Left;
			int y = Height - element.Top;
			mouse.X -= x;
			mouse.Y -= y;
			return new Point(mouse.X, mouse.Y);
		}

		public static Rectangle Bounds(this Rectangle element)
		{
			return new Rectangle(element.Left, element.Top, element.Width, element.Height);
		}

		#endregion
	}
}