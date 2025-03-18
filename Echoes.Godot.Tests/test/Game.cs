namespace Echoes.Godot.Tests;

using System;
using System.Globalization;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver.Drivers;
using global::Godot;
using Shouldly;
using Translations;

public partial class Game : Control
{
    public Button TestButton { get; private set; } = default!;
    public Label HelloLabel { get; private set; } = default!;
    public Label LangLabel { get; private set; } = default!;
    public int ButtonPresses { get; private set; }

    private string[] _cultures = ["en-US", "de-DE", "zh-CN"];

    public override void _Ready()
    {
        TestButton = GetNode<Button>("%TestButton");
        HelloLabel = GetNode<Label>("%HelloText");
        LangLabel = GetNode<Label>("%LangText");
        Strings.hello_world.Bind(s => HelloLabel.Text = s);
        TranslationProvider.OnCultureChanged += (sender, info) => LangLabel.Text = $"Culture: {info}";
        TranslationProvider.SetCulture(CultureInfo.CurrentUICulture);
    }

    public void OnTestButtonPressed()
    {
        ButtonPresses++;
        var culture = _cultures[ButtonPresses % _cultures.Length];
        TranslationProvider.SetCulture(CultureInfo.GetCultureInfo(culture));
    }
}
