using System.Diagnostics;


namespace API_bacup_server
{
	public class DumpControl
	{
		public string DatabaseUser { get; set; }
		public string DatabasePassword { get; set; }
		public string DatabaseHost { get; set; }
		public string DatabaseName { get; set; }
		public string BackupDirectory { get; set; }

		public string pathToDump = "";//  пиздец - установить адрес условный моего репозитория с бэкапом полный адрес я буду писать позже

		public DumpControl(IConfiguration configuration)	
		{
			DatabaseUser = configuration["BackupSettings:DatabaseUser"];
			DatabasePassword = configuration["BackupSettings:DatabasePassword"];
			DatabaseHost = configuration["BackupSettings:DatabaseHost"];
			DatabaseName = configuration["BackupSettings:DatabaseName"];
			BackupDirectory = configuration["BackupSettings:BackupDirectory"];
		}
		public byte[] PerformFullBackup()
		{
			string filePath = Path.Combine(BackupDirectory, "FullBackup", "backup.sql");
			string command = $"mysqldump --user={DatabaseUser} --password={DatabasePassword} --host={DatabaseHost} --databases {DatabaseName} > \"{filePath}\"";

			try
			{
				using (var process = new Process())
				{
					process.StartInfo = new ProcessStartInfo
					{
						FileName = "cmd.exe",
						Arguments = $"/c {command}",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					};

					process.Start();

					string error = process.StandardError.ReadToEnd();
					process.WaitForExit();

					if (process.ExitCode != 0)
					{
						Console.WriteLine($"Ошибка при создании бэкапа: {error}");
						throw new Exception("Ошибка выполнения команды.");
					}

					Console.WriteLine("Бэкап успешно создан.");
				}

				byte[] fileContent = File.ReadAllBytes(filePath);

				return fileContent;
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");

				throw;
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}
		public void RestoreFullBackup(byte[] dumpData)
		{
			string filePath = Path.Combine(BackupDirectory, "FullBackup", "restore.sql");
			string command = $"mysql --user={DatabaseUser} --password={DatabasePassword} --host={DatabaseHost} {DatabaseName} < \"{filePath}\"";

			try
			{
				if (dumpData == null || dumpData.Length == 0)
				{
					throw new ArgumentException("Данные дампа отсутствуют или пусты.");
				}

				using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
				{
					stream.Write(dumpData, 0, dumpData.Length);
				}

				using (var process = new Process())
				{
					process.StartInfo = new ProcessStartInfo
					{
						FileName = "cmd.exe",
						Arguments = $"/c {command}",
						RedirectStandardOutput = true,
						RedirectStandardError = true,
						UseShellExecute = false,
						CreateNoWindow = true
					};

					process.Start();

					string error = process.StandardError.ReadToEnd();
					process.WaitForExit();

					if (process.ExitCode != 0)
					{
						Console.WriteLine($"Ошибка при восстановлении базы данных: {error}");
						throw new Exception("Ошибка выполнения команды.");
					}

					Console.WriteLine("База данных успешно восстановлена.");
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Ошибка: {ex.Message}");
				throw;
			}
			finally
			{
				if (File.Exists(filePath))
				{
					File.Delete(filePath);
				}
			}
		}
	}
}
