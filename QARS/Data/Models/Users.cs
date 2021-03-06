using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace QARS.Data.Models
{
	/// <summary>
	/// Serves as the base class for all user types in <see cref="QARS"/>.
	/// </summary>
	[DebuggerDisplay("Id = {Id}, Email = {Email}")]
	public abstract class User
	{
		/// <summary>
		/// Gets or sets the primary key of this <see cref="User"/>.
		/// </summary>
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int Id { get; set; }
		/// <summary>
		/// Gets or sets the e-mail address of this <see cref="User"/>.
		/// </summary>
		[Required, EmailAddress, PersonalData]
		public string Email { get; set; }
		/// <summary>
		/// Gets or sets the normalized e-mail of this <see cref="User"/>.
		/// <para/>
		/// This property is used to optimize queries.
		/// </summary>
		[Required, EmailAddress]
		public string NormalizedEmail { get; set; }

		/// <summary>
		/// Gets or sets whether this <see cref="User"/>'s email has been verified.
		/// </summary>
		public bool IsEmailVerified { get; set; } = false;

		/// <summary>
		/// Gets or sets the hashed password for this <see cref="User"/>.
		/// </summary>
		[MinLength(64), MaxLength(64)]
		public byte[] Password { get; set; }
		/// <summary>
		/// Gets or sets the raw password.
		/// <para/>
		/// This property is not mapped and is only intended for temporary storage.
		/// </summary>
		[Required, NotMapped]
		internal string RawPassword { get; set; }

		/// <summary>
		/// Gets or sets the first name of this <see cref="User"/>.
		/// </summary>
		[Required, MinLength(1), PersonalData]
		public string FirstName { get; set; }
		/// <summary>
		/// Gets or sets the last name of this <see cref="User"/>.
		/// </summary>
		[Required, MinLength(1), PersonalData]
		public string LastName { get; set; }
		/// <summary>
		/// Gets or sets the phone number of this <see cref="User"/>.
		/// </summary>
		[Required, Phone, PersonalData]
		public string PhoneNumber { get; set; }

		/// <summary>
		/// Gets or sets the id of the <see cref="Location"/> of this <see cref="User"/>.
		/// </summary>
		public int LocationId { get; set; }
		/// <summary>
		/// Gets or sets the location where this <see cref="User"/> resides.
		/// </summary>
		public virtual Location Location { get; set; }

		/// <summary>
		/// Gets or sets a list of <see cref="UserRole"/>s for this user.
		/// </summary>
		public List<UserRole> Roles { get; set; }

		[PersonalData]
		public async Task<Dictionary<string, object>> GetPersonalData(AppDbContext dbContext)
		{
			await dbContext.Entry(this).Reference(x => x.Location).LoadAsync();
			return new Dictionary<string, object> { { "Location", Location } };
		}

		public override string ToString() => Utils.GetDetailedString(this);

		/// <summary>
		/// Generates and returns a new salted <see cref="SHA512"/> hash.
		/// </summary>
		/// <param name="salt">A unique string (preferably a username) to add before hashing.</param>
		/// <param name="password">The password that will be hashed.</param>
		/// <exception cref="ArgumentException"><paramref name="salt"/> or <paramref name="password"/> are null or empty.</exception>
		public static byte[] GetPasswordHash(string salt, string password)
		{
			if (string.IsNullOrEmpty(salt))
				throw new ArgumentException($"'{nameof(salt)}' cannot be null or empty", nameof(salt));
			if (string.IsNullOrEmpty(password))
				throw new ArgumentException($"'{nameof(password)}' cannot be null or empty", nameof(password));

			string saltedString = $"{salt}#:#{password}"; // https://imgur.com/k2L1aQg
			using var sha512 = SHA512.Create();
			return sha512.ComputeHash(Encoding.UTF8.GetBytes(saltedString));
		}
	}

	/// <summary>
	/// Represents a customer at <see cref="QARS"/>.
	/// </summary>
	public class Customer : User {

		public byte[] DriversLicenseFront { get; set; }

		public byte[] DriversLicenseBack { get; set; }

	}
	/// <summary>
	/// Represents a franchise owner.
	/// </summary>
	public class Franchisee : User { }
	/// <summary>
	/// Represents an employee working at <see cref="QARS"/>.
	/// </summary>
	public class Employee : User {

		public int FranchiseeId { get; set; }

		public int StoreId { get; set; }

	}
	/// <summary>
	/// Represents a user with special privileges in the system.
	/// </summary>
	public class Administrator : Employee { }
}
