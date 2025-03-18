namespace Echoes.Godot.Tests;

using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Chickensoft.GoDotTest;
using Chickensoft.GodotTestDriver;
using Chickensoft.GodotTestDriver.Drivers;
using global::Godot;
using Shouldly;
using Translations;

public class PackageTest(Node testScene) : TestClass(testScene)
{
    private Game _game = default!;
    private Fixture _fixture = default!;

    [SetupAll]
    public async Task Setup()
    {
        _fixture = new Fixture(TestScene.GetTree());
        _game = await _fixture.LoadAndAddScene<Game>();
    }

    [CleanupAll]
    public void Cleanup() => _fixture.Cleanup();

    [Test]
    public void TestCultureChange()
    {
        var buttonDriver = new ButtonDriver(() => _game.TestButton);
        for (var i = 0; i < 10; i++)
        {
            buttonDriver.ClickCenter();
            var lastValue = Strings.hello_world.Value.Latest().FirstOrDefault();
            _game.HelloLabel.Text.ShouldBe(lastValue);
        }
    }
}
