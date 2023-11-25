using Azure.Data.Tables;
using Azure.Storage.Queues;
using SantasList.Domain.Models;
using SantasList.Domain.Services;
using System.Collections;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Text.Json;

namespace SantasList.Worker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IGiftSuggestionService _giftSuggestionService;
    private readonly QueueClient _queueClient;
    private readonly TableClient _tableClient;

    public Worker(ILogger<Worker> logger, QueueServiceClient queueServiceClient, TableServiceClient tableServiceClient, IGiftSuggestionService giftSuggestionService)
    {
        _logger = logger;
        _giftSuggestionService = giftSuggestionService;

        _queueClient = queueServiceClient.GetQueueClient("prompts");
        _tableClient = tableServiceClient.GetTableClient("queue");
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        await _queueClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        await _tableClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested) {
            var response = await _queueClient.ReceiveMessagesAsync(maxMessages: 10);
            if (response != null) {
                var messages = response.Value;
                foreach (var message in messages) {

                    var prompt = message.Body.ToObjectFromJson<GiftSuggestionPrompt>();

                    var teResponse = await _tableClient.GetEntityAsync<TableEntity>("1", message.MessageId);
                    var tableEntity = teResponse.Value;

                    var suggestions = await _giftSuggestionService.ProcessAsync(prompt);

                    var suggestionsJson = JsonSerializer.Serialize(suggestions);

                    tableEntity["status"] = (int)QueueStatus.Processed;
                    tableEntity["suggestions"] = suggestionsJson;

                    await _tableClient.UpdateEntityAsync(tableEntity, tableEntity.ETag);
                    await _queueClient.DeleteMessageAsync(message.MessageId, message.PopReceipt);
                }
            }

            await Task.Delay(1000);
        }
    }
}
