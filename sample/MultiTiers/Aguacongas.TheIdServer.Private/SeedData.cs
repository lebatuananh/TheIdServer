﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2022 @Olivier Lefebvre
using Aguacongas.IdentityServer.EntityFramework.Store;
using Aguacongas.IdentityServer.Store;
using Aguacongas.TheIdServer.Data;
using Aguacongas.TheIdServer.Models;
using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Entity = Aguacongas.IdentityServer.Store.Entity;

namespace Aguacongas.TheIdServer
{
    static class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var configContext = scope.ServiceProvider.GetRequiredService<ConfigurationDbContext>();
            configContext.Database.Migrate();

            var opContext = scope.ServiceProvider.GetRequiredService<OperationalDbContext>();
            opContext.Database.Migrate();

            var appcontext = scope.ServiceProvider.GetService<ApplicationDbContext>();
            appcontext.Database.Migrate();

            SeedUsers(scope);
            SeedConfiguration(scope);
        }

        public static void SeedConfiguration(IServiceScope scope)
        {
            var provider = scope.ServiceProvider;
            SeedClients(provider);
            SeedIdentities(provider);
            SeedApiScopes(provider);
            SeedApis(provider);
        }

        private static void SeedApis(IServiceProvider provider)
        {
            var apiStore = provider.GetRequiredService<IAdminStore<Entity.ProtectResource>>();
            var apiClaimStore = provider.GetRequiredService<IAdminStore<Entity.ApiClaim>>();
            var apiSecretStore = provider.GetRequiredService<IAdminStore<Entity.ApiSecret>>();
            var apiApiScopeStore = provider.GetRequiredService<IAdminStore<Entity.ApiApiScope>>();
            var apiPropertyStore = provider.GetRequiredService<IAdminStore<Entity.ApiProperty>>();

            foreach (var resource in Config.GetApis())
            {
                if (apiStore.GetAsync(resource.Name, null).GetAwaiter().GetResult() != null)
                {
                    continue;
                }

                apiStore.CreateAsync(new Entity.ProtectResource
                {
                    Description = resource.Description,
                    DisplayName = resource.DisplayName,
                    Enabled = resource.Enabled,
                    Id = resource.Name,
                }).GetAwaiter().GetResult();
                SeedApiClaims(apiClaimStore, resource);
                SeedApiSecrets(apiSecretStore, resource);
                SeedApiApiScopes(apiApiScopeStore, resource);
                SeedApiProperties(apiPropertyStore, resource);

                Console.WriteLine($"Add api resource {resource.DisplayName}");
            }
        }

        private static void SeedApiProperties(IAdminStore<Entity.ApiProperty> apiPropertyStore, Duende.IdentityServer.Models.ApiResource resource)
        {
            foreach (var property in resource.Properties)
            {
                apiPropertyStore.CreateAsync(new Entity.ApiProperty
                {
                    ApiId = resource.Name,
                    Id = Guid.NewGuid().ToString(),
                    Key = property.Key,
                    Value = property.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedApiApiScopes(IAdminStore<Entity.ApiApiScope> apiApiScopeStore, Duende.IdentityServer.Models.ApiResource resource)
        {
            foreach (var apiScope in resource.Scopes)
            {
                apiApiScopeStore.CreateAsync(new Entity.ApiApiScope
                {
                    ApiId = resource.Name,
                    ApiScopeId = apiScope,
                    Id = Guid.NewGuid().ToString()
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedApiSecrets(IAdminStore<Entity.ApiSecret> apiSecretStore, Duende.IdentityServer.Models.ApiResource resource)
        {
            foreach (var secret in resource.ApiSecrets)
            {
                apiSecretStore.CreateAsync(new Entity.ApiSecret
                {
                    ApiId = resource.Name,
                    Expiration = secret.Expiration,
                    Description = secret.Description,
                    Id = Guid.NewGuid().ToString(),
                    Type = secret.Type,
                    Value = secret.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedApiClaims(IAdminStore<Entity.ApiClaim> apiClaimStore, Duende.IdentityServer.Models.ApiResource resource)
        {
            foreach (var claim in resource.UserClaims)
            {
                apiClaimStore.CreateAsync(new Entity.ApiClaim
                {
                    ApiId = resource.Name,
                    Id = Guid.NewGuid().ToString(),
                    Type = claim
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedApiScopes(IServiceProvider provider)
        {
            var apiScopeStore = provider.GetRequiredService<IAdminStore<Entity.ApiScope>>();
            var apiScopeClaimStore = provider.GetRequiredService<IAdminStore<Entity.ApiScopeClaim>>();
            var apiScopePropertyStore = provider.GetRequiredService<IAdminStore<Entity.ApiScopeProperty>>();
            foreach (var resource in Config.GetApiScopes())
            {
                if (apiScopeStore.GetAsync(resource.Name, null).GetAwaiter().GetResult() != null)
                {
                    continue;
                }

                apiScopeStore.CreateAsync(new Entity.ApiScope
                {
                    Description = resource.Description,
                    DisplayName = resource.DisplayName,
                    Emphasize = resource.Emphasize,
                    Enabled = resource.Enabled,
                    Id = resource.Name,
                    Required = resource.Required,
                    ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument
                }).GetAwaiter().GetResult();

                SeedApiScopeClaims(apiScopeClaimStore, resource);
                SeedApiScopeProperties(apiScopePropertyStore, resource);

                Console.WriteLine($"Add api scope resource {resource.DisplayName}");
            }
        }

        private static void SeedApiScopeProperties(IAdminStore<Entity.ApiScopeProperty> apiScopePropertyStore, Duende.IdentityServer.Models.ApiScope resource)
        {
            foreach (var property in resource.Properties)
            {
                apiScopePropertyStore.CreateAsync(new Entity.ApiScopeProperty
                {
                    ApiScopeId = resource.Name,
                    Id = Guid.NewGuid().ToString(),
                    Key = property.Key,
                    Value = property.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedApiScopeClaims(IAdminStore<Entity.ApiScopeClaim> apiScopeClaimStore, Duende.IdentityServer.Models.ApiScope resource)
        {
            foreach (var claim in resource.UserClaims)
            {
                apiScopeClaimStore.CreateAsync(new Entity.ApiScopeClaim
                {
                    ApiScopeId = resource.Name,
                    Id = Guid.NewGuid().ToString(),
                    Type = claim
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedIdentities(IServiceProvider provider)
        {
            var identityStore = provider.GetRequiredService<IAdminStore<Entity.IdentityResource>>();
            var identityClaimStore = provider.GetRequiredService<IAdminStore<Entity.IdentityClaim>>();
            var identityPropertyStore = provider.GetRequiredService<IAdminStore<Entity.IdentityProperty>>();
            foreach (var resource in Config.GetIdentityResources())
            {
                if (identityStore.GetAsync(resource.Name, null).GetAwaiter().GetResult() != null)
                {
                    continue;
                }

                identityStore.CreateAsync(new Entity.IdentityResource
                {
                    Description = resource.Description,
                    DisplayName = resource.DisplayName,
                    Emphasize = resource.Emphasize,
                    Enabled = resource.Enabled,
                    Id = resource.Name,
                    Required = resource.Required,
                    ShowInDiscoveryDocument = resource.ShowInDiscoveryDocument
                }).GetAwaiter().GetResult();
                SeedIdentityClaims(identityClaimStore, resource);
                SeedIdentityProperties(identityPropertyStore, resource);

                Console.WriteLine($"Add identity resource {resource.DisplayName}");
            }
        }

        private static void SeedIdentityProperties(IAdminStore<Entity.IdentityProperty> identityPropertyStore, Duende.IdentityServer.Models.IdentityResource resource)
        {
            foreach (var property in resource.Properties)
            {
                identityPropertyStore.CreateAsync(new Entity.IdentityProperty
                {
                    Id = Guid.NewGuid().ToString(),
                    IdentityId = resource.Name,
                    Key = property.Key,
                    Value = property.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedIdentityClaims(IAdminStore<Entity.IdentityClaim> identityClaimStore, Duende.IdentityServer.Models.IdentityResource resource)
        {
            foreach (var claim in resource.UserClaims)
            {
                identityClaimStore.CreateAsync(new Entity.IdentityClaim
                {
                    Id = Guid.NewGuid().ToString(),
                    IdentityId = resource.Name,
                    Type = claim
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClients(IServiceProvider provider)
        {
            var clientStore = provider.GetRequiredService<IAdminStore<Entity.Client>>();
            var clientGrantTypeStore = provider.GetRequiredService<IAdminStore<Entity.ClientGrantType>>();
            var clientScopeStore = provider.GetRequiredService<IAdminStore<Entity.ClientScope>>();
            var clientClaimStore = provider.GetRequiredService<IAdminStore<Entity.ClientClaim>>();
            var clientSecretStore = provider.GetRequiredService<IAdminStore<Entity.ClientSecret>>();
            var clientIdpRestrictionStore = provider.GetRequiredService<IAdminStore<Entity.ClientIdpRestriction>>();
            var clientUriStore = provider.GetRequiredService<IAdminStore<Entity.ClientUri>>();
            var clientPropertyStore = provider.GetRequiredService<IAdminStore<Entity.ClientProperty>>();

            foreach (var client in Config.GetClients())
            {
                if (clientStore.GetAsync(client.ClientId, null).GetAwaiter().GetResult() != null)
                {
                    continue;
                }

                clientStore.CreateAsync(new Entity.Client
                {
                    AbsoluteRefreshTokenLifetime = client.AbsoluteRefreshTokenLifetime,
                    AccessTokenLifetime = client.AccessTokenLifetime,
                    AccessTokenType = (int)client.AccessTokenType,
                    AllowAccessTokensViaBrowser = client.AllowAccessTokensViaBrowser,
                    AllowOfflineAccess = client.AllowOfflineAccess,
                    AllowPlainTextPkce = client.AllowPlainTextPkce,
                    AllowRememberConsent = client.AllowRememberConsent,
                    AlwaysIncludeUserClaimsInIdToken = client.AlwaysIncludeUserClaimsInIdToken,
                    AlwaysSendClientClaims = client.AlwaysSendClientClaims,
                    AuthorizationCodeLifetime = client.AuthorizationCodeLifetime,
                    BackChannelLogoutSessionRequired = client.BackChannelLogoutSessionRequired,
                    BackChannelLogoutUri = client.BackChannelLogoutUri,
                    ClientClaimsPrefix = client.ClientClaimsPrefix,
                    ClientName = client.ClientName,
                    ClientUri = client.ClientUri,
                    ConsentLifetime = client.ConsentLifetime,
                    Description = client.Description,
                    DeviceCodeLifetime = client.DeviceCodeLifetime,
                    Enabled = client.Enabled,
                    EnableLocalLogin = client.EnableLocalLogin,
                    FrontChannelLogoutSessionRequired = client.FrontChannelLogoutSessionRequired,
                    FrontChannelLogoutUri = client.FrontChannelLogoutUri,
                    Id = client.ClientId,
                    IdentityTokenLifetime = client.IdentityTokenLifetime,
                    IncludeJwtId = client.IncludeJwtId,
                    LogoUri = client.LogoUri,
                    PairWiseSubjectSalt = client.PairWiseSubjectSalt,
                    ProtocolType = client.ProtocolType,
                    RefreshTokenExpiration = (int)client.RefreshTokenExpiration,
                    RefreshTokenUsage = (int)client.RefreshTokenUsage,
                    RequireClientSecret = client.RequireClientSecret,
                    RequireConsent = client.RequireConsent,
                    RequirePkce = client.RequirePkce,
                    SlidingRefreshTokenLifetime = client.SlidingRefreshTokenLifetime,
                    UpdateAccessTokenClaimsOnRefresh = client.UpdateAccessTokenClaimsOnRefresh,
                    UserCodeType = client.UserCodeType,
                    UserSsoLifetime = client.UserSsoLifetime
                }).GetAwaiter().GetResult();
                SeedClientGrantType(clientGrantTypeStore, client);
                SeedClientScopes(clientScopeStore, client);
                SeedClientClaims(clientClaimStore, client);
                SeedClientSecrets(clientSecretStore, client);
                SeedClientRestrictions(clientIdpRestrictionStore, client);
                SeedClientProperties(clientPropertyStore, client);
                SeedClientUris(clientUriStore, client);

                Console.WriteLine($"Add client {client.ClientName}");
            }
        }

        private static void SeedClientUris(IAdminStore<Entity.ClientUri> clientUriStore, Duende.IdentityServer.Models.Client client)
        {
            var uris = client.RedirectUris.Select(o => new Entity.ClientUri
            {
                Id = Guid.NewGuid().ToString(),
                ClientId = client.ClientId,
                Uri = o
            }).ToList();

            foreach (var origin in client.AllowedCorsOrigins)
            {
                var cors = new Uri(origin);
                var uri = uris.FirstOrDefault(u => cors.CorsMatch(u.Uri));
                var corsUri = new Uri(origin);
                var sanetized = $"{corsUri.Scheme.ToUpperInvariant()}://{corsUri.Host.ToUpperInvariant()}:{corsUri.Port}";

                if (uri == null)
                {

                    uris.Add(new Entity.ClientUri
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClientId = client.ClientId,
                        Uri = origin,
                        Kind = Entity.UriKinds.Cors,
                        SanetizedCorsUri = sanetized
                    });
                    continue;
                }

                uri.SanetizedCorsUri = sanetized;
                uri.Kind = Entity.UriKinds.Redirect | Entity.UriKinds.Cors;
            }

            foreach (var postLogout in client.PostLogoutRedirectUris)
            {
                var uri = uris.FirstOrDefault(u => u.Uri == postLogout);
                if (uri == null)
                {
                    uris.Add(new Entity.ClientUri
                    {
                        Id = Guid.NewGuid().ToString(),
                        ClientId = client.ClientId,
                        Uri = postLogout,
                        Kind = Entity.UriKinds.PostLogout
                    });
                    continue;
                }

                uri.Kind |= Entity.UriKinds.Redirect;
            }

            foreach (var uri in uris)
            {
                clientUriStore.CreateAsync(uri).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientProperties(IAdminStore<Entity.ClientProperty> clientPropertyStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var property in client.Properties)
            {
                clientPropertyStore.CreateAsync(new Entity.ClientProperty
                {
                    ClientId = client.ClientId,
                    Id = Guid.NewGuid().ToString(),
                    Key = property.Key,
                    Value = property.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientRestrictions(IAdminStore<Entity.ClientIdpRestriction> clientIdpRestrictionStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var restriction in client.IdentityProviderRestrictions)
            {
                clientIdpRestrictionStore.CreateAsync(new Entity.ClientIdpRestriction
                {
                    ClientId = client.ClientId,
                    Id = Guid.NewGuid().ToString(),
                    Provider = restriction
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientSecrets(IAdminStore<Entity.ClientSecret> clientSecretStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var secret in client.ClientSecrets)
            {
                clientSecretStore.CreateAsync(new Entity.ClientSecret
                {
                    ClientId = client.ClientId,
                    Description = secret.Description,
                    Expiration = secret.Expiration,
                    Id = Guid.NewGuid().ToString(),
                    Type = secret.Type,
                    Value = secret.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientClaims(IAdminStore<Entity.ClientClaim> clientClaimStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var claim in client.Claims)
            {
                clientClaimStore.CreateAsync(new Entity.ClientClaim
                {
                    ClientId = client.ClientId,
                    Id = Guid.NewGuid().ToString(),
                    Type = claim.Type,
                    Value = claim.Value
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientScopes(IAdminStore<Entity.ClientScope> clientScopeStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var clientScope in client.AllowedScopes)
            {
                clientScopeStore.CreateAsync(new Entity.ClientScope
                {
                    ClientId = client.ClientId,
                    Scope = clientScope,
                    Id = Guid.NewGuid().ToString()
                }).GetAwaiter().GetResult();
            }
        }

        private static void SeedClientGrantType(IAdminStore<Entity.ClientGrantType> clientGrantTypeStore, Duende.IdentityServer.Models.Client client)
        {
            foreach (var grantType in client.AllowedGrantTypes)
            {
                clientGrantTypeStore.CreateAsync(new Entity.ClientGrantType
                {
                    ClientId = client.ClientId,
                    GrantType = grantType,
                    Id = Guid.NewGuid().ToString()
                }).GetAwaiter().GetResult();
            }
        }

        public static void SeedUsers(IServiceScope scope)
        {
            var context = scope.ServiceProvider.GetService<ApplicationDbContext>();

            var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new string[]
            {
                "Is4-Writer",
                "Is4-Reader"
            };
            foreach (var role in roles)
            {
                if (roleMgr.FindByNameAsync(role).GetAwaiter().GetResult() == null)
                {
                    ExcuteAndCheckResult(() => roleMgr.CreateAsync(new IdentityRole
                    {
                        Name = role
                    })).GetAwaiter().GetResult();
                }
            }

            var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var alice = userMgr.FindByNameAsync("alice").Result;
            if (alice == null)
            {
                alice = new ApplicationUser
                {
                    UserName = "alice"
                };
                ExcuteAndCheckResult(() => userMgr.CreateAsync(alice, "Pass123$"))
                    .GetAwaiter().GetResult();

                ExcuteAndCheckResult(() => userMgr.AddClaimsAsync(alice, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", Duende.IdentityServer.IdentityServerConstants.ClaimValueTypes.Json)
                    })).GetAwaiter().GetResult();

                ExcuteAndCheckResult(() => userMgr.AddToRolesAsync(alice, roles))
                    .GetAwaiter().GetResult();

                Console.WriteLine("alice created");
            }
            else
            {
                Console.WriteLine("alice already exists");
            }

            var bob = userMgr.FindByNameAsync("bob").GetAwaiter().GetResult();
            if (bob == null)
            {
                bob = new ApplicationUser
                {
                    UserName = "bob"
                };
                ExcuteAndCheckResult(() => userMgr.CreateAsync(bob, "Pass123$"))
                    .GetAwaiter().GetResult();

                ExcuteAndCheckResult(() => userMgr.AddClaimsAsync(bob, new Claim[]{
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", Duende.IdentityServer.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "somewhere")
                    })).GetAwaiter().GetResult();
                ExcuteAndCheckResult(() => userMgr.AddToRoleAsync(bob, "Is4-Reader"))
                    .GetAwaiter().GetResult();
                Console.WriteLine("bob created");
            }
            else
            {
                Console.WriteLine("bob already exists");
            }
            context.SaveChanges();
        }

        [SuppressMessage("Major Code Smell", "S112:General exceptions should never be thrown", Justification = "Seeding")]
        private static async Task ExcuteAndCheckResult(Func<Task<IdentityResult>> action)
        {
            var result = await action.Invoke();
            if (!result.Succeeded)
            {
                throw new Exception(result.Errors.First().Description);
            }
        }
    }
}
