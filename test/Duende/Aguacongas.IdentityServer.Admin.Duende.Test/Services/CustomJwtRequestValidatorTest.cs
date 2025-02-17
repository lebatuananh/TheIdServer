﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2022 @Olivier Lefebvre
using Aguacongas.IdentityServer.Admin.Services;
using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Duende.IdentityServer.Services;
using ISConfiguration = Duende.IdentityServer.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Aguacongas.IdentityServer.Admin.Test.Services
{
    public class CustomJwtRequestValidatorTest
    {
        [Fact]
        public async Task ValidateJwtAsync_should_validate_and_return_token()
        {
            var tokenValidationOptionsMock = new Mock<IOptions<TokenValidationParameters>>();
            var issuerNameServiceMock = new Mock<IIssuerNameService>();
            var options = new ISConfiguration.IdentityServerOptions
            {
                IssuerUri = "http://test"
            };
            var loggerMock = new Mock<ILogger<JwtRequestValidator>>();

            Assert.Throws<ArgumentNullException>(() => new CustomJwtRequestValidator(tokenValidationOptionsMock.Object, options, issuerNameServiceMock.Object, loggerMock.Object));
            var tokenValidationParameters = new TokenValidationParameters();
            tokenValidationOptionsMock.SetupGet(m => m.Value).Returns(tokenValidationParameters);
            var httpContextMock = new Mock<HttpContext>();

            var provider = new ServiceCollection().AddTransient(p => options).BuildServiceProvider();
            httpContextMock.SetupGet(m => m.RequestServices).Returns(provider);

            var sut = new CustomJwtRequestValidator(tokenValidationOptionsMock.Object, options, issuerNameServiceMock.Object, loggerMock.Object);
            var client = new Client
            {
                ClientId = Guid.NewGuid().ToString(),
                ClientSecrets = new[]
                {
                    new Secret
                    {
                        Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                        Value = "{\"kty\": \"RSA\",\"e\": \"AQAB\",\"use\": \"sig\",\"alg\": \"RS256\",\"n\": \"qBulUDaYV027shwCq82LKIevXdQL2pCwXktQgf2TT3c496pxGdRuxcN_MHGKWNOGQsDLuAVk6NjxYF95obDUFrDiugMuXrvptPrTO8dzTX83k_6ngtjOtx2UrTk_7f0EYNrusykrsB-cOvCMREsfktlsavvMKBGrzpxaHlRxcSsMxzB0dddDSlH8mxlzOGcbBuvZnbNg0EUuQC4jvM9Gy6gUEcoU0S19XnUcgwLGLPfIX2dMO4FxTAsaaTYT7msxGMBNIVUTVnL0HctYr0YVYu0hD9rePnvxJ_-OwOdxIETQlR9vp61xFr4juzyyMWTrjCACxxLm-CyEQGjwx2YZaw\"}"
                    }
                }
            };

            var jwtString = "eyJhbGciOiJub25lIn0.eyJzY29wZSI6Im9wZW5pZCIsInJlc3BvbnNlX3R5cGUiOiJjb2RlIiwicmVkaXJlY3RfdXJpIjoiaHR0cHM6XC9cL3d3dy5jZXJ0aWZpY2F0aW9uLm9wZW5pZC5uZXRcL3Rlc3RcL2FcL3RoZWlkc2VydmVyXC9jYWxsYmFjayIsInN0YXRlIjoiRXBTcFc3clVmciIsIm5vbmNlIjoiaU5Ia3gyT3ltOSIsImNsaWVudF9pZCI6ImVjZjk1Y2Q3LWI4NDQtNGNkZS05OWE4LTc2N2EyNDNmOTZjYiJ9.";

            var result = await sut.ValidateAsync(new JwtRequestValidationContext
            {
                Client = client,
                JwtTokenString = jwtString,
            });

            Assert.True(result.IsError);

            tokenValidationParameters.ValidateIssuerSigningKey = tokenValidationParameters.ValidateIssuer
                = tokenValidationParameters.ValidateAudience
                = tokenValidationParameters.ValidateLifetime
                = tokenValidationParameters.RequireAudience
                = tokenValidationParameters.RequireSignedTokens
                = tokenValidationParameters.RequireExpirationTime
                = false;

            result = await sut.ValidateAsync(new JwtRequestValidationContext
            {
                Client = client,
                JwtTokenString = jwtString,
            });

            Assert.False(result.IsError);

            options.StrictJarValidation = true;

            result = await sut.ValidateAsync(new JwtRequestValidationContext
            {
                Client = client,
                JwtTokenString = jwtString,
            });

            Assert.True(result.IsError);
        }
    }
}
