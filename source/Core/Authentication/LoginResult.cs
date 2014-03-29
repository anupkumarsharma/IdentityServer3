﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Thinktecture.IdentityServer.Core.Services;

namespace Thinktecture.IdentityServer.Core.Authentication
{
    public class LoginResult : IHttpActionResult
    {
        SignInMessage _message;
        HttpRequestMessage _request;
        private ICoreSettings _settings;

        public LoginResult(SignInMessage message, HttpRequestMessage request, ICoreSettings settings)
        {
            _message = message;
            _settings = settings;
            _request = request;
        }

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute());
        }

        private HttpResponseMessage Execute()
        {
            HttpResponseMessage response = new HttpResponseMessage(HttpStatusCode.Redirect);

            try
            {
                var protection = _settings.GetInternalProtectionSettings();
                var jwt = _message.ToJwt(
                    protection.Issuer,
                    protection.Audience,
                    protection.SigningKey,
                    protection.Ttl);

                var urlHelper = _request.GetUrlHelper();
                var loginUrl = urlHelper.Route("login", new { message = jwt });
                var uri = new Uri(_request.RequestUri, loginUrl);

                response.Headers.Location = uri;
            }
            catch
            {
                response.Dispose();
                throw;
            }

            return response;
        }
    }
}