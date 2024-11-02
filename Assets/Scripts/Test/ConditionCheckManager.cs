using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ConditionCheckManager : MonoBehaviour
{
    private ConditionTree _winConditionTree;
    private ConditionFactory _conditionFactory;
    private List<Condition> _activeConditions = new List<Condition>();

    public void Initialize()
    {
        //관련 오브젝트들 모두 Initialize 된 후에 불려야 함.
        //조건 생성 팩토리와 트리 초기화
        _conditionFactory = new ConditionFactory(OnConditionMet);
        _winConditionTree = new ConditionTree(ConditionEvaluationMode.NonSequential);
        RegisterSceneObjects();
        //테스트용 조건 추가
        __TEST__AddConditions();
        //테스트용 조건 만족 코루틴 호출
        StartCoroutine(nameof(__TEST__SetTimerForTest));
    }

    //테스트용 조건 추가 함수
    private void __TEST__AddConditions()
    {
        var hpCondition = _conditionFactory.CreateCondition("PlayerController", "HP", ComparisonOperator.LessThan, 5, ConditionType.GameWin);
        if (hpCondition != null)
        {
            _activeConditions.Add(hpCondition);
            _winConditionTree.AddNode(new ConditionNode(hpCondition));
        }

        var positionCondition = _conditionFactory.CreateCondition("PlayerController", "Position", ComparisonOperator.EqualTo, new Vector3(0, 0, 0), ConditionType.GameWin);
        if (positionCondition != null)
        {
            _activeConditions.Add(positionCondition);
            _winConditionTree.AddNode(new ConditionNode(positionCondition));
        }
    }

    public void UnsubscribeAllConditions()
    {
        foreach (var condition in _activeConditions)
        {
            condition.Unsubscribe();
        }
        _activeConditions.Clear();
    }


    private void OnDestroy()
    {
        UnsubscribeAllConditions(); 
    }


    //씬 내 오브젝트 중 IconditionTarget 인터페이스 구현 오브젝트 등록
    private void RegisterSceneObjects()
    {
        var conditionTargets = FindObjectsOfType<MonoBehaviour>().OfType<ITrackableObject>();

        foreach (var target in conditionTargets)
        {
            _conditionFactory.RegisterObject(target.GetType().Name, target);
        }
    }
    //개별 조건 만족시 ConditionFactory에서 등록한 콜백이 불려서 트리 검사 들어감
    private void OnConditionMet()
    {
        if (_winConditionTree.Evaluate())
        {
            Debug.Log("ConditionCheckManager::OnConditionMet::Victory condition achieved!");
        }
    }
    //테스트용 타이머 후 조건 만족 함수 호출
    private IEnumerator __TEST__SetTimerForTest()
    {
        yield return new WaitForSeconds(3);
        TestOnlyCondition();
    }
    //테스트용 조건 만족 함수
    private void TestOnlyCondition()
    {
        var playerController = _conditionFactory.GetRegisteredObject<PlayerController>("PlayerController");
        if (playerController != null)
        {
            playerController.HP.Value = 0;
            playerController.Position.Value = Vector3.zero;
        }
    }
}