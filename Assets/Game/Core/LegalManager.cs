/// <summary>
/// This class is reponsible for managing all legal stuff, such as age/consent requirements (GDPR, COPPA). 
/// The intention is to be moving legal related stuff that is all around the game into this class.
/// </summary>
public class LegalManager : Singleton<LegalManager>
{   
    public enum ETermsPolicy
    {
        Basic,
        GDPR,
        Coppa
    };

    public ETermsPolicy GetTermsPolicy()
    {
        ETermsPolicy returnValue = ETermsPolicy.Basic;
        if (GDPRManager.SharedInstance.IsAgeRestrictionRequired())
        {
            returnValue = GDPRManager.SharedInstance.IsConsentRequired() ? ETermsPolicy.GDPR : ETermsPolicy.Coppa;
        }

        return returnValue;
    }
}
