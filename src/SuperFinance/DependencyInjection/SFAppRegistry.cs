using System;
using ASKSource.AuthDefinitions;
using ASKSource.DataModels;
using ASKSource.Managers;
using ASPSecurityKit;
using ASPSecurityKit.Authorization;
using ASPSecurityKit.Net;
using Autofac;
using SuperFinance.AuthDefinitions;
using SuperFinance.DataModels;
using SuperFinance.Managers;
using SuperFinance.Repositories;
using SuperFinance.Security;

namespace SuperFinance.DependencyInjection
{
	public class SFAppRegistry : Module
	{
		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<SFAuthEvents>()
				.As<ISecurityEvents>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SFIdentityRepository>()
				.As<IIdentityRepository>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SFUserService>()
				.As<ISFUserService>()
				.As<IUserService<Guid, Guid, DbUser>>()
				.As<IUserService>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SFReferencesProvider>()
				.As<IReferencesProvider<IdMemberReference>>()
				.InstancePerLifetimeScope();

			builder.RegisterType<DemoDbContext>()
				.AsSelf()
				.As<AppDbContext>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SFUserManager>()
				.As<IUserManager>()
				.As<ISFUserManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<BankManager>()
				.As<IBankManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<BranchManager>()
				.As<IBranchManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<AccountManager>()
				.As<IAccountManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<AccountTypeManager>()
				.As<IAccountTypeManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<TransactionManager>()
				.As<ITransactionManager>()
				.InstancePerLifetimeScope();

			builder.RegisterType<SFSecuritySettings>()
				.As<ISecuritySettings>()
				.As<INetSecuritySettings>()
				.InstancePerLifetimeScope();
		}
	}
}