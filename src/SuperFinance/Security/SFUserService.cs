using System;
using ASKSource.DataModels;
using ASPSecurityKit;

namespace SuperFinance.Security
{
	public interface ISFUserService : IUserService<Guid, Guid, DbUser>
	{
		Guid? BankId { get; set; }
	}

	public class SFUserService : UserService<Guid, Guid, DbUser>, ISFUserService
	{
		public SFUserService(ISessionService session,
			IUserRepository<Guid> userRepository, IPermitRepository<Guid, Guid> permitRepository, IIdentityRepository identityRepository,
			IPermitManager<Guid> permitManager, IHashService hashService, ISecuritySettings settings, ILogger logger)
			: base(session, userRepository, permitRepository, identityRepository, permitManager, hashService, settings, logger)
		{
		}

		public Guid? BankId
		{
			get => TryGetData<Guid?>(nameof(BankId)).Value;
			set => PutData(nameof(BankId), value);
		}
	}
}