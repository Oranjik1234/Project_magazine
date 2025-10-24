using System.ComponentModel.DataAnnotations;

namespace CounterLibrary;
public class ProductCounter
{
	[Key]
	public int ProductID { get; set; }
	public int Count { get; set; }

	public ProductCounter()
	{
	}
	public ProductCounter(int productId)
	{
		ProductID = productId;
		Count = 0;
	}

	public void Increment()
	{
		Count++;
	}
}