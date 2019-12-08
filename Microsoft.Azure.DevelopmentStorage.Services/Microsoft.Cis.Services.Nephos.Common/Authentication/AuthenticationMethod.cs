using Microsoft.Cis.Services.Nephos.Common.Account;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public delegate AuthDataEntry AuthenticationMethod(string stringToSign, string requestSignature, AuthenticationInformation authInfo);
}