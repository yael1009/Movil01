using System.Globalization;
using System.Windows.Input;

namespace Movil01;

public partial class MainPage : ContentPage
{
    private string _display = "0";
    private string _expression = "";
    private bool _resetOnNextDigit;

    public MainPage()
    {
        InitializeComponent();

        DigitCommand = new Command<string>(OnDigit);
        ConvertCommand = new Command<string>(OnConvert);
        ClearCommand = new Command(OnClear);
        DecimalCommand = new Command(OnDecimal);
        ToggleSignCommand = new Command(OnToggleSign);
        BackspaceCommand = new Command(OnBackspace);

        BindingContext = this;
    }

    public string Display
    {
        get => _display;
        private set
        {
            if (_display == value) return;
            _display = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayFontSize));
        }
    }

    public string Expression
    {
        get => _expression;
        private set { if (_expression != value) { _expression = value; OnPropertyChanged(); } }
    }

    public double DisplayFontSize => Display.Length switch
    {
        <= 8 => 56,
        <= 11 => 44,
        <= 14 => 34,
        _ => 28
    };

    public ICommand DigitCommand { get; }
    public ICommand ConvertCommand { get; }
    public ICommand ClearCommand { get; }
    public ICommand DecimalCommand { get; }
    public ICommand ToggleSignCommand { get; }
    public ICommand BackspaceCommand { get; }

    private double Current => double.Parse(Display, CultureInfo.InvariantCulture);

    private static string Format(double v)
    {
        if (double.IsNaN(v) || double.IsInfinity(v)) return "Error";
        v = Math.Round(v, 4);
        if (v == 0) v = 0; 
        return v.ToString("0.####", CultureInfo.InvariantCulture);
    }

    private static string UnitName(char u) => u switch
    {
        'C' => "°C",
        'F' => "°F",
        _ => "K"
    };

    private void OnDigit(string? d)
    {
        if (d is null) return;
        if (Display == "Error") OnClear();
        if (_resetOnNextDigit || Display == "0") { Display = d; _resetOnNextDigit = false; }
        else if (Display.Count(char.IsDigit) < 15) Display += d;
    }

    private void OnDecimal()
    {
        if (Display == "Error") OnClear();
        if (_resetOnNextDigit) { Display = "0."; _resetOnNextDigit = false; return; }
        if (!Display.Contains('.')) Display += ".";
    }

   private void OnConvert(string? kind)
    {
        if (kind is null || kind.Length != 2 || Display == "Error") return;
        char from = kind[0], to = kind[1];
        double value = Current;

        double kelvin = from switch
        {
            'C' => value + 273.15,
            'F' => (value - 32) * 5 / 9 + 273.15,
            _ => value
        };

        if (kelvin < -1e-9)  
        {
            Display = "Error";
            Expression = "Menor al cero absoluto";
            _resetOnNextDigit = true;
            return;
        }
        if (kelvin < 0) kelvin = 0;   
        double result = to switch
        {
            'C' => kelvin - 273.15,
            'F' => (kelvin - 273.15) * 9 / 5 + 32,
            _ => kelvin
        };

        Expression = $"{Format(value)} {UnitName(from)} → {UnitName(to)}";
        Display = Format(result);
        _resetOnNextDigit = true;   
    }

    private void OnClear()
    {
        Display = "0";
        Expression = "";
        _resetOnNextDigit = false;
    }

    private void OnToggleSign()
    {
        if (Display == "Error" || Display == "0") return;
        Display = Display.StartsWith('-') ? Display[1..] : "-" + Display;
    }

    private void OnBackspace()
    {
        if (Display == "Error" || _resetOnNextDigit) return;
        var s = Display[..^1];
        Display = s is "" or "-" ? "0" : s;
    }
}