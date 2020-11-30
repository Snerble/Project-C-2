using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

using QARS.Data.Models;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace QARS.Data.Authentication
{
	public partial class QARSUserStore : IUserStore<User>, IQueryableUserStore<User>
	{
		/// <summary>
		/// Initializes a new instance of <see cref="QARSUserStore"/>.
		/// </summary>
		/// <param name="dbContext">The database context used to obtain the <see cref="User"/> instances.</param>
		/// <param name="roleManager"></param>
		public QARSUserStore(AppDbContext dbContext, RoleManager<Role> roleManager)
		{
			DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
			RoleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
		}

		public IQueryable<User> Users => DbContext.Users;

		private AppDbContext DbContext { get; }
		private RoleManager<Role> RoleManager { get; }

		public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
		{
			EntityEntry<User> entry = await DbContext.AddAsync(user, cancellationToken);
			if (entry.State == EntityState.Added)
			{
				// Add the new user to it's default role
				await AddToRoleAsync(user, user switch
				{
					Administrator _ => Role.Admin,
					Customer _ => Role.Customer,
					Employee _ => Role.Employee,
					Franchisee _ => Role.Franchisee,
					_ => throw new ArgumentException($"Unknown user type ")
				}, cancellationToken);

				// Save changes (this is done late to ensure that the role was also added succesfully)
				await DbContext.SaveChangesAsync(cancellationToken);
				return IdentityResult.Success;
			}

			// Save changes to ensure that nothing lingers
			await DbContext.SaveChangesAsync(cancellationToken);
			return IdentityResult.Failed();
		}

		public async Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
		{
			int.TryParse(userId, out int id);
			return await Users.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
		}

		public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
			=> FindByEmailAsync(normalizedUserName, cancellationToken);

		public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
			=> Task.FromResult(user.Id.ToString());

		public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
			=> GetEmailAsync(user, cancellationToken);

		public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
			=> GetNormalizedEmailAsync(user, cancellationToken);

		public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
			=> SetEmailAsync(user, userName, cancellationToken);

		public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
			=> SetNormalizedEmailAsync(user, normalizedName, cancellationToken);

		public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
		{
			EntityEntry<User> entry = DbContext.Update(user);
			await DbContext.SaveChangesAsync(cancellationToken);
			if (entry.State == EntityState.Detached)
				return IdentityResult.Failed();
			return IdentityResult.Success;
		}

		public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
		{
			EntityEntry<User> entry = DbContext.Remove(user);
			await DbContext.SaveChangesAsync(cancellationToken);
			if (entry.State == EntityState.Deleted)
				return IdentityResult.Success;
			return IdentityResult.Failed();
		}

		public void Dispose()
		{
			RoleManager.Dispose();
			DbContext.Dispose();
		}
	}
}
