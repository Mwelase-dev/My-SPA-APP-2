using System.Security.Claims;
using System.Linq;
using NUnit.Framework;

namespace NUnit.Asserts.Microsoft.Owin.Security
{
    public class ClaimsIdentityAssert
    {
        public static void ContainsClaim(string type, string value, ClaimsIdentity identity)
        {
            var expectedClaim = new Claim(type, value);
            var actualClaimsList = identity.Claims.Select(c => c.ToString());
            var actualClaimsMsg = actualClaimsList.Aggregate((i, j) => i + "\n" + j);

            var errorMessage = string.Format("Claim:\n{0}\nnot found between actual Claims:\n{1}", expectedClaim.ToString(), actualClaimsMsg);
            CollectionAssert.Contains(actualClaimsList, expectedClaim.ToString(), errorMessage);
        }
    }
}
