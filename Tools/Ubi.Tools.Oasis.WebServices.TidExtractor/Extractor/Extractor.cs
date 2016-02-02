using System;
using Ubi.Tools.Oasis.Shared.Services.UserExperience;
using Ubi.Tools.Oasis.WebServices.XmlExtractor.OasisService;

namespace Ubi.Tools.Oasis.WebServices.XmlExtractor.Extractor
{
    abstract class Extractor : IExtractor
    {
        private readonly DataContext _dataContext;

        protected DataContext DataContext { get { return _dataContext; } }

        protected Extractor(OasisServiceClient client)
        {
            _dataContext = new DataContext(client);
        }

        #region IExtractor Members

        public bool Extract()
        {
            try
            {
                using (new ImageDisablerScope())
                {
                    ExtractCore();

                    return true;
                }
            }
            catch (Exception ex)
            {
                Program.DisplayException(ex);

                return false;
            }
        }

        #endregion

        protected virtual void ExtractCore()
        {
        }
    }
}
