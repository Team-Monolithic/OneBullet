using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ConditionCheckManager : MonoBehaviour
{
    private enum ConditionIndex
    {
        Lose = 0, Win = 1
    }
    private List<ConditionTree> _conditionTreeList;
    private ConditionFactory _conditionFactory;
    private List<Condition> _activeConditions = new List<Condition>();

    public void Initialize()
    {
        _conditionFactory = new ConditionFactory(OnGameWinConditionMet, OnGameLoseConditionMet);
        RegisterSceneObjects();
        InitializeConditions();
        StartCoroutine(nameof(__TEST__SetTimerForTest));
    }

    private void AddConditionToTree(ConditionTree tree, string objectName, string propertyName, ComparisonOperator comparison, object targetValue, ConditionType conditionType)
    {
        var condition = _conditionFactory.CreateCondition(objectName, propertyName, comparison, targetValue, conditionType);
        if (condition != null)
        {
            _activeConditions.Add(condition);
            tree.AddNode(new ConditionNode(condition, conditionType));
        }
    }

    private void InitializeConditions()
    {

        _conditionTreeList = new List<ConditionTree>
        {
            new ConditionTree(ConditionEvaluationMode.NonSequential),   // Win Condition Tree
            new ConditionTree(ConditionEvaluationMode.Sequential) // Lose Condition Tree
        };

        CreateConditionsForTest();


    }

    // **테스트용** 조건 추가 함수
    private void CreateConditionsForTest()
    {
        // GameLose 조건 추가
        AddConditionToTree(_conditionTreeList[(int)ConditionIndex.Lose], "PlayerController", "HP", ComparisonOperator.LessThan, 0, ConditionType.GameLose);
        AddConditionToTree(_conditionTreeList[(int)ConditionIndex.Lose], "PlayerController", "Position", ComparisonOperator.EqualTo, new Vector3(0, 0, 0), ConditionType.GameLose);

        // GameWin 조건 추가
        AddConditionToTree(_conditionTreeList[(int)ConditionIndex.Win], "PlayerController", "KillCount", ComparisonOperator.GreaterThan, 10, ConditionType.GameWin);
        AddConditionToTree(_conditionTreeList[(int)ConditionIndex.Win], "PlayerController", "Speed", ComparisonOperator.LessThan, 1, ConditionType.GameWin);
    }

    // 해제 처리
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

    // 씬 내 오브젝트 중 ITrackableObject 인터페이스 구현 오브젝트 등록
    private void RegisterSceneObjects()
    {
        var conditionTargets = FindObjectsOfType<MonoBehaviour>().OfType<ITrackableObject>();

        foreach (var target in conditionTargets)
        {
            _conditionFactory.RegisterObject(target.GetType().Name, target);
        }
    }


    private void OnGameWinConditionMet()
    {
        if (_conditionTreeList[(int)ConditionIndex.Win].Evaluate() == ConditionType.GameWin)
        {
            Debug.Log("Game Win condition met!");
            // Game Win 처리 로직 추가
        }
    }

    private void OnGameLoseConditionMet()
    {
        if (_conditionTreeList[(int)ConditionIndex.Lose].Evaluate() == ConditionType.GameLose)
        {
            Debug.Log("Game Lose condition met!");
            // Game Lose 처리 로직 추가
        }
    }

    // **테스트용** 타이머 후 조건 만족 함수 호출
    private IEnumerator __TEST__SetTimerForTest()
    {
        yield return new WaitForSeconds(3);
        Debug.Log("Sequential Win Condition wrong case Test");
        TestSequentialCondition(false);


        yield return new WaitForSeconds(3);
        Debug.Log("Sequential Win Condition Success Test");
        TestSequentialCondition(true);

        yield return new WaitForSeconds(3);
        Debug.Log("Non Sequential Lose Condition Success Test");
        TestNonSequentialCondition();

    }


    // **테스트용**  순차적 조건 만족 테스트 함수
    private void TestSequentialCondition(bool value)
    {
        var playerController = _conditionFactory.GetRegisteredObject<PlayerController>("PlayerController");
        if (playerController != null )
        {
            if (value == true)
            {
                playerController.KillCount.Value = 0; 
                playerController.Speed.Value = 0f;
                //초기화
                playerController.KillCount.Value = 15; // KillCount Win 조건 만족
                playerController.Speed.Value = 0.5f; // Speed Win 조건 만족
            } else
            {
                playerController.Speed.Value = 0.5f; // Speed Win 조건 만족
                playerController.KillCount.Value = 15; // KillCount Win 조건 만족
            }
        }
    }

    // **테스트용**  비순차적 조건 만족 테스트 함수
    private void TestNonSequentialCondition()
    {
        var playerController = _conditionFactory.GetRegisteredObject<PlayerController>("PlayerController");
        if (playerController != null)
        {
            playerController.HP.Value = -1; // HP FAIL 조건 만족
            playerController.Position.Value = Vector3.zero; // Position FAIL 조건 만족
        }
    }
}
