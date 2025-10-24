namespace API_bacup_server
{
	public class Config
	{
		public class AppConfig
		{
			public DatabaseConfig DatabaseConfig { get; set; }
			public BackupConfig BackupConfig { get; set; }
			public CmdConsoleConfig CmdConsoleConfig { get; set; }
		}

		public class DatabaseConfig
		{
			public string User { get; set; }
			public string Password { get; set; }
			public string Host { get; set; }
			public string DatabaseName { get; set; }
		}

		public class BackupConfig
		{
			public string FullBackupPath { get; set; }
			public string IncrementalBackupPath { get; set; }
			public int RetentionDays { get; set; }
		}

		public class CmdConsoleConfig
		{
			public string DumpCommand { get; set; }
			public string RestoreCommand { get; set; }
		}
	}
}
