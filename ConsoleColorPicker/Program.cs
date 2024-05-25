
// https://gist.github.com/fnky/458719343aabd01cfb17a3a4f7296797#rgb-colors

// https://colordesigner.io/gradient-generator

// https://learn.microsoft.com/en-us/windows/console/console-virtual-terminal-sequences#extended-colors

// MAIN

Console.CursorVisible = false;

ConsoleColorPicker colorPicker = new ConsoleColorPicker();
colorPicker.Render();

Console.CursorVisible = true;

// CLASSES

public class ConsoleColorPicker
{
	private int R { get; set; }
	private int G { get; set; }
	private int B { get; set; }

	private Slider SliderR { get; }
	private Slider SliderG { get; }
	private Slider SliderB { get; }

	// console location tracking
	private int CursorLeft { get; set; }
	private int CursorTop { get; set; }

	public ConsoleColorPicker(int r, int g, int b)
	{
		R = r;
		G = g;
		B = b;

		SliderR = Slider.CreateRGBSlider(R);
		SliderG = Slider.CreateRGBSlider(G);
		SliderB = Slider.CreateRGBSlider(B);
	}

	public ConsoleColorPicker() : this(80, 200, 120) {}

	public void Render()
	{
		CursorLeft = Console.CursorLeft;
		CursorTop = Console.CursorTop;

		// first line (red slider)
		SetCosnoleRGB();
		Console.Write("████████");
		ResetConsoleRGB();
		Console.Write("  [R] ");
		SliderR.Render();
		Console.WriteLine();

		// second line (green slider)
		SetCosnoleRGB();
		Console.Write("████████");
		ResetConsoleRGB();
		Console.Write("  [G] ");
		SliderG.Render();
		Console.WriteLine();

		// second line (blue slider)
		SetCosnoleRGB();
		Console.Write("████████");
		ResetConsoleRGB();
		Console.Write("  [B] ");
		SliderB.Render();
		Console.WriteLine();
	}

	private void SetCosnoleRGB() => Console.Write($"\x1b[38;2;{R};{G};{B}m");
	private void ResetConsoleRGB() => Console.Write("\x1b[0m");
}

public class Slider
{
	// slider settings
	public int Value { get; private set; }
	private int Minimum { get; init; }
	private int Maximum { get; init; }
	private int Step { get; init; }

	// console location tracking
	private int CursorLeft { get; set; }
	private int CursorTop { get; set; }

	public Slider(int value, int minimum, int maximum, int step)
	{
		Value = value;
		Minimum = minimum;
		Maximum = maximum;
		Step = step;
	}

	public static Slider CreateRGBSlider(int value) => new(value, 0, 255, 8);

	// prints the slider to the console at current location
	public void Render()
	{
		CursorLeft = Console.CursorLeft;
		CursorTop = Console.CursorTop;

		PrintSlider();
	}

	// update slider display with new value
	public void Update(int value)
	{
		Value = value;

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

		// create slider with thumb at value
		for (int i = minLocation; i <= maxLocation; i++)
		{
			if (i == thumbLocation)
				Console.Write(thumb);
			else
				Console.Write(line);
		}

		// add current value at end
		Console.Write($" [ {Value.ToString().PadRight(2, ' '),3} ]");
	}
}
