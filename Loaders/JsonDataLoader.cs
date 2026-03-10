using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using InventoryKpiSystem.Models;

namespace InventoryKpiSystem.Loaders
{
	public class JsonFileLoader : IJsonFileLoader
	{
		private readonly JsonSerializerOptions _jsonOptions;

		public JsonFileLoader()
		{
			_jsonOptions = new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true
			};
		}

		public async IAsyncEnumerable<PurchaseOrder> LoadPurchaseOrdersAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in LoadDataStreamAsync<PurchaseOrder>(filePath, cancellationToken))
			{
				yield return item;
			}
		}

		public async IAsyncEnumerable<Invoice> LoadInvoicesAsync(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken = default)
		{
			await foreach (var item in LoadDataStreamAsync<Invoice>(filePath, cancellationToken))
			{
				yield return item;
			}
		}

		private async IAsyncEnumerable<T> LoadDataStreamAsync<T>(string filePath, [EnumeratorCancellation] CancellationToken cancellationToken) where T : class
		{
			if (!File.Exists(filePath))
			{
				Console.WriteLine($"[Warning] File not found: {filePath}");
				yield break;
			}

			var fileOptions = new FileStreamOptions
			{
				Mode = FileMode.Open,
				Access = FileAccess.Read,
				Share = FileShare.Read,
				Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
				BufferSize = 65536
			};

			FileStream? stream = null;

			IAsyncEnumerator<T?>? enumerator = null;

			try
			{
				stream = File.Open(filePath, fileOptions);
				var asyncStream = JsonSerializer.DeserializeAsyncEnumerable<T>(stream, _jsonOptions, cancellationToken);
				enumerator = asyncStream.GetAsyncEnumerator(cancellationToken);
			}
			catch (IOException ex)
			{
				Console.WriteLine($"[Error] IO Exception for {filePath}: {ex.Message}");
				if (stream != null) await stream.DisposeAsync();
				yield break;
			}

			bool hasNext = true;
			while (hasNext)
			{
				try
				{
					hasNext = await enumerator.MoveNextAsync();
				}
				catch (JsonException ex)
				{
					Console.WriteLine($"[Error] Invalid JSON in {filePath}: {ex.Message}");
					break;
				}
				catch (Exception ex)
				{
					Console.WriteLine($"[Error] Unexpected error in {filePath}: {ex.Message}");
					break;
				}

				if (hasNext && enumerator.Current != null)
				{
					yield return enumerator.Current;
				}
			}

			if (enumerator != null) await enumerator.DisposeAsync();
			if (stream != null) await stream.DisposeAsync();
		}
	}
}