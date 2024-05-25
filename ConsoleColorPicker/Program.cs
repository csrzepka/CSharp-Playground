// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#extended-colors
// https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#rgb-colors

// MAIN

Console.CursorVisible = false;
Console.TreatControlCAsInput = true;

ConsoleColorPicker colorPicker = new();
colorPicker.Render();

Console.WriteLine("""
	──────────────────────────────────────────────────────
	Up/Down: change slider, Left/Right: change value
	CTRL-C: quit
	""");

while (true)
{
	ConsoleKeyInfo pressedKey = Console.ReadKey(false);

	switch (pressedKey.Key)
	{
		case ConsoleKey.UpArrow:
			colorPicker.SelectUp();
			break;
		case ConsoleKey.DownArrow:
			colorPicker.SelectDown();
			break;
		case ConsoleKey.LeftArrow:
			colorPicker.SelectLeft();
			break;
		case ConsoleKey.RightArrow:
			colorPicker.SelectRight();
			break;
		default:
			break;
	}

	// Console.WriteLine($"Key Press: {pressedKey.Key}");

	// Handle CTRL+C as input to safely quit program
	if ((pressedKey.Modifiers & ConsoleModifiers.Control) != 0 && pressedKey.Key == ConsoleKey.C)
		break;
}

Console.CursorVisible = true;
Console.TreatControlCAsInput = false;

// CLASSES

public class ConsoleColorPicker
{
	private int R { get; set; }
	private int G { get; set; }
	private int B { get; set; }

	public Slider[] Sliders { get; private set; }

	// track selected slider for key controls
	private Slider _selected;
	private Slider Selected
	{
		get
		{
			return _selected;
		}
		set
		{
			_selected.IsSelected = false;
			_selected = value;
			_selected.IsSelected = true;
		}
	}

	// console location tracking
	private int CursorLeft { get; set; }
	private int CursorTop { get; set; }
	private bool HasRendered { get; set; } = false;

	public ConsoleColorPicker(int r, int g, int b)
	{
		R = r;
		G = g;
		B = b;

		Sliders = new Slider[3];
		Sliders[0] = Slider.CreateRGBSlider(R, "R");
		Sliders[1] = Slider.CreateRGBSlider(G, "G");
		Sliders[2] = Slider.CreateRGBSlider(B, "B");

		// initalize and set current selected slider
		_selected = Sliders[0];
		_selected.IsSelected = true;
	}

	public ConsoleColorPicker() : this(80, 200, 120) { }

	public void Render()
	{
		if (!HasRendered) HasRendered = true;

		CursorLeft = Console.CursorLeft;
		CursorTop = Console.CursorTop;

		foreach (Slider slider in Sliders)
		{
			SetCosnoleRGB();
			Console.Write("████████  ");
			ResetConsoleRGB();
			slider.Render();
			Console.WriteLine();
		}
	}

	public void Update(int r, int g, int b)
	{
		// restrict values to max/min values
		if (r > 255) r = 255; if (r < 0) r = 0;
		if (g > 255) g = 255; if (g < 0) g = 0;
		if (b > 255) b = 255; if (b < 0) b = 0;

		if (R == r && G == g && B == b) return; // nothing to update

		int[] values = [r, g, b];
		R = r;
		G = g;
		B = b;

		if (!HasRendered) return; // not displayed yet

		// save current console cursor location
		int CurrentCursorLeft = Console.CursorLeft;
		int CurrentCursorTop = Console.CursorTop;

		// move cursor to color picker location
		Console.CursorLeft = CursorLeft;
		Console.CursorTop = CursorTop;

		// print new color picker over old color picker
		for (int i = 0; i < 3; i++)
		{
			SetCosnoleRGB();
			Console.Write("████████  ");
			ResetConsoleRGB();
			Sliders[i].Update(values[i]);
			Console.WriteLine();
		}

		// restore console cursor location
		Console.CursorLeft = CurrentCursorLeft;
		Console.CursorTop = CurrentCursorTop;
	}

