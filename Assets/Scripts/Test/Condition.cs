using System;

public class Condition
{
    private readonly Func<ITrackedProperty> _propertyGetter;
    private readonly Func<object, bool> _conditionCheck;
    private readonly Action _onConditionMet;
    private Action<object> _unsubscribeHandler;  // 구독 해제 핸들러 저장

    public Condition(Func<ITrackedProperty> propertyGetter, Func<object, bool> conditionCheck, Action onConditionMet)
    {
        _propertyGetter = propertyGetter ?? throw new ArgumentNullException(nameof(propertyGetter));
        _conditionCheck = conditionCheck ?? throw new ArgumentNullException(nameof(conditionCheck));
        _onConditionMet = onConditionMet ?? throw new ArgumentNullException(nameof(onConditionMet));

        var property = _propertyGetter();
        if (property == null)
        {
            throw new NullReferenceException("The propertyGetter returned a null value.");
        }

        // 조건 평가를 위한 구독 핸들러 생성 및 저장
        _unsubscribeHandler = newValue =>
        {
            if (_conditionCheck(newValue))
            {
                _onConditionMet(); // 조건 만족 시 콜백 호출
            }
        };

        // 이벤트 구독
        property.Subscribe(_unsubscribeHandler);
    }

    // 구독 해제 메서드
    public void Unsubscribe()
    {
        var property = _propertyGetter();
        if (property != null && _unsubscribeHandler != null)
        {
            property.Unsubscribe(_unsubscribeHandler); // 저장된 핸들러로 구독 해제
            _unsubscribeHandler = null; // 중복 해제 방지
        }
    }

    // 현재 조건 상태를 평가
    public bool Evaluate()
    {
        var property = _propertyGetter();
        return _conditionCheck(property.GetValue());
    }
}
