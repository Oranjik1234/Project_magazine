using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using static API_bacup_server.Config;

namespace API_bacup_server
{
	[Route("api/[controller]")]	
	[ApiController]
	public class BacupController : ControllerBase
	{
		private readonly ILogger<BacupController> _ILogger;
		private readonly DumpControl _DumpControl;
		private readonly AppConfig _config;
		public BacupController(ILogger<BacupController> ILogger, DumpControl DumpControl, IOptions<AppConfig> config)
		{
			_DumpControl = DumpControl;
			_ILogger = ILogger;
			_config = config.Value;
		}

		[HttpPost("send-full-dump")]
		public async Task<IActionResult> SendFullDump([FromQuery] string targetUrl)
		{
			try
			{
				byte[] backupData = _DumpControl.PerformFullBackup();

				using var httpClient = new HttpClient();
				using var content = new MultipartFormDataContent
				{
					{ new ByteArrayContent(backupData), "file", "backup.sql" }
				};


				HttpResponseMessage response = await httpClient.PostAsync(targetUrl, content);

				if (response.IsSuccessStatusCode)
				{
					return Ok(new { message = "Файл дампа успешно отправлен." });
				}
				else
				{
					_ILogger.LogError($"Ошибка при отправке файла на стороне получателя: {response.ReasonPhrase}");
					return StatusCode(500, new { error = "Ошибка при отправке файла." });
				}
			}
			catch (Exception ex)
			{
				_ILogger.LogError($"Ошибка при отправке файла дампа на стороне бекап базы: {ex.Message}");
				return StatusCode(500, new { error = "Ошибка при отправке файла дампа." });
			}
		}

		[HttpPost("upload-full-dump")]
		public IActionResult UploadFullDump([FromForm] IFormFile dumpFile)
		{
			try
			{

			if (dumpFile == null || dumpFile.Length == 0)
				{
					_ILogger.LogError("Файл дампа отсутствует или пуст.");
					return BadRequest(new { error = "Файл дампа отсутствует или пуст." });
				}

				byte[] fileData;
				using (var memoryStream = new MemoryStream())
				{
					dumpFile.CopyTo(memoryStream);
					fileData = memoryStream.ToArray();
				}
	
				_DumpControl.RestoreFullBackup(fileData);

				return Ok(new { message = "База данных успешно восстановлена." });
			}
			catch (Exception ex)
			{
				_ILogger.LogError($"Ошибка при восстановлении базы данных: {ex.Message}");
				return StatusCode(500, new { error = "Ошибка при восстановлении базы данных." });
			}
		}

	}

}
