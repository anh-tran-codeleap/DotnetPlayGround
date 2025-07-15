using Confluent.Kafka;

public class KafkaIntegrationTests
{
    // --- Configuration ---
    // Replace these values with your actual Kafka details.
    // Based on your docker-compose file, you should connect to the secure SASL port.
    private const string KafkaBootstrapServer = "localhost:9093";
    private const string TestTopic = "7dcb13f7-a937-4580-bb00-7abaadbca375.orders"; // e.g., "tenant-acme.orders"
    private const string TestUsername = "7dcb13f7-a937-4580-bb00-7abaadbca375-user";     // e.g., "admin" or a tenant-specific user
    private const string TestPassword = "SMuu6sPiiLIbM3HA9YyVfmGuoN9LU6ay";     // e.g., "admin-secret"

    [Fact]
    public async Task ProduceAndConsume_Should_Succeed()
    {
        // --- ARRANGE ---

        // 1. Producer Configuration
        // This setup uses SASL/PLAIN authentication as defined in your docker-compose.yml.
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = KafkaBootstrapServer,
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = TestUsername,
            SaslPassword = TestPassword
        };

        // 2. Consumer Configuration
        // Note: Each test run uses a unique GroupId to ensure it reads from the beginning
        // of the topic and doesn't interfere with other consumers.
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = KafkaBootstrapServer,
            GroupId = $"test-consumer-group-{Guid.NewGuid()}",
            AutoOffsetReset = AutoOffsetReset.Earliest, // Start reading from the beginning of the topic
            SecurityProtocol = SecurityProtocol.SaslPlaintext,
            SaslMechanism = SaslMechanism.Plain,
            SaslUsername = TestUsername,
            SaslPassword = TestPassword
        };

        // A unique message to produce and verify
        var testMessage = $"Test message at {DateTime.UtcNow:O}";

        // --- ACT ---

        // 3. Produce the message
        using (var producer = new ProducerBuilder<Null, string>(producerConfig).Build())
        {
            var deliveryResult = await producer.ProduceAsync(TestTopic, new Message<Null, string> { Value = testMessage });
            Assert.NotNull(deliveryResult);
            Assert.Equal(PersistenceStatus.Persisted, deliveryResult.Status);
        }

        // 4. Consume the message
        string consumedMessage = null;
        using (var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build())
        {
            consumer.Subscribe(TestTopic);

            // Set a timeout for the consume operation to prevent the test from hanging indefinitely.
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            try
            {
                var consumeResult = consumer.Consume(cts.Token);
                consumedMessage = consumeResult?.Message?.Value;
            }
            catch (ConsumeException ex) when (ex.Error.IsFatal)
            {
                // Handle fatal consumer errors
                Assert.Fail($"A fatal error occurred while consuming: {ex.Error.Reason}");
            }
            catch (OperationCanceledException)
            {
                // This happens if the timeout is reached before a message is consumed.
                Assert.Fail("Test timed out. No message was consumed.");
            }
            finally
            {
                consumer.Close();
            }
        }

        // --- ASSERT ---

        // 5. Verify the consumed message is what we produced
        Assert.NotNull(consumedMessage);
        Assert.Equal(testMessage, consumedMessage);
    }
}