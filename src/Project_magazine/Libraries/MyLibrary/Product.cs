using CounterLibrary;
using System.ComponentModel.DataAnnotations;
namespace MyLibrary;
abstract public class Product
	{
	public Product() { }//  явный конструктор без аргументов насколько помню нужен для грамотной работы миграции.  функционала у него нет но нужен для того чтобы база данных не ругалась
	public Product(string discriminator, int prise ,string name, int weight, int volume, int id, string typeOfProduct)
	{
		Discriminator = discriminator;
		Prise = prise;
		Name = name;
		Weight = weight;
		Volume = volume;
		ProductID = id;
		TypeOfProduct = typeOfProduct ?? throw new ArgumentNullException(nameof(typeOfProduct)); 
	}

	public string Discriminator { get; set; } 
	public int Prise { get; set; }
	public string TypeOfProduct{ get; set; }
	public string Name { get; set; }
	public int Weight { get; set; }
	public int Volume { get; set; }
	[Key]
	public int ProductID { get; set; }
}

	public class MeatProduct : Product
	{
		public MeatProduct() : base("Meat Product", 0, "", 0, 0, 0, "Meat Product")
		{
			Nature = false;
		}
	public MeatProduct( int prise, string name, int weight, int volume, int id, bool nature)
			: base("Meat Product", prise ,name, weight, volume, id, "Meat Product")
		{
			Nature = nature;
		}
		//public MeatProduct() { }

		public bool? Nature { get; set; }
	}

	public class MilkProduct : Product
	{
		public MilkProduct() : base("Milk Product", 0, "", 0, 0, 0, "Milk Product")
		{
			ProductionDay = default;
		}
	public MilkProduct(int prise, string name, int weight, int volume, int id, string productionDay)
			: base("Milk Product", prise, name, weight, volume, id, "Milk Product")
		{
			ProductionDay = productionDay;
		}

	public string? ProductionDay { get; set; }
}

	public class SweetProduct : Product
	{
		public SweetProduct() : base("Sweet Product" ,0, "", 0, 0, 0, "Sweet Product")
		{
			AmountOfSugar = 0;
		}
	public SweetProduct( int prise, string name, int weight, int volume, int id, int amountOfSugar)
			: base("Sweet Product", prise ,name, weight, volume, id, "Sweet Product")
		{
			AmountOfSugar = amountOfSugar;
		}
		public int? AmountOfSugar { get; set; }
}

