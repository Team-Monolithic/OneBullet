using System;
using System.Collections;
using System.Collections.Generic;
//인터페이스 명 : ITrackedProperty
//인터페이스 역할 : Condition이 ITrackedProperty를 통해 접근
public interface ITrackedProperty
{
    object GetValue();
    void Subscribe(Action<object> onValueChanged);
    void Unsubscribe(Action<object> onValueChanged);

}