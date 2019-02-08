/// <summary>
/// This class is used as a wrapper for an aync operation so the provider that is doing the actual stuff is hidden
/// </summary>
public class AddressablesAsyncOp : AddressablesOp
{
    protected UbiAsyncOperation Operation { get; set; }    
    
    public void Setup(UbiAsyncOperation operation)
    {        
        Operation = operation;
    }   

    public override bool isDone
    {
        get
        {            
            return (Operation == null) ? true : (Operation.isDone);
        }
    }    

    protected override float ExtendedProgress
    {
        get
        {
            if (isDone)
            {
                return 1f;
            }
            else
            {
                return (Operation == null) ? 1f : Operation.progress;
            }            
        }
    }    
}
