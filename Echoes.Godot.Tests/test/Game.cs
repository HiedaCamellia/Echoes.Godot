namespace Echoes.Godot.Tests;

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reactive.Disposables;
using global::Godot;
using Translations;

public partial class Game : Control
{
    // UI节点
    [Export] public Button TestButton { get; private set; } = null!;
    [Export] public Label HelloLabel { get; private set; } = null!;
    [Export] public Label LangLabel { get; private set; } = null!;

    // 私有字段
    private readonly string[] _cultures = ["en-US", "de-DE", "zh-CN"];
    private readonly CompositeDisposable _disposables = new();
    private int _buttonPresses;

    public override void _Ready()
    {
        InitializeNodes();
        SetupBindings();
        SetInitialCulture();
    }

    [SuppressMessage("ReSharper", "NullCoalescingConditionIsAlwaysNotNullAccordingToAPIContract")]
    private void InitializeNodes()
    {
        TestButton ??= GetNode<Button>("%TestButton");
        HelloLabel ??= GetNode<Label>("%HelloText");
        LangLabel ??= GetNode<Label>("%LangText");
    }

    private void SetupBindings()
    {
        // 文本绑定
        _disposables.Add(
            Strings.hello_world.Bind(s => HelloLabel.Text = s)
        );

        TranslationProvider.OnCultureChanged += handleCultureChanged;
        _disposables.Add(Disposable.Create(() =>
            TranslationProvider.OnCultureChanged -= handleCultureChanged
        ));
        return;

        // 文化变更处理
        void handleCultureChanged(object? sender, CultureInfo info)
        {
            LangLabel.Text = $"Culture: {info}";
        }
    }

    private static void SetInitialCulture() => TranslationProvider.SetCulture(CultureInfo.CurrentUICulture);

    private void OnTestButtonPressed()
    {
        _buttonPresses++;
        var nextCulture = _cultures[_buttonPresses % _cultures.Length];
        TranslationProvider.SetCulture(CultureInfo.GetCultureInfo(nextCulture));
    }

    public override void _ExitTree()
    {
        base._ExitTree();
        _disposables.Dispose();
    }
}
