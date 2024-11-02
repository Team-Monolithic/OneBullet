using System;

public class ConditionNode
{
    private readonly Condition _condition;
    private bool _isConditionMet;

    public ConditionNode(Condition condition)
    {
        _condition = condition ?? throw new ArgumentNullException(nameof(condition));
        _isConditionMet = false;
    }

    public bool Evaluate()
    {
        _isConditionMet = _condition.Evaluate();
        return _isConditionMet;
    }

    public void Reset()
    {
        _isConditionMet = false;
    }

    // Condition의 구독을 해제하는 메서드
    public void UnsubscribeCondition()
    {
        _condition.Unsubscribe();
    }
}
