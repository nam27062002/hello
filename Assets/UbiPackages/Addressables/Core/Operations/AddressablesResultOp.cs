public class AddressablesOpResult : AddressablesOp
{
    public override bool isDone { get { return true; } }
    protected override float ExtendedProgress { get { return 1f; } }        
}
