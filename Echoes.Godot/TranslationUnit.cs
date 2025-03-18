namespace Echoes.Godot;

using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

public class TranslationUnit
{
    private BehaviorSubject<string?> _value;

    public IObservable<string?> Value => _value;

    public string SourceFile { get; }
    public string Key { get; }

    public TranslationUnit(Assembly assembly, string sourceFile, string key)
    {
        SourceFile = sourceFile;
        Key = key;

        _value = new BehaviorSubject<string?>(TranslationProvider.ReadTranslation(assembly, sourceFile, key, TranslationProvider.Culture));

        TranslationProvider.OnCultureChanged += (sender, info) =>
        {
            _value.OnNext(TranslationProvider.ReadTranslation(assembly, sourceFile, key, info));
        };
    }

    public void Bind(Action<string> action) => _value.Select(s => s??"").Subscribe(action);
}
