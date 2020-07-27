﻿using Autofac;
using Fur.ApplicationBase;
using Fur.ApplicationBase.Attributes;
using Fur.DatabaseAccessor.Contexts.Pools;
using Fur.DatabaseAccessor.MultipleTenants;
using Fur.DatabaseAccessor.MultipleTenants.Providers;
using Fur.DatabaseAccessor.Repositories;
using Fur.DatabaseAccessor.Repositories.MasterSlave;
using Fur.DatabaseAccessor.Repositories.Multiple;
using Fur.DatabaseAccessor.Tangent;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fur.DatabaseAccessor.Extensions
{
    /// <summary>
    /// 数据库访问器依赖注入拓展类
    /// </summary>
    [NonWrapper]
    public static class DependencyInjectionExtensions
    {
        #region 注册上下文 + public static ContainerBuilder RegisterDbContexts<TDefaultDbContext>(this ContainerBuilder builder, params Type[] dbContextTypes)
        /// <summary>
        /// 注册上下文
        /// <para>泛型参数为默认数据库上下文</para>
        /// </summary>
        /// <typeparam name="TDefaultDbContext">默认上下文</typeparam>
        /// <param name="builder">容器构建器</param>
        /// <param name="dbContextTypes">数据库上下文集合</param>
        /// <returns><see cref="ContainerBuilder"/></returns>
        public static ContainerBuilder RegisterDbContexts<TDefaultDbContext>(this ContainerBuilder builder, Action<FurDbContextInjectionOptions> configureOptions, params Type[] dbContextTypes)
            where TDefaultDbContext : DbContext
        {
            // 注册数据库上下文池
            builder.RegisterType<DbContextPool>()
                .As<IDbContextPool>()
                .InstancePerLifetimeScope();

            // 注册默认数据库上下文
            builder.RegisterType<TDefaultDbContext>()
                .As<DbContext>()
                .InstancePerLifetimeScope();

            // 载入配置
            var furDbContextInjectionOptions = new FurDbContextInjectionOptions();
            configureOptions(furDbContextInjectionOptions);

            var dbContextTypeList = dbContextTypes.Distinct().ToList();
            dbContextTypeList.Add(typeof(TDefaultDbContext));

            // 支持切面上下文
            if (furDbContextInjectionOptions.SupportTangent)
            {
                builder.RegisterGeneric(typeof(TangentDbContextOfT<>))
                    .As(typeof(ITangentDbContextOfT<>))
                    .InstancePerLifetimeScope();
            }

            // 注册多租户
            if (furDbContextInjectionOptions.MultipleTenantProvider != null)
            {
                AppGlobal.SupportedMultipleTenant = true;
                dbContextTypeList.Add(typeof(FurMultipleTenantDbContext));

                builder.RegisterType(furDbContextInjectionOptions.MultipleTenantProvider)
                    .As<IMultipleTenantProvider>()
                    .InstancePerLifetimeScope();
            }

            // 注册仓储
            builder.RegisterRepositories(furDbContextInjectionOptions.SupportMultipleDbContext, furDbContextInjectionOptions.SupportMasterSlaveDbContext);

            // 注册多数据库上下文
            builder.RegisterDbContexts(dbContextTypeList.ToArray());

            return builder;
        }
        #endregion

        #region 注册上下文 + private static ContainerBuilder RegisterDbContexts(this ContainerBuilder builder, params Type[] dbContextTypes)
        /// <summary>
        /// 注册上下文
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <param name="dbContextTypes">数据库上下文集合</param>
        /// <returns><see cref="ContainerBuilder"/></returns>
        private static ContainerBuilder RegisterDbContexts(this ContainerBuilder builder, params Type[] dbContextTypes)
        {
            foreach (var dbContextType in dbContextTypes)
            {
                builder.RegisterType(dbContextType)
                    .Named<DbContext>(dbContextType.BaseType.GenericTypeArguments.Last().Name)
                    .InstancePerLifetimeScope();
            }
            return builder;
        }
        #endregion

        #region 注册仓储 + public static ContainerBuilder RegisterRepositories(this ContainerBuilder builder, bool supportMultiple = true, bool supportMasterSlave = true)
        /// <summary>
        /// 注册仓储
        /// </summary>
        /// <param name="builder">容器构建器</param>
        /// <param name="supportMultiple">支持多个数据库上下文，默认true：支持</param>
        /// <param name="supportMasterSlave">支持主从库数据库上下文，默认true：支持</param>
        /// <returns><see cref="ContainerBuilder"/></returns>
        private static ContainerBuilder RegisterRepositories(this ContainerBuilder builder, bool supportMultiple = true, bool supportMasterSlave = true)
        {
            // 注册泛型仓储
            builder.RegisterGeneric(typeof(EFCoreRepositoryOfT<>))
                .As(typeof(IRepositoryOfT<>))
                .InstancePerLifetimeScope();

            // 注册非泛型仓储
            builder.RegisterType<EFCoreRepository>()
                .As<Repositories.IRepository>()
                .InstancePerLifetimeScope();

            // 支持多个数据库上下文
            if (supportMultiple)
            {
                builder.RegisterGeneric(typeof(EFCoreRepositoryOfT<,>))
                    .As(typeof(IRepositoryOfT<,>))
                    .InstancePerLifetimeScope();

                builder.RegisterType<MultipleEFCoreRepository>()
                    .As<Repositories.Multiple.IMultipleRepository>()
                    .InstancePerLifetimeScope();
            }

            // 支持主从库数据库上下文
            if (supportMasterSlave)
            {
                builder.RegisterGeneric(typeof(EFCoreRepositoryOfT<,,>))
                    .As(typeof(IRepositoryOfT<,,>))
                    .InstancePerLifetimeScope();

                builder.RegisterType<MasterSlaveEFCoreRepository>()
                   .As<Repositories.MasterSlave.IMasterSlaveRepository>()
                   .InstancePerLifetimeScope();
            }

            return builder;
        }
        #endregion
    }

    /// <summary>
    /// 数据库上下文注入配置选项
    /// </summary>
    public class FurDbContextInjectionOptions
    {
        /// <summary>
        /// 是否支持切面上下文
        /// <para>默认true：支持</para>
        /// </summary>
        public bool SupportTangent { get; set; } = true;

        /// <summary>
        /// 多租户提供器
        /// <para>如果指定了，则自动启用多租户模式</para>
        /// <para>默认true：支持</para>
        /// </summary>
        public Type MultipleTenantProvider { get; set; } = null;

        /// <summary>
        /// 支持多数据库上下文
        /// </summary>
        public bool SupportMultipleDbContext { get; set; } = true;

        /// <summary>
        /// 支持主从库数据库上下文
        /// </summary>
        public bool SupportMasterSlaveDbContext { get; set; } = true;
    }
}