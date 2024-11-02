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

    public bool Evaluate()
    {
        switch (_evaluationMode)
        {
            case ConditionEvaluationMode.Sequential:
                return EvaluateSequentially();
            case ConditionEvaluationMode.NonSequential:
                return EvaluateNonSequentially();
            default:
                return false;
        }
    }

    // 순차적 평가
    private bool EvaluateSequentially()
    {
        if (_currentConditionIndex >= _nodes.Count)
            return true;

        var currentNode = _nodes[_currentConditionIndex];
        if (currentNode.Evaluate())
        {
            _currentConditionIndex++;
            if (_currentConditionIndex >= _nodes.Count)
            {
                Debug.Log("Victory condition met in the correct order!");
                return true;
            }
        }
        else
        {
            Debug.Log("Failure: Conditions met in the wrong order.");
            Reset();
        }

        return false;
    }

    // 비순차적 평가
    private bool EvaluateNonSequentially()
    {
        foreach (var node in _nodes)
        {
            if (!node.Evaluate())
            {
                return false;
            }
        }

        Debug.Log("ConditionTree::EvaluateNonSequentially()::Victory condition met!");
        return true;
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
