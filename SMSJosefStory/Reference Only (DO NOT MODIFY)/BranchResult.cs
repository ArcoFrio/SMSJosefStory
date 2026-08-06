namespace GameCreator.Runtime.VisualScripting;

public class BranchResult
{
    public static readonly BranchResult True = new BranchResult(value: true);

    public static readonly BranchResult False = new BranchResult(value: false);

    public bool Value { get; }

    private BranchResult(bool value)
    {
        Value = value;
    }
}