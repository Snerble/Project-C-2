using System;
using System.ComponentModel.DataAnnotations;

namespace QARS.Data.Models
{
	/// <summary>
	/// Represents a physical car.
	/// </summary>
	public class Car
	{
		/// <summary>
		/// Gets or sets the primary key of this <see cref="Car"/>.
		/// </summary>
		public int? Id { get; set; }
		/// <summary>
		/// Gets or sets the model of this <see cref="Car"/>.
		/// </summary>
		public CarModel Model { get; set; }

		/// <summary>
		/// Gets or sets the license plate of this <see cref="Car"/>.
		/// </summary>
		[Required, StringLength(16)]
		public string LicensePlate { get; set; }

		/// <summary>
		/// Gets or sets the original <see cref="Models.Store"/> of this <see cref="Car"/>.
		/// </summary>
		public Store Store { get; set; }
		/// <summary>
		/// Gets or sets the current store location of this <see cref="Car"/>.
		/// </summary>
		public Store Location { get; set; }

		/// <summary>
		/// Gets or sets whether this <see cref="Car"/> is available for reservations.
		/// </summary>
		public bool Available { get; set; }

		/// <summary>
		/// Returns the distance that has been travelled with this <see cref="Car"/>,
		/// measured in kilometers.
		/// </summary>
		[Range(0, double.MaxValue)]
		public decimal Mileage { get; set; }

		/// <summary>
		/// Marks this car as available.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when this <see cref="Car"/> is already available.</exception>
		public void MakeAvailable()
		{
			lock (this)
			{
				if (Available)
					throw new InvalidOperationException("This car is already available.");
				Available = true; 
			}
		}
		/// <summary>
		/// Marks this <see cref="Car"/> as reserved.
		/// </summary>
		/// <exception cref="InvalidOperationException">Thrown when this <see cref="Car"/> is already reserved.</exception>
		public void Reserve()
		{
			lock (this)
			{
				if (!Available)
					throw new InvalidOperationException("This car is already reserved");
				Available = false; 
			}
		}

		public override string ToString() => Utils.GetDetailedString(this);
	}
}
