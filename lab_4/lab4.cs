using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

// Zadanie 1: Klasa reprezentująca zdarzenie w systemie
class Event
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public required string Source { get; set; } // Wymagana wartość
    public EventType Type { get; set; }
    public required string Data { get; set; } // Wymagana wartość
    public string? Identifier { get; set; } // Może być null
}

enum EventType
{
    Information,
    Warning,
    Error,
    CriticalError
}

class EventRepository
{
    private readonly string connectionString;

    public EventRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    // Zadanie 2: Dodanie podstawowej obsługi bazy danych
    public void AddEvent(Event eventData)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO Events (Timestamp, Source, Type, Data, Identifier) VALUES (@Timestamp, @Source, @Type, @Data, @Identifier)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Timestamp", eventData.Timestamp);
            command.Parameters.AddWithValue("@Source", eventData.Source);
            command.Parameters.AddWithValue("@Type", eventData.Type.ToString());
            command.Parameters.AddWithValue("@Data", eventData.Data);
            command.Parameters.AddWithValue("@Identifier", eventData.Identifier ?? (object)DBNull.Value);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    // Pobieranie wszystkich zdarzeń z bazy danych
    public List<Event> GetEvents()
    {
        List<Event> events = new List<Event>();

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "SELECT Id, Timestamp, Source, Type, Data, Identifier FROM Events";
            using SqlCommand command = new SqlCommand(query, connection);
            connection.Open();

            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    events.Add(new Event
                    {
                        Id = (int)reader["Id"],
                        Timestamp = (DateTime)reader["Timestamp"],
                        Source = reader["Source"].ToString()!,
                        Type = Enum.Parse<EventType>(reader["Type"].ToString()!),
                        Data = reader["Data"].ToString()!,
                        Identifier = reader["Identifier"] as string
                    });
                }
            }
        }

        return events;
    }

    // Usuwanie zdarzenia z bazy danych
    public void DeleteEvent(int eventId)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "DELETE FROM Events WHERE Id = @Id";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", eventId);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}

// Zadanie 3: Klasa do logowania błędów do bazy danych
class ErrorLogger
{
    private readonly string mainDbConnection;
    private readonly string localDbConnection;

    public ErrorLogger(string mainDb, string localDb)
    {
        mainDbConnection = mainDb;
        localDbConnection = localDb;
    }

    public void LogError(Event errorEvent)
    {
        try
        {
            InsertIntoDatabase(mainDbConnection, errorEvent);
        }
        catch (Exception)
        {
            InsertIntoDatabase(localDbConnection, errorEvent);
        }
    }

    private void InsertIntoDatabase(string connectionString, Event errorEvent)
    {
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            string query = "INSERT INTO ErrorLogs (Timestamp, Source, Type, Data, Identifier) VALUES (@Timestamp, @Source, @Type, @Data, @Identifier)";
            using SqlCommand command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Timestamp", errorEvent.Timestamp);
            command.Parameters.AddWithValue("@Source", errorEvent.Source);
            command.Parameters.AddWithValue("@Type", errorEvent.Type.ToString());
            command.Parameters.AddWithValue("@Data", errorEvent.Data);
            command.Parameters.AddWithValue("@Identifier", errorEvent.Identifier ?? (object)DBNull.Value);
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    public void SyncDatabases()
    {
        using (SqlConnection localConnection = new SqlConnection(localDbConnection))
        {
            localConnection.Open();
            using (SqlTransaction transaction = localConnection.BeginTransaction())
            {
                try
                {
                    string selectQuery = "SELECT * FROM ErrorLogs";
                    using SqlCommand selectCommand = new SqlCommand(selectQuery, localConnection, transaction);
                    using (SqlDataReader reader = selectCommand.ExecuteReader())
                    {
                        List<Event> localEvents = new List<Event>();
                        while (reader.Read())
                        {
                            localEvents.Add(new Event
                            {
                                Id = (int)reader["Id"],
                                Timestamp = (DateTime)reader["Timestamp"],
                                Source = reader["Source"].ToString()!,
                                Type = Enum.Parse<EventType>(reader["Type"].ToString()!),
                                Data = reader["Data"].ToString()!,
                                Identifier = reader["Identifier"] as string
                            });
                        }

                        foreach (var evt in localEvents)
                        {
                            try
                            {
                                InsertIntoDatabase(mainDbConnection, evt);
                                string deleteQuery = "DELETE FROM ErrorLogs WHERE Id = @Id";
                                using SqlCommand deleteCommand = new SqlCommand(deleteQuery, localConnection, transaction);
                                deleteCommand.Parameters.AddWithValue("@Id", evt.Id);
                                deleteCommand.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                 Console.WriteLine($"Error occurred: {ex.Message}");
                                throw;
                            }
                        }
                    }

                    transaction.Commit();
                }
                catch (Exception)
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }
    }
}
