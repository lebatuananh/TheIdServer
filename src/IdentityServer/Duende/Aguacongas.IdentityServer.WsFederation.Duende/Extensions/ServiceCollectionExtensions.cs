﻿// Project: Aguafrommars/TheIdServer
// Copyright (c) 2022 @Olivier Lefebvre
using Aguacongas.IdentityServer.WsFederation;
using Aguacongas.IdentityServer.WsFederation.Metadata;
using Aguacongas.IdentityServer.WsFederation.Stores;
using Aguacongas.IdentityServer.WsFederation.Validation;
using Duende.IdentityServer.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// IServiceCollection extensions to add WS-Federation servives
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds WS-Federation services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="configuration">Configuration to bind to <see cref="WsFederationOptions"/></param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityServerWsFederation(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<WsFederationOptions>(configuration);
            return AddWsFederationSevices(services);
        }

        /// <summary>
        /// Adds WS-Federation services.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="options">Options to configure metadata</param>
        /// <returns></returns>
        public static IServiceCollection AddIdentityServerWsFederation(this IServiceCollection services, WsFederationOptions options = null)
        {
            services.Configure<WsFederationOptions>(o =>
            {
                if (options == null)
                {
                    return;
                }
                o.ClaimTypesOffered = options.ClaimTypesOffered;
                o.ClaimTypesRequested = options.ClaimTypesRequested;
                o.TokenTypesOffered = options.TokenTypesOffered;
            });
            return AddWsFederationSevices(services);
        }

        private static IServiceCollection AddWsFederationSevices(IServiceCollection services)
        {
            services.TryAddTransient<IRelyingPartyStore, RelyingPartyStore>();
            services.TryAddTransient<IMetatdataSerializer, MetadataSerializer>();
            return services.AddTransient<IMetadataResponseGenerator, MetadataResponseGenerator>()
                .AddTransient<IWsFederationService, WsFederationService>()
                .AddTransient<ISignInValidator, SignInValidator>()
                .AddTransient<ISignInResponseGenerator, SignInResponseGenerator>()
                .AddTransient<EndSessionRequestValidator>()
                .AddTransient<IEndSessionRequestValidator, WsFederationEndSessionRequestValidator>();
        }
    }
}
