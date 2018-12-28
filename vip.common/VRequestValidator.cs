using System.Web;
using System.Web.Util;

namespace vip.common
{
    public class VRequestValidator : RequestValidator
    {
        public VRequestValidator() { }

        protected override bool IsValidRequestString(HttpContext context, string value, RequestValidationSource requestValidationSource, string collectionKey, out int validationFailureIndex)
        {
            validationFailureIndex = -1;
            if (((requestValidationSource == RequestValidationSource.QueryString) || (requestValidationSource == RequestValidationSource.Form)) && (!string.IsNullOrEmpty(collectionKey) && collectionKey.Contains("crv_")))
            {
                return true;
            }
            else
            {
                return base.IsValidRequestString(context, value, requestValidationSource, collectionKey, out validationFailureIndex);
            }
        }
    }
}