	private void SetCosnoleRGB() => Console.Write($"\x1b[38;2;{R};{G};{B}m");
	private void ResetConsoleRGB() => Console.Write("\x1b[0m");

	// handlers for key press state updates
	public void SelectUp()
	{
		if      (Selected == Sliders[0]) return;
		else if (Selected == Sliders[1]) Selected = Sliders[0];
		else if (Selected == Sliders[2]) Selected = Sliders[1];
	}

	public void SelectDown()
	{
		if      (Selected == Sliders[0]) Selected = Sliders[1];
		else if (Selected == Sliders[1]) Selected = Sliders[2];
		else if (Selected == Sliders[2]) return;
	}

	public void SelectLeft()
	{
		if      (Selected == Sliders[0]) Update(R - 1, G, B);
		else if (Selected == Sliders[1]) Update(R, G - 1, B);
		else if (Selected == Sliders[2]) Update(R, G, B - 1);
	}

	public void SelectRight()
	{
		if      (Selected == Sliders[0]) Update(R + 1, G, B);
		else if (Selected == Sliders[1]) Update(R, G + 1, B);
		else if (Selected == Sliders[2]) Update(R, G, B + 1);
	}
}

public class Slider
{
	// slider settings
	public int Value { get; private set; }
	private int Minimum { get; init; }
	private int Maximum { get; init; }
	private int Step { get; init; }
	public string Name { get; private set; }

	// console location tracking
	private int CursorLeft { get; set; }
	private int CursorTop { get; set; }

	private bool HasRendered { get; set; } = false;

	private bool _isSelected = false;
	public bool IsSelected
	{
		get
		{
			return _isSelected;
		}
		set
		{
			_isSelected = value;
			Update();
		}
	}

	public Slider(int value, int minimum, int maximum, int step, string name)
	{
		Value = value;
		Minimum = minimum;
		Maximum = maximum;
		Step = step;
		Name = name;
	}

	public static Slider CreateRGBSlider(int value, string name) => new(value, 0, 255, 8, name);

	// prints the slider to the console at current location
	public void Render()
	{
		if (!HasRendered) HasRendered = true;

		CursorLeft = Console.CursorLeft;
		CursorTop = Console.CursorTop;

		PrintSlider();
	}

	// update slider display with new value
	public void Update(int value)
	{
		if (value == Value) return; // nothing to update

		if (value < Minimum) value = Minimum;
		if (value > Maximum) value = Maximum;

		Value = value;

		Update();
	}

	private void Update()
	{
		if (!HasRendered) return; // slider not displayed yet, but allows updating value

		// save current console cursor location
		int CurrentCursorLeft = Console.CursorLeft;
		int CurrentCursorTop = Console.CursorTop;

		// move cursor to slider location
		Console.CursorLeft = CursorLeft;
		Console.CursorTop = CursorTop;

		// print new slider over old slider
		PrintSlider();

		// restore console cursor location
		Console.CursorLeft = CurrentCursorLeft;
		Console.CursorTop = CurrentCursorTop;
	}

	private void PrintSlider()
	{
		string line = "─";
		string thumb = "│";

		int thumbLocation = Value / Step; // where thumnb should be placed on slider
		int minLocation = Minimum / Step;
		int maxLocation = Maximum / Step;

		// create title
		Console.Write($"[{Name}] ");

		// create slider with thumb at value
		for (int i = minLocation; i <= maxLocation; i++)
		{
			if (i == thumbLocation)
			{
				if (IsSelected) // highlight thumb
					Console.Write("\x1b[30;47m");

				Console.Write(thumb);

				if (IsSelected) // reset styling
					Console.Write("\x1b[0m");
			}
			else
			{
				Console.Write(line);
			}
		}

		// add current value at end
		// TODO: Fix padding to be dependent on max + min sizing
		Console.Write($" [ {Value.ToString().PadRight(2, ' '),3} ]");
	}
}
