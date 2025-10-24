
using System.ComponentModel.DataAnnotations;

namespace SalesClassLibrary;

	public class Sale
	{
		[Key]
		public int SaleID { get; set; }
		public int ProductID { get; set; }
		public DateTime SaleDate { get; set; }
	}
