using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;

public abstract class Command
{

}

public class OrderStatusChangedCommand : Command
{
    public Guid OrderId { get; init; }
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
    public int Version { get; init; }
}

public class OrderStatusUpdateCommand : Command
{
    public Guid OrderId { get; init; }
    public OrderStatus OldStatus { get; init; }
    public OrderStatus NewStatus { get; init; }
    public int Version { get; init; } // Zad 1 dodanie pola version
}

public class OrderUndoCommand : Command
{
    public Guid OrderId { get; init; }
    public int TargetVersion { get; init; } // Klasa cofajca modyfikacje do zadanej wersji
}

public enum OrderStatus
{
    Pending,
    Processed,
    Shipped,
    Delivered
}

//Zad 2:
public class SqliteOrderCommandRepository : IOrderCommandRepository
{
    private readonly string _connectionString;

    public SqliteOrderCommandRepository(string connectionString)
    {
        _connectionString = connectionString;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS OrderStatusUpdates (
                    OrderId TEXT NOT NULL,
                    OldStatus TEXT NOT NULL,
                    NewStatus TEXT NOT NULL,
                    Version INTEGER NOT NULL,
                    PRIMARY KEY (OrderId, Version)
                );";

            using (var cmd = new SqliteCommand(createTableQuery, connection))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void AddOrderStatusUpdate(OrderStatusUpdateCommand command)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var query = "INSERT INTO OrderStatusUpdates (OrderId, OldStatus, NewStatus, Version) VALUES (@OrderId, @OldStatus, @NewStatus, @Version)";
            
            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", command.OrderId);
                cmd.Parameters.AddWithValue("@OldStatus", command.OldStatus.ToString());
                cmd.Parameters.AddWithValue("@NewStatus", command.NewStatus.ToString());
                cmd.Parameters.AddWithValue("@Version", command.Version);
                
                cmd.ExecuteNonQuery();
            }
        }
    }

    public void UpdateStatus(Guid orderId, OrderStatus newStatus, int version)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var query = "UPDATE OrderStatusUpdates SET NewStatus = @NewStatus, Version = @Version WHERE OrderId = @OrderId";
            
            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@NewStatus", newStatus.ToString());
                cmd.Parameters.AddWithValue("@Version", version);

                cmd.ExecuteNonQuery();
            }
        }
    }

    public List<OrderStatusUpdateCommand> GetOrderStatusUpdates(Guid orderId)
    {
        var orderStatusUpdates = new List<OrderStatusUpdateCommand>();

        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var query = "SELECT OrderId, OldStatus, NewStatus, Version FROM OrderStatusUpdates WHERE OrderId = @OrderId ORDER BY Version ASC";

            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);

                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        orderStatusUpdates.Add(new OrderStatusUpdateCommand
                        {
                            OrderId = reader.GetGuid(0),
                            OldStatus = Enum.Parse<OrderStatus>(reader.GetString(1)),
                            NewStatus = Enum.Parse<OrderStatus>(reader.GetString(2)),
                            Version = reader.GetInt32(3)
                        });
                    }
                }
            }
        }

        return orderStatusUpdates;
    }

    public void RevertToVersion(Guid orderId, int targetVersion)
    {
        using (var connection = new SqliteConnection(_connectionString))
        {
            connection.Open();

            var query = "DELETE FROM OrderStatusUpdates WHERE OrderId = @OrderId AND Version > @TargetVersion";
            
            using (var cmd = new SqliteCommand(query, connection))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@TargetVersion", targetVersion);

                cmd.ExecuteNonQuery();
            }
        }
    }
}

public class OrderCommandHandler
{
    protected IOrderCommandRepository RepDB { get; private set; }
    protected EventBroker Broker { get; private set; }

    public OrderCommandHandler(IOrderCommandRepository repository, EventBroker broker)
    {
        this.RepDB = repository;
        this.Broker = broker;
    }

    public void CommandHandler(Command command)
    {
        switch (command)
        {
            case OrderStatusChangedCommand orderCommand:
                Broker.Command(new OrderStatusUpdateCommand()
                {
                    OrderId = orderCommand.OrderId,
                    OldStatus = orderCommand.OldStatus,
                    NewStatus = orderCommand.NewStatus,
                    Version = orderCommand.Version
                });
                break;

            case OrderUndoCommand undoCommand:
                RepDB.RevertToVersion(undoCommand.OrderId, undoCommand.TargetVersion);
                break;
        }
    }
}

public class OrderEventHandler
{
    protected IOrderCommandRepository RepDB { get; private set; }

    public OrderEventHandler(IOrderCommandRepository repository, EventBroker broker)
    {
        this.RepDB = repository;
        broker.OnNewCommand += EventHandler!;
    }

    public void EventHandler(object sender, Command command)
    {
        switch (command)
        {
            case OrderStatusUpdateCommand orderCommand:
                RepDB.UpdateStatus(orderCommand.OrderId, orderCommand.NewStatus, orderCommand.Version);
                break;
        }
    }
}


public class EventBroker
{
    public event EventHandler<Command>? OnNewCommand;

    public void Command(Command command)
    {
        OnNewCommand?.Invoke(this, command);
    }
}


// Interface for order command repository
public interface IOrderCommandRepository
{
    void AddOrderStatusUpdate(OrderStatusUpdateCommand command);
    void UpdateStatus(Guid orderId, OrderStatus newStatus, int version);
    List<OrderStatusUpdateCommand> GetOrderStatusUpdates(Guid orderId);
    void RevertToVersion(Guid orderId, int targetVersion);
}