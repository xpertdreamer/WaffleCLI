using System.Reflection.PortableExecutable;

namespace WaffleCLI.Core.TUI.Reactive;

public class ReactiveProperty<T> : IObservable<T>
{
    private T _value;
    private readonly List<IObserver<T>> _observers = [];

    public ReactiveProperty(T initialValue)
    {
        _value = initialValue;
    }

    public T Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value)) return;
            _value = value;
            NotifyObservers();
        }
    }

    public IDisposable Subscribe(IObserver<T> observer)
    {
        _observers.Add(observer);
        observer.OnNext(_value);
        return new Unsubscriber(_observers, observer);
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.OnNext(_value);
        }
    }

    private class Unsubscriber : IDisposable
    {
        private readonly List<IObserver<T>> _observers;
        private readonly IObserver<T>? _observer;

        public Unsubscriber(List<IObserver<T>> observers, IObserver<T> observer)
        {
            _observers = observers;
            _observer = observer;
        }

        public void Dispose()
        {
            if(_observer != null && _observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}