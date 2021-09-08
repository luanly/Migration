using Microsoft.Azure.Cosmos.Table;
using Microsoft.Extensions.Caching.Memory;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
	public class TableStorageLock
		:
		IAsyncDisposable
	{
		#region Felder

		TableEntity _lockEntity;

		static readonly Regex DisallowedCharsInRowKeys = new Regex(@"[\\\\#%+/?\u0000-\u001F\u007F-\u009F]");

		#endregion

		#region Konstruktor

		public TableStorageLock(string rowKey, [CallerMemberName] string partitionKey = "")
		{
			if (DisallowedCharsInRowKeys.IsMatch(rowKey))
			{
				rowKey = DisallowedCharsInRowKeys.Replace(rowKey, string.Empty);
			}
			_lockEntity = new TableEntity(partitionKey, rowKey);
		}

		#endregion

		#region Eigenschaften

		internal static TableStorageRepository Repo { get; set; }

		#endregion

		#region Methoden

		public async ValueTask DisposeAsync()
		{
			if (_lockEntity == null)
			{
				return;
			}
			try
			{
				_lockEntity.ETag = "*";
				await Repo.ExecuteAsync(TableOperation.Delete(_lockEntity));
			}
			catch (StorageException ex) when (ex.RequestInformation.HttpStatusCode == 404) { }
			catch (Exception ex)
			{
				Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat,(nameof(TableStorageLock), _lockEntity));
			}
		}

		public async Task<bool> TryEnter(int maxLockDurationInMinutes = 1)
		{
			try
			{
				await Repo.ExecuteAsync(TableOperation.Insert(_lockEntity), throwOnException: true);
				return true;
			}
			catch(StorageException ex)
			{
				var allowEnter = false;
				try
				{
					if (ex.RequestInformation.HttpStatusCode == 409)
					{
						try
						{
							var item = await Repo.ExecuteAsync(TableOperation.Retrieve(_lockEntity.PartitionKey, _lockEntity.RowKey));
							if (item == null || item.Result == null)
							{
								//Item wurde bereits gelöscht. Wir versuchen es nochmals
								try
								{
									await Repo.ExecuteAsync(TableOperation.Insert(_lockEntity));
									allowEnter = true;
									return allowEnter;
								}
								catch { }
								allowEnter = false;
								return allowEnter;
							}
							if (((DynamicTableEntity)item.Result).Timestamp < DateTimeOffset.UtcNow.AddMinutes(-maxLockDurationInMinutes))
							{
								//Failed deleted _lockEntity oder dauert zu lange
								allowEnter = true;
								return allowEnter;
							}
						}
						catch (StorageException) { }

						allowEnter = false;
						return allowEnter;
					}
				}
				finally
				{
					if (!allowEnter)
					{
						_lockEntity = null;
					}
				}

				return false;
			}
			catch(Exception ignored)
			{
				Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
				return false;
			}
		}

		#endregion

		#region Statische Methoden

		public static async Task InitializeAsync(CloudTableClient client)
		{
			Repo = new TableStorageRepository(false);
			await Repo.InitializeAsync(AzureConstants.TableStorageLockTable, client: client);
		}

		#endregion
	}
}
