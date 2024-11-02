using System;

//객체 명 : TrackedProperty
//객체 역할 : 제네릭으로 일반 멤버 프로퍼티를 래핑하여 값 변경이 일어날 때 구독된 쪽에 알림을 줌.


public class TrackedProperty<T> : ITrackedProperty
{
    private T _value;
    public event Action<T> OnValueChanged;

    public T Value
    {
        get => _value;
        set
        {
            if (!_value.Equals(value))
            {
                _value = value;
                OnValueChanged?.Invoke(_value);
            }
        }
    }

    public TrackedProperty(T initialValue)
    {
        _value = initialValue;
    }

    public void Subscribe(Action<object> onValueChanged)
    {
        OnValueChanged += (newValue) => onValueChanged(newValue);
    }

    public void Unsubscribe(Action<object> onValueChanged)
    {
        OnValueChanged -= (newValue) => onValueChanged(newValue);
    }

    public object GetValue()
    {
        return _value;
    }
}
