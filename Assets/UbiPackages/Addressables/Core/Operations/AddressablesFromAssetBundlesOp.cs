public class AddressablesFromAssetBundlesOp : AddressablesAsyncOp
{    
    private AssetBundlesOpRequest OperationAsRequest
    {
        get
        {
            return (Operation == null) ? null : (AssetBundlesOpRequest) Operation;
        }
    }

    private AddressablesError m_error;
    public override AddressablesError Error
    {
        get
        {
            if (m_error == null)
            {                
                AssetBundlesOpRequest request = OperationAsRequest;
                if (request != null && request.Result != AssetBundlesOp.EResult.None && request.Result != AssetBundlesOp.EResult.Success)
                {
                    m_error = new AddressablesError(request.Result);                    
                }
            }

            return m_error;
        }
    }

    public override T GetAsset<T>()
    {
        return (OperationAsRequest == null) ? default(T) : OperationAsRequest.GetData<T>();
    }
}
