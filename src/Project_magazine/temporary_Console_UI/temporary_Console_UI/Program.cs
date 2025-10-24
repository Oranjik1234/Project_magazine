using CounterLibrary;
using MyLibrary;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
	static bool isRunning = true;

	static async Task Main(string[] args)
	{
		while (isRunning)
		{
			Console.WriteLine("\nВыберите действие:");
			Console.WriteLine("1. Добавить новый продукт (POST)");
			Console.WriteLine("2. Обновить продукт (PUT)");
			Console.WriteLine("3. Удалить продукт (DELETE)");
			Console.WriteLine("4. Получить все продукты (GET)");
			Console.WriteLine("5. Получить продукт по ID (GET)");
			Console.WriteLine("6. Получить продукт по имени (GET)");
			Console.WriteLine("7. Запросить все каунтеры (GET)");
			Console.WriteLine("8. Запросить каунтер по ID (GET)");
			Console.WriteLine("9. Запросить общий доход (GET)");
			Console.WriteLine("10. Запросить доход по категории (GET)");
			Console.WriteLine("11. Запросить доход по каунтеру (GET)");
			Console.WriteLine("12. Завершить работу");
            Console.WriteLine("13 Запрос рестарта");
            Console.WriteLine("14 Запрос конфиг файла приложения");
            Console.WriteLine("15 Обновление настроек приложения");
			Console.WriteLine("16. Отправить запрос на восстановление (POST)");

			Console.Write("Введите номер действия: ");

			string? choice = Console.ReadLine();

			switch (choice)
			{
				case "1":
					await AddProductAsync();
					break;
				case "2":
					await UpdateProductAsync();
					break;
				case "3":
					await DeleteProductAsync();
					break;
				case "4":
					await GetAllProductsAsync();
					break;
				case "5":
					await GetProductByIdAsync();
					break;
				case "6":
					await GetProductByNameAsync();
					break;
				case "8":
					await GetAllCountersAsync();
					break;
				case "9":
					await GetTotalRevenueAsync();
					break;
				case "10":
					await GetRevenueByCategoryAsync();
					break;
				case "11":
					await GetRevenueByCounterAsync();
					break;
				case "12":
					isRunning = false;
					Console.WriteLine("Завершение работы...");
					break;
				case "13":
						await RestartAsync();
					break;
				case "14":
						await GetConfigAsync();
					break;
				case "15":
						await UpdateConfigAsync();
					break;
				case "16":
					await SendRestoreRequestAsync();
					break;
				default:
					Console.WriteLine("Неверный ввод. Попробуйте снова.");
					break;
			}
		}
	}


	static async Task SendRestoreRequestAsync()
	{
		using var httpClient = new HttpClient();

		Console.WriteLine("Введите адрес бэкап-сервера:");
		string? backupServerUrl = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(backupServerUrl))
		{
			Console.WriteLine("Неверный адрес бэкап-сервера.");
			return;
		}

		Console.WriteLine("Введите адрес для восстановления:");
		string? restoreTargetUrl = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(restoreTargetUrl))
		{
			Console.WriteLine("Неверный адрес для восстановления.");
			return;
		}

		try
		{
			var content = new FormUrlEncodedContent(new[]
			{
			new KeyValuePair<string, string>("TargetUrl", restoreTargetUrl)
		});

			var response = await httpClient.PostAsync($"{backupServerUrl}/upload-full-dump", content);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine("Запрос на восстановление успешно отправлен.");
			}
			else
			{
				Console.WriteLine($"Ошибка при отправке запроса: {response.StatusCode}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка: {ex.Message}");
		}
	}


	static async Task AddProductAsync()
	{
		using var httpClient = new HttpClient();
		try
		{
			Console.WriteLine("Выберите категорию продукта:");
			Console.WriteLine("1. Мясной продукт");
			Console.WriteLine("2. Молочный продукт");
			Console.WriteLine("3. Сладкий продукт");
			Console.Write("Введите номер категории: ");
			string? categoryChoice = Console.ReadLine();

			if (!int.TryParse(categoryChoice, out int category) || category < 1 || category > 3)
			{
				Console.WriteLine("Неверный выбор категории. Попробуйте снова.");
				return;
			}

			Console.Write("Цена продукта: ");
			if (!int.TryParse(Console.ReadLine(), out int price) || price <= 0)
			{
				Console.WriteLine("Некорректный ввод цены. Попробуйте снова.");
				return;
			}

			Console.Write("Имя продукта: ");
			string name = Console.ReadLine();

			Console.Write("Вес продукта (г): ");
			if (!int.TryParse(Console.ReadLine(), out int weight) || weight <= 0)
			{
				Console.WriteLine("Некорректный ввод веса. Попробуйте снова.");
				return;
			}

			Console.Write("Объём продукта (мл): ");
			if (!int.TryParse(Console.ReadLine(), out int volume) || volume <= 0)
			{
				Console.WriteLine("Некорректный ввод объёма. Попробуйте снова.");
				return;
			}

			Console.Write("ID продукта: ");
			if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
			{
				Console.WriteLine("Некорректный ввод ID. Попробуйте снова.");
				return;
			}

			Product newProduct;
			switch (category)
			{
				case 1:
					Console.Write("Природный продукт? (true/false): ");
					if (!bool.TryParse(Console.ReadLine(), out bool nature))
					{
						Console.WriteLine("Некорректный ввод значения. Попробуйте снова.");
						return;
					}
					newProduct = new MeatProduct(price, name, weight, volume, id, nature)
					{
						Discriminator = "Meat Product"
					};
					break;

				case 2:
					Console.Write("День производства (например, Monday): ");
					if (!Enum.TryParse(Console.ReadLine(), true, out DayOfWeek productionDay))
					{
						Console.WriteLine("Некорректный ввод дня недели. Попробуйте снова.");
						return;
					}

					newProduct = new MilkProduct(price, name, weight, volume, id, productionDay.ToString())
					{
						Discriminator = "Milk Product"
					};
					break;

				case 3:
					Console.Write("Количество сахара (г): ");
					if (!int.TryParse(Console.ReadLine(), out int amountOfSugar) || amountOfSugar < 0)
					{
						Console.WriteLine("Некорректный ввод количества сахара. Попробуйте снова.");
						return;
					}
					newProduct = new SweetProduct(price, name, weight, volume, id, amountOfSugar)
					{
						Discriminator = "Sweet Product"
					};
					break;

				default:
					Console.WriteLine("Неизвестная категория.");
					return;
			}

			string apiUrl = "https://localhost:5201/SalesDataStorageServer/AddProduct";
			var json = JsonConvert.SerializeObject(newProduct);
			Console.WriteLine(json);
			var response = await httpClient.PostAsJsonAsync(apiUrl, newProduct);

			if (response.IsSuccessStatusCode)
			{
				var createdProduct = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Продукт успешно добавлен: {createdProduct}");
			}
			else
			{
				Console.WriteLine($"Ошибка при добавлении продукта: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task UpdateProductAsync()
	{
		using var httpClient = new HttpClient();

		try
		{
			Console.Write("Введите ID продукта для обновления: ");
			if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
			{
				Console.WriteLine("Некорректный ввод ID. Попробуйте снова.");
				return;
			}

			Console.WriteLine("Выберите категорию продукта для обновления:");
			Console.WriteLine("1. Мясной продукт");
			Console.WriteLine("2. Молочный продукт");
			Console.WriteLine("3. Сладкий продукт");
			Console.Write("Введите номер категории: ");
			string? categoryChoice = Console.ReadLine();

			if (!int.TryParse(categoryChoice, out int category) || category < 1 || category > 3)
			{
				Console.WriteLine("Неверный выбор категории. Попробуйте снова.");
				return;
			}

			Console.Write("Введите новую цену продукта: ");
			if (!int.TryParse(Console.ReadLine(), out int price) || price <= 0)
			{
				Console.WriteLine("Некорректный ввод цены. Попробуйте снова.");
				return;
			}

			Console.Write("Введите новое имя продукта: ");
			string name = Console.ReadLine();

			Console.Write("Введите новый вес продукта (г): ");
			if (!int.TryParse(Console.ReadLine(), out int weight) || weight <= 0)
			{
				Console.WriteLine("Некорректный ввод веса. Попробуйте снова.");
				return;
			}

			Console.Write("Введите новый объём продукта (мл): ");
			if (!int.TryParse(Console.ReadLine(), out int volume) || volume <= 0)
			{
				Console.WriteLine("Некорректный ввод объёма. Попробуйте снова.");
				return;
			}

			Product updatedProduct;
			switch (category)
			{
				case 1:
					Console.Write("Природный продукт? (true/false): ");
					if (!bool.TryParse(Console.ReadLine(), out bool nature))
					{
						Console.WriteLine("Некорректный ввод значения. Попробуйте снова.");
						return;
					}
					updatedProduct = new MeatProduct(price, name, weight, volume, id, nature)
					{
						Discriminator = "Meat Product"
					};
					break;

				case 2:
					Console.Write("День производства (например, Monday): ");
					if (!Enum.TryParse(Console.ReadLine(), true, out DayOfWeek productionDay))
					{
						Console.WriteLine("Некорректный ввод дня недели. Попробуйте снова.");
						return;
					}

					updatedProduct = new MilkProduct(price, name, weight, volume, id, productionDay.ToString())
					{
						Discriminator = "Milk Product"
					};
					break;

				case 3:
					Console.Write("Новое количество сахара (г): ");
					if (!int.TryParse(Console.ReadLine(), out int amountOfSugar) || amountOfSugar < 0)
					{
						Console.WriteLine("Некорректный ввод количества сахара. Попробуйте снова.");
						return;
					}
					updatedProduct = new SweetProduct(price, name, weight, volume, id, amountOfSugar)
					{
						Discriminator = "Sweet Product"
					};
					break;

				default:
					Console.WriteLine("Неизвестная категория.");
					return;
			}

			string apiUrl = $"http://localhost:5201/UpdateProduct/{id}";
			var response = await httpClient.PutAsJsonAsync(apiUrl, updatedProduct);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine($"Продукт с ID {id} успешно обновлён.");
			}
			else
			{
				Console.WriteLine($"Ошибка при обновлении продукта: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task DeleteProductAsync()
	{
		using var httpClient = new HttpClient();

		try
		{
			Console.Write("Введите ID продукта для удаления: ");
			if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
			{
				Console.WriteLine("Некорректный ввод ID. Попробуйте снова.");
				return;
			}

			string apiUrl = "https://localhost:5201/SalesDataStorageServer/DeleteProduct/{id}";
			var response = await httpClient.DeleteAsync(apiUrl);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine($"Продукт с ID {id} успешно удалён.");
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine($"Продукт с ID {id} не найден.");
			}
			else
			{
				Console.WriteLine($"Ошибка при удалении продукта: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task GetAllProductsAsync()
	{
		using var httpClient = new HttpClient();

		try
		{
			string apiUrl = "http://localhost:5201/GetAllProducts";

			var response = await httpClient.GetAsync(apiUrl);

			if (response.IsSuccessStatusCode)
			{
				var products = await response.Content.ReadFromJsonAsync<List<Product>>();
				if (products != null && products.Any())
				{
					Console.WriteLine("Список продуктов:");
					foreach (var product in products)
					{
						Console.WriteLine($"ID: {product.ProductID}, Имя: {product.Name}, Цена: {product.Prise}, " +
										  $"Вес: {product.Weight} г, Объём: {product.Volume} мл, Категория: {product.TypeOfProduct}");
					}
				}
				else
				{
					Console.WriteLine("Продукты не найдены.");
				}
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine("Продукты не найдены на сервере.");
			}
			else
			{
				Console.WriteLine($"Ошибка при получении продуктов: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task GetProductByIdAsync()
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("http://localhost:5201/")
		};

		Console.Write("Введите ID продукта для поиска: ");
		if (!int.TryParse(Console.ReadLine(), out int id) || id <= 0)
		{
			Console.WriteLine("Некорректный ввод ID. Попробуйте снова.");
			return;
		}

		var response = await httpClient.GetAsync($"GetProductNameById/{id}");

		if (response.IsSuccessStatusCode)
		{
			var productName = await response.Content.ReadAsStringAsync();
			Console.WriteLine($"Продукт найден: {productName}");
		}
		else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			Console.WriteLine($"Продукт с ID {id} не найден.");
		}
		else
		{
			Console.WriteLine($"Ошибка при запросе: {response.StatusCode} - {response.ReasonPhrase}");
		}
	}

	static async Task GetProductByNameAsync()
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("http://localhost:5201/")
		};

		Console.Write("Введите имя или ID продукта: ");
		string input = Console.ReadLine() ?? string.Empty;

		if (string.IsNullOrWhiteSpace(input))
		{
			Console.WriteLine("Некорректный ввод. Попробуйте снова.");
			return;
		}

		var response = await httpClient.GetAsync($"GetProductInfoByIdOrName/{input}");

		if (response.IsSuccessStatusCode)
		{
			var product = await response.Content.ReadFromJsonAsync<Product>();
			if (product != null)
			{
				Console.WriteLine("Информация о продукте:");

				Console.WriteLine($"ID: {product.ProductID}");
				Console.WriteLine($"Имя: {product.Name ?? "нет данных"}");
				Console.WriteLine($"Цена: {product.Prise}");
				Console.WriteLine($"Вес: {product.Weight} г");
				Console.WriteLine($"Объём: {product.Volume} мл");
				Console.WriteLine($"Категория: {product.TypeOfProduct ?? "нет данных"}");

				if (product is MeatProduct meatProduct)
				{
					Console.WriteLine($"Натуральность: {meatProduct.Nature}");
				}
				else if (product is MilkProduct milkProduct)
				{
					Console.WriteLine($"День производства: {milkProduct.ProductionDay}");
				}
				else if (product is SweetProduct sweetProduct)
				{
					Console.WriteLine($"Количество сахара: {sweetProduct.AmountOfSugar} г");
				}
			}
			else
			{
				Console.WriteLine("Продукт не найден.");
			}
		}
		else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
		{
			Console.WriteLine($"Продукт с именем или ID '{input}' не найден.");
		}
		else
		{
			Console.WriteLine($"Ошибка при запросе: {response.StatusCode} - {response.ReasonPhrase}");
		}
	}

	static async Task GetAllCountersAsync()
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("http://localhost:5201/")
		};

		try
		{
			var response = await httpClient.GetAsync("GetCountersSummary");

			if (response.IsSuccessStatusCode)
			{
				var counters = await response.Content.ReadFromJsonAsync<IEnumerable<ProductCounter>>();

				if (counters != null && counters.Any())
				{
					Console.WriteLine("Сводка о продажах:");
					foreach (var counter in counters)
					{
						Console.WriteLine($"ID Продукта: {counter.ProductID}");
						//Console.WriteLine($"Имя Продукта: {counter.ProductName ?? "нет данных"}");
						Console.WriteLine($"Количество Продаж: {counter.Count}");
						Console.WriteLine(new string('-', 20));
					}
				}
				else
				{
					Console.WriteLine("Данные о продажах отсутствуют.");
				}
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine("Сводка о продажах не найдена.");
			}
			else
			{
				Console.WriteLine($"Ошибка при запросе: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task GetTotalRevenueAsync()
	{
		using var httpClient = new HttpClient
		{
			BaseAddress = new Uri("http://localhost:5201/")
		};

		try
		{
			var response = await httpClient.GetAsync("GetTotalRevenue");

			if (response.IsSuccessStatusCode)
			{
				var totalRevenue = await response.Content.ReadFromJsonAsync<decimal>();

				if (totalRevenue != default)
				{
					Console.WriteLine($"Общая выручка: {totalRevenue}");
				}
				else
				{
					Console.WriteLine("Данные о выручке отсутствуют.");
				}
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine("Данные о выручке не найдены.");
			}
			else
			{
				Console.WriteLine($"Ошибка при запросе: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
		}
	}

	static async Task GetRevenueByCategoryAsync()
	{
		Console.Write("Введите категорию продукта: ");
		string? category = Console.ReadLine();

		if (string.IsNullOrEmpty(category))
		{
			Console.WriteLine("Категория не может быть пустой.");
			return;
		}

		using var httpClient = new HttpClient
		{
			//BaseAddress = new Uri() написать что надо
		};

		try
		{
			HttpResponseMessage response = await httpClient.GetAsync($"GetRevenueByCategory/{category}");

			if (response.IsSuccessStatusCode)
			{
				decimal revenue = await response.Content.ReadFromJsonAsync<decimal>();
				Console.WriteLine($"Доход от продаж в категории '{category}': {revenue:C}");
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine($"Нет данных по категории: '{category}'.");
			}
			else
			{
				Console.WriteLine($"Ошибка при запросе: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Произошла ошибка: {ex.Message}");
		}
	}

	static async Task GetRevenueByCounterAsync()
	{
		Console.Write("Введите ID каунтера: ");
		if (!int.TryParse(Console.ReadLine(), out int counterId))
		{
			Console.WriteLine("ID должен быть числом.");
			return;
		}

		using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5201/") };
		try
		{
			var response = await httpClient.GetAsync($"GetRevenueByCounter/{counterId}");

			if (response.IsSuccessStatusCode)
			{
				var revenue = await response.Content.ReadAsStringAsync();
				Console.WriteLine($"Доход для каунтера {counterId}: {revenue}");
			}
			else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
			{
				Console.WriteLine("Каунтер с данным ID не найден.");
			}
			else
			{
				Console.WriteLine($"Ошибка: {response.StatusCode} - {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
		}
	}

	static async Task RestartAsync()
	{
		Console.WriteLine("Введите адрес для перезагрузки (например, http://localhost:5201/restart):");
		string? address = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(address))
		{
			Console.WriteLine("Адрес не может быть пустым. Попробуйте снова.");
			return;
		}

		using var httpClient = new HttpClient();
		try
		{
			var response = await httpClient.PostAsync(address, null);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine("Программа успешно перезагружена.");
			}
			else
			{
				Console.WriteLine($"Ошибка при выполнении перезагрузки: {response.StatusCode}, {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
		}
	}

	static async Task GetConfigAsync()
	{
		Console.WriteLine("Введите адрес для получения конфигурации (например, http://localhost:5201/getconfig):");
		string? address = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(address))
		{
			Console.WriteLine("Адрес не может быть пустым. Попробуйте снова.");
			return;
		}

		using var httpClient = new HttpClient();
		try
		{
			var response = await httpClient.GetAsync(address);

			if (response.IsSuccessStatusCode)
			{
				var json = await response.Content.ReadAsStringAsync();
				Console.WriteLine("Текущая конфигурация:");
				Console.WriteLine(json);
			}
			else
			{
				Console.WriteLine($"Ошибка при получении конфигурации: {response.StatusCode}, {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
		}
	}

	static async Task UpdateConfigAsync()
	{
		Console.WriteLine("Введите адрес для обновления конфигурации (например, http://localhost:5201/updateconfig):");
		string? address = Console.ReadLine();

		if (string.IsNullOrWhiteSpace(address))
		{
			Console.WriteLine("Адрес не может быть пустым. Попробуйте снова.");
			return;
		}

		Console.WriteLine("Введите название приложения:");
		string? appName = Console.ReadLine();

		Console.WriteLine("Введите порт приложения:");
		if (!int.TryParse(Console.ReadLine(), out int appPort) || appPort <= 0)
		{
			Console.WriteLine("Некорректный порт. Попробуйте снова.");
			return;
		}

		var targetAddresses = new List<string>();
		Console.WriteLine("Введите адреса отправки (введите 'стоп' для завершения ввода):");

		while (true)
		{
			Console.Write("Адрес: ");
			string? input = Console.ReadLine();

			if (input == "stop" || input == "")
			{
				break;
			}

			if (Uri.IsWellFormedUriString(input, UriKind.Absolute))
			{
				targetAddresses.Add(input);
			}
			else
			{
				Console.WriteLine("Некорректный адрес. Попробуйте снова.");
			}
		}

		var config = new
		{
			AppName = appName,
			AppPort = appPort,
			TargetAddresses = targetAddresses
		};

		using var httpClient = new HttpClient();
		try
		{
			var json = System.Text.Json.JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await httpClient.PostAsync(address, content);

			if (response.IsSuccessStatusCode)
			{
				Console.WriteLine("Конфигурация успешно обновлена.");
			}
			else
			{
				Console.WriteLine($"Ошибка при обновлении конфигурации: {response.StatusCode}, {response.ReasonPhrase}");
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine($"Ошибка при выполнении запроса: {ex.Message}");
		}
	}
}

