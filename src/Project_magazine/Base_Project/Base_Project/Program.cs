using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using static Base_Project.Program;
using System.IO;
using System.Threading;
using MyLibrary;


namespace Base_Project
{
	internal class Program
	{
		public class Сustomer
		{
			public Сustomer()
			{
				ProductsCart = new List<Product>();
				СallMethodGetProduct();
			}
			Random R = new Random();
			private static readonly List<Product> AvailableProducts = new List<Product>
			{
				new MeatProduct("Beef", 400, 250, 3374, true),
				new MilkProduct("Milk", 1000, 1000, 6381, DayOfWeek.Monday),
				new SweetProduct("Chocolate", 200, 100, 7732, 45)
			};
			public List<Product> ProductsCart { get; }

			public void СallMethodGetProduct()
			{
				int randomInt = R.Next(3, 7);

				for (int i = 0; i <= randomInt; i++)
				{
					ProductsCart.Add(GetProduct());
				}
			}
			private Product GetProduct()
			{
				int randomInt = R.Next(AvailableProducts.Count);

				return AvailableProducts[randomInt];
			}
		}

		public class Node
		{
			public Node(Сustomer savedCustomer)
			{
				this.savedCustomer = savedCustomer;
				Next = null;
			}
			public Сustomer savedCustomer { get; }
			public Node Next { get; set; }
		}

		public class Queue
		{
			public Node Head { get; set; }
			public Node Tail { get; set; }

			public bool IsEmpty => Head == null;
			public void Enqueue(Сustomer customer)
			{
				Node newNode = new Node(customer);

				if (IsEmpty)
				{
					Head = newNode;
					Tail = newNode;
				}
				else
				{
					Tail.Next = newNode;
					Tail = newNode;
				}
			}
			public Сustomer Dequeue()
			{
				if (IsEmpty)
				{
					Console.WriteLine("Queue is null");
				}

				Сustomer customer = Head.savedCustomer;
				Head = Head.Next;

				if (Head == null)
				{
					Tail = null;
				}
				return customer;
			}
		}

		public class QueueCommand
		{
			public Queue ReferenseQueue { get; }
			public QueueCommand(Queue queue)
			{
				this.queue = queue;
				ReferenseQueue = queue;
			}

			Queue queue = new Queue();

			public List<Product> GetCartFromCustomer()
			{
				if (queue.IsEmpty) throw new Exception("Queue is null");

				else return queue.Tail.savedCustomer.ProductsCart;
			}
			public void DeliteFromQueue()
			{
				if (queue.IsEmpty) throw new Exception("Queue is null");

				else ReferenseQueue.Dequeue();
			}
		}

		public class Cashbox
		{
			public int[] SelledProductIDs = new int[51];
			QueueCommand RefQueueCommand { get; }

			private string fileName = $"Sell_Log{DateTime.UtcNow:yyyyMMdd_HHmmss_ff}.Json";

			private string filePath = "C:\\Users\\alex\\Downloads\\Test_Program_Log";//Пиздец -  абсолютные адреса уникальные для моего компьютера во время теста (убрать и заменить на условные)


			public int countOfIDs{  get; set; }


			public Cashbox(QueueCommand queueCommand)
			{
				RefQueueCommand = queueCommand;
			}

			List<Product> CopyOfCustomerPrоducts = new List<Product>();

			public void SaveLogToFile()
			{
				string jsonString = JsonConvert.SerializeObject(SelledProductIDs);
				File.WriteAllText(Path.Combine(filePath, fileName), jsonString);
			}
			public void FiilCopeList()
			{
				try
				{
					CopyOfCustomerPrоducts = GetCart(RefQueueCommand);
					SkipCastomer();
				}
				catch (InvalidOperationException)
				{
					Console.WriteLine("Queue is empty");
					CopyOfCustomerPrоducts = new List<Product>();
				}

			}
			public void LoggingProductID()
			{
				int shortTimeInt = 0;
				for (int i = 0; i < CopyOfCustomerPrоducts.Count; i++)
				{
					if (countOfIDs + i >= SelledProductIDs.Length - 1)
					{
						SaveLogToFile();
                        Console.WriteLine("Данные сохранены в файл");
                        Array.Clear(SelledProductIDs, 0, SelledProductIDs.Length);
						fileName = $"Sell_Log{DateTime.UtcNow:yyyyMMdd_HHmmss_ff}.Json";
						countOfIDs = 0;
						i = 0;
					}
					shortTimeInt = CopyOfCustomerPrоducts[i].ID;
					SelledProductIDs[countOfIDs] = shortTimeInt;
					countOfIDs++;
				}
				countOfIDs = countOfIDs + CopyOfCustomerPrоducts.Count;
			}
			private List<Product> GetCart(QueueCommand queueController)
			{
				return RefQueueCommand.GetCartFromCustomer();
			}
			private void SkipCastomer()
			{
				try
				{
					RefQueueCommand.DeliteFromQueue();
				}
				catch (InvalidOperationException)
				{
					Console.WriteLine("Queue is empty");
				}
			}
		}


		static void Main(string[] args)
		{
			Queue customerQueue = new Queue();
			QueueCommand queueCommand = new QueueCommand(customerQueue);
			Cashbox cashbox = new Cashbox(queueCommand);

			while (true)
			{
				Console.WriteLine("Добавление нового покупателя...");
				Сustomer customer = new Сustomer();
				customerQueue.Enqueue(customer);

				Console.WriteLine("Обработка очереди...");
				cashbox.FiilCopeList();
				cashbox.LoggingProductID();

				Console.WriteLine("Покупатель обработан. Ожидание перед следующим.");

				Thread.Sleep(500);

				if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
				{
					cashbox.SaveLogToFile();
					Console.WriteLine("Завершение работы программы...");
					break;
				}
			}
		}
	}
}
