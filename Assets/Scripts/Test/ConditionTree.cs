using System.Collections.Generic;
using UnityEngine;

public enum ConditionEvaluationMode
{
    Sequential,   // 순차적 평가
    NonSequential // 비순차적 평가
}

public class ConditionTree
{
    private readonly List<ConditionNode> _nodes;
    private readonly ConditionEvaluationMode _evaluationMode;
    private int _currentConditionIndex;

    public ConditionTree(ConditionEvaluationMode evaluationMode)
    {
        _nodes = new List<ConditionNode>();
        _evaluationMode = evaluationMode;
        _currentConditionIndex = 0;
    }

    public void AddNode(ConditionNode node)
    {
        _nodes.Add(node);
    }

    public ConditionType? Evaluate()
    {
        switch (_evaluationMode)
        {
            case ConditionEvaluationMode.Sequential:
                return EvaluateSequentially();
            case ConditionEvaluationMode.NonSequential:
                return EvaluateNonSequentially();
            default:
                return null;
        }
    }

    // 순차적 평가
    private ConditionType? EvaluateSequentially()
    {
        if (_currentConditionIndex >= _nodes.Count)
            return null;

        var currentNode = _nodes[_currentConditionIndex];
        if (currentNode.Evaluate())
        {
            _currentConditionIndex++;
            if (_currentConditionIndex >= _nodes.Count)
            {
                return currentNode.GetConditionType();
            }
        }
        else
        {
            Reset(); // 순서가 맞지 않을 경우 초기화
        }

        return null;
    }

    // 비순차적 평가
    private ConditionType? EvaluateNonSequentially()
    {
        bool winConditionMet = false;
        bool loseConditionMet = false;

        foreach (var node in _nodes)
        {
            if (node.Evaluate())
            {
                if (node.GetConditionType() == ConditionType.GameWin)
                {
                    winConditionMet = true;
                }
                else if (node.GetConditionType() == ConditionType.GameLose)
                {
                    loseConditionMet = true;
                }
            }
        }

        if (winConditionMet && !loseConditionMet) return ConditionType.GameWin;
        if (loseConditionMet && !winConditionMet) return ConditionType.GameLose;

        return null;
    }


    public void Reset()
    {
        _currentConditionIndex = 0;
        foreach (var node in _nodes)
        {
            node.Reset();
        }
    }

    // 모든 Condition의 구독을 해제하는 메서드
    public void UnsubscribeAllConditions()
    {
        foreach (var node in _nodes)
        {
            node.UnsubscribeCondition();
        }
    }

}
