using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//객체 명 :ConditionFactory
//객체 역할 : Condition 생성하고, 풀에 등록(리플렉션)

public enum ComparisonOperator
{
    LessThan,
    GreaterThan,
    EqualTo,
    NotEqualTo
}

public enum ConditionType
{
    GameWin,
    GameLose
}


public class ConditionFactory
{
    private readonly Dictionary<string, ITrackableObject> _objectRegistry;
    private readonly Dictionary<string, Dictionary<string, ITrackedProperty>> _propertyCache; // 캐시 추가
    private readonly Action _onGameWinConditionMet;
    private readonly Action _onGameLoseConditionMet;

    //생성자
    public ConditionFactory(Action onGameWinConditionMet, Action onGameLoseConditionMet)
    {
        _objectRegistry = new Dictionary<string, ITrackableObject>();
        _propertyCache = new Dictionary<string, Dictionary<string, ITrackedProperty>>();
        _onGameWinConditionMet = onGameWinConditionMet;
        _onGameLoseConditionMet = onGameLoseConditionMet;
    }


    public void RegisterObject(string objectName, ITrackableObject obj)
    {
        _objectRegistry[objectName] = obj;
        _propertyCache[objectName] = obj.GetProperties();
    }

    // 등록된 객체를 가져오는 메서드
    public T GetRegisteredObject<T>(string objectName) where T : class, ITrackableObject
    {
        if (_objectRegistry.TryGetValue(objectName, out var obj))
        {
            return obj as T;
        }

        Debug.LogError($"Object '{objectName}' not found.");
        return null;
    }

    // 외부(UI)에서 선택한 객체, 속성, 비교 연산자, 값으로 Condition 생성
    public Condition CreateCondition(string objectName, string propertyName, ComparisonOperator comparison, object targetValue, ConditionType conditionType)
    {
        if (!_propertyCache.TryGetValue(objectName, out var properties))
        {
            Debug.LogError($"Object '{objectName}' not found in property cache.");
            return null;
        }

        if (!properties.TryGetValue(propertyName, out var trackedProperty))
        {
            Debug.LogError($"Property '{propertyName}' not found on object '{objectName}'.");
            return null;
        }

        Func<object, bool> conditionCheck = value =>
        {
            switch (comparison)
            {
                case ComparisonOperator.LessThan:
                    return Convert.ToDouble(value) < Convert.ToDouble(targetValue);
                case ComparisonOperator.GreaterThan:
                    return Convert.ToDouble(value) > Convert.ToDouble(targetValue);
                case ComparisonOperator.EqualTo:
                    return Equals(value, targetValue);
                case ComparisonOperator.NotEqualTo:
                    return !Equals(value, targetValue);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        };

        Func<ITrackedProperty> propertyGetter = () => trackedProperty;

        // 조건 타입에 맞는 콜백 지정
        Action conditionMetCallback = conditionType == ConditionType.GameWin ? _onGameWinConditionMet : _onGameLoseConditionMet;

        return new Condition(propertyGetter, conditionCheck, conditionMetCallback);
    }
}
