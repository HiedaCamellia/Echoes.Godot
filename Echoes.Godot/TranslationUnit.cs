namespace Echoes.Godot;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;

public class TranslationUnit : IDisposable
{
    private readonly BehaviorSubject<string?> _value;
    private readonly ConcurrentDictionary<object, List<IDisposable>> _bindings = new();
    private readonly List<IDisposable> _unkeyedBindings = new();
    private readonly CompositeDisposable _disposables = new();

    public IObservable<string?> Value => _value;
    public string SourceFile { get; }
    public string Key { get; }

    public TranslationUnit(Assembly assembly, string sourceFile, string key)
    {
        SourceFile = sourceFile;
        Key = key;

        _value = new BehaviorSubject<string?>(
            TranslationProvider.ReadTranslation(assembly, sourceFile, key, TranslationProvider.Culture));

        _disposables.Add(_value);

        _disposables.Add(Disposable.Create(() =>
        {
            // 清理所有带key的绑定
            foreach (var binding in _bindings.Values)
            {
                lock (binding)
                {
                    foreach (var disposable in binding)
                    {
                        disposable?.Dispose();
                    }
                }
            }
            _bindings.Clear();

            // 清理所有不带key的绑定
            lock (_unkeyedBindings)
            {
                foreach (var disposable in _unkeyedBindings)
                {
                    disposable?.Dispose();
                }
                _unkeyedBindings.Clear();
            }
        }));

        TranslationProvider.OnCultureChanged += (sender, info) =>
        {
            _value.OnNext(TranslationProvider.ReadTranslation(assembly, sourceFile, key, info));
        };
    }

    public IDisposable Bind(Action<string> action)
    {
        ArgumentNullException.ThrowIfNull(action);

        var disposable = _value.Select(s => s ?? "").Subscribe(action);

        lock (_unkeyedBindings)
        {
            _unkeyedBindings.Add(disposable);
        }

        return Disposable.Create(() =>
        {
            lock (_unkeyedBindings)
            {
                _unkeyedBindings.Remove(disposable);
                disposable?.Dispose();
            }
        });
    }

    public IDisposable Bind(Action<string> action, object key)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(key);

        var disposable = _value.Select(s => s ?? "").Subscribe(action);

        var list = _bindings.GetOrAdd(key, _ => new List<IDisposable>());
        lock (list)
        {
            list.Add(disposable);
        }

        return Disposable.Create(() =>
        {
            lock (list)
            {
                list.Remove(disposable);
                disposable?.Dispose();
            }
        });
    }

    public void Unbind(object key)
    {
        ArgumentNullException.ThrowIfNull(key);

        if (!_bindings.TryRemove(key, out var list))
        {
            return;
        }

        lock (list)
        {
            foreach (var disposable in list)
            {
                disposable?.Dispose();
            }
        }
    }

    public void Dispose()
    {
        _disposables.Dispose();
    }
}
