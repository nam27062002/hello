public class DummyLogger : Logger
{
    public override bool CanLog()
    {
        return false;
    }
}
