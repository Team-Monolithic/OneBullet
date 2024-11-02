using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//인터페이스 명 : ITrackableObject
//역할 : ConditionFactory에서 타겟이 트래킹 가능 한 객체인지 확인할 때 사용, 조건 검사에 들어갈 수 있는 객체면 다 인터페이스 달 것.
public interface ITrackableObject
{
    Dictionary<string, ITrackedProperty> GetProperties();
}
