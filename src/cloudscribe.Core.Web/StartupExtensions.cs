﻿// Copyright (c) Source Tree Solutions, LLC. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// Author:					Joe Audette
// Created:					2016-05-07
// Last Modified:			2016-11-26
// 


using cloudscribe.Core.Models;
using cloudscribe.Core.Models.Setup;
using cloudscribe.Core.Web;
using cloudscribe.Core.Web.Components;
using cloudscribe.Core.Web.Components.Editor;
using cloudscribe.Core.Web.Components.Messaging;
using cloudscribe.Core.Web.Navigation;
using cloudscribe.Web.Common.Razor;
using cloudscribe.Web.Navigation;
using cloudscribe.Web.Navigation.Caching;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using cloudscribe.Messaging.Email;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddCloudscribeCore(this IServiceCollection services, IConfigurationRoot configuration)
        {
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.Configure<MultiTenantOptions>(configuration.GetSection("MultiTenantOptions"));
            services.Configure<SmtpOptions>(configuration.GetSection("SmtpOptions"));
            services.Configure<SiteConfigOptions>(configuration.GetSection("SiteConfigOptions"));
            services.Configure<UIOptions>(configuration.GetSection("UIOptions"));
            services.Configure<CkeditorOptions>(configuration.GetSection("CkeditorOptions"));
            services.Configure<CachingSiteResolverOptions>(configuration.GetSection("CachingSiteResolverOptions"));
            
           
            //services.AddMultitenancy<SiteSettings, SiteResolver>();
            
            services.AddMultitenancy<SiteContext, CachingSiteResolver>();
            services.AddScoped<CacheHelper, CacheHelper>();

            services.AddScoped<SiteEvents, SiteEvents>();
            services.AddScoped<SiteManager, SiteManager>();
            services.AddScoped<GeoDataManager, GeoDataManager>();
            services.AddScoped<SystemInfoManager, SystemInfoManager>();
            services.AddScoped<IpAddressTracker, IpAddressTracker>();

            services.AddScoped<SiteDataProtector>();
            // timezone localization from NodaTime
            services.AddCloudscribeCommmon();
            services.AddScoped<ITimeZoneIdResolver, RequestTimeZoneIdResolver>();

            services.AddCloudscribePagination();

            services.AddScoped<IVersionProviderFactory, VersionProviderFactory>();
            services.AddScoped<IVersionProvider, CloudscribeCoreVersionProvider>();
            
            services.AddTransient<ISiteMessageEmailSender, SiteEmailMessageSender>();
            //services.AddTransient<ISiteMessageEmailSender, FakeSiteEmailSender>();
            
            services.AddTransient<ISmsSender, SiteSmsSender>();

            services.TryAddSingleton<IThemeListBuilder, SiteThemeListBuilder>();
            //services.AddSingleton<IRazorViewEngine, CoreViewEngine>();
            services.TryAddScoped<ViewRenderer, ViewRenderer>();

            services.AddSingleton<IOptions<NavigationOptions>, SiteNavigationOptionsResolver>();
            services.AddScoped<ITreeCacheKeyResolver, SiteNavigationCacheKeyResolver>();
            services.AddScoped<INodeUrlPrefixProvider, FolderTenantNodeUrlPrefixProvider>();
            services.AddCloudscribeNavigation(configuration);

            services.AddCloudscribeIdentity();

            services.AddScoped<IUserContextResolver, UserContextResolver>();
            services.AddScoped<ISiteIdResolver, SiteIdResolver>();

            return services;
        }

        /// <summary>
        /// This method adds an embedded file provider to the RazorViewOptions to be able to load the cloudscribe Core related views.
        /// If you download and install the views below your view folder you don't need this method and you can customize the views.
        /// You can get the views from https://github.com/joeaudette/cloudscribe/tree/master/src/cloudscribe.Core.Web/Views
        /// </summary>
        /// <param name="options"></param>
        /// <returns>RazorViewEngineOptions</returns>
        [Obsolete("AddEmbeddedViewsForCloudscribeCore is deprecated, please use AddEmbeddedBootstrap3ViewsForCloudscribeCore instead.")]
        public static RazorViewEngineOptions AddEmbeddedViewsForCloudscribeCore(this RazorViewEngineOptions options)
        {
            //options.FileProviders.Add(new EmbeddedFileProvider(
            //        typeof(SiteManager).GetTypeInfo().Assembly,
            //        "cloudscribe.Core.Web"
            //    ));
            options.AddEmbeddedBootstrap3ViewsForCloudscribeCore();

            return options;
        }

        

        public static RazorViewEngineOptions AddCloudscribeViewLocationFormats(this RazorViewEngineOptions options)
        {
            options.ViewLocationFormats.Add("/Views/Sys/{1}/{0}" + RazorViewEngine.ViewExtension);
            options.ViewLocationFormats.Add("/Views/Sys/Shared/{0}" + RazorViewEngine.ViewExtension);

            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Sys/{1}/{0}" + RazorViewEngine.ViewExtension);
            options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Sys/Shared/{0}" + RazorViewEngine.ViewExtension);
            options.AreaViewLocationFormats.Add("/Views/Sys/Shared/{0}" + RazorViewEngine.ViewExtension);

            return options;
        }

        public static AuthorizationOptions AddCloudscribeCoreDefaultPolicies(this AuthorizationOptions options)
        {
            options.AddPolicy(
                    "ServerAdminPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("ServerAdmins");
                    });

            options.AddPolicy(
                "CoreDataPolicy",
                authBuilder =>
                {
                    authBuilder.RequireRole("ServerAdmins");
                });

            options.AddPolicy(
                "AdminPolicy",
                authBuilder =>
                {
                    authBuilder.RequireRole("ServerAdmins", "Administrators");
                });

            options.AddPolicy(
                "UserManagementPolicy",
                authBuilder =>
                {
                    authBuilder.RequireRole("ServerAdmins", "Administrators");
                });

            options.AddPolicy(
                "RoleAdminPolicy",
                authBuilder =>
                {
                    authBuilder.RequireRole("Role Administrators", "Administrators");
                });

            return options;
        }

    }
}