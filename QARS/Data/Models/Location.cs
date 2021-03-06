using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace QARS.Data.Models
{
	/// <summary>
	/// Describes a physical location.
	/// </summary>
	public class Location
	{
		/// <summary>
		/// Gets or sets the primary key of this <see cref="Location"/>.
		/// </summary>
		[JsonIgnore]
		public int Id { get; set; }
		/// <summary>
		/// Gets or sets the address of this <see cref="Location"/>.
		/// </summary>
		[Required]
		public string Address { get; set; }
		/// <summary>
		/// Gets or sets the city of this <see cref="Location"/>.
		/// </summary>
		[Required]
		public string City { get; set; }
		/// <summary>
		/// Gets or sets the zip code of this <see cref="Location"/>.
		/// </summary>
		[Required]
		[Display(Name = "Zip code")]
		public string ZipCode { get; set; }
		/// <summary>
		/// Gets or sets the location of this <see cref="Location"/>.
		/// </summary>
		[Required]
		[Display(Name = "Country")]
		public string CountryCode { get; set; }

	}
}
