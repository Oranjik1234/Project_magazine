using ConfigLibrary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using static ConfigLibrary.ConfigManager;

namespace Cart_Tranzit_API.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ApiTransitController : ControllerBase
	{

		private readonly ILogger<ApiTransitController> _logger;
		private readonly IConfiguration _ñonfiguration;
		private readonly ConfigManager _configManager;

		public ApiTransitController(ILogger<ApiTransitController> logger, IConfiguration configuration, ConfigManager configManager)
		{
			_logger = logger;
			_ñonfiguration = configuration;
			string FilePath = _ñonfiguration["LocalStoragePath"];
			_configManager = configManager;
		}
	
		[HttpGet("GetAppConfig")]
		public IActionResult GetAppConfig()
		{
			var config = _configManager.LoadConfig<AppConfig>();
			return Ok(config);
		}
		[HttpPost]
		public IActionResult Restart()
		{
			try
			{
				_configManager.Restart();
			}
			catch (Exception)
			{
				throw new Exception("505 Restart faled");
			}
			return Ok("Programm restarted");

		}
		[HttpPost]
		public IActionResult UpdateAppConfig([FromBody] AppConfig NewConfigData)
		{
			if (NewConfigData == null)
			{
				return BadRequest("Invalid configuration data.");
			}
			try
			{
				_configManager.SaveConfig(NewConfigData);
				Console.WriteLine("Configuration updated successfully. Restarting...");
				_configManager.Restart();
			}
			catch (Exception ex)
			{
				return StatusCode(500, $"Error updating configuration: {ex.Message}");
			}

			return Ok("Configuration updated and application restarted.");
		}

		[HttpPost("SendJsonFile")]
		public async Task<IActionResult> SendJsonFile() 
		{
			try
			{
				string directoryPath = _ñonfiguration["LocalStoragePath"];

				var files = Directory.GetFiles(directoryPath, "*.json");
				if (files.Length == 0)
				{
					return NotFound("No files to upload.");
				}

				string filePath = files[0];
				string fileName = Path.GetFileName(filePath);

				var fileContent = await System.IO.File.ReadAllTextAsync(filePath);	

				using var client = new HttpClient();
				var content = new StringContent(fileContent, System.Text.Encoding.UTF8, "application/json");

				var targetAdresses = _ñonfiguration.GetSection("AppConfig: TargetAddresses").Get<List<string>>();
				string serverAddress = targetAdresses.First();

				var response = await client.PostAsync($"{serverAddress}/SalesDataStorageServer/ProcessLogFile", content);

				if (response.IsSuccessStatusCode)
				{
					System.IO.File.Delete(filePath);
					return Ok($"File {fileName} uploaded successfully.");
				}

				return StatusCode((int)response.StatusCode, $"Failed to upload file {fileName}: {response.ReasonPhrase}");
			}
			catch (Exception ex)
			{
				_logger.LogError($"Error during file upload: {ex.Message}");
				return StatusCode(500, "Internal server error.");
			}
		}
	}
}
