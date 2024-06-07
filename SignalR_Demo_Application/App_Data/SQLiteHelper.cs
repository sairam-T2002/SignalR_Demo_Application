using System.Data;
using System;
using System.Data;
using System.Data.Entity;
using System.Data.SQLite;
using System.IO;
using System.Web;

namespace SignalR_Demo_Application.App_Data
{
    public class SQLiteHelper : IDisposable
    {
        private string connectionString;
        private SQLiteConnection connection;
        private DbUtils dbUtils;

        public SQLiteHelper( IConfiguration config )
        {
            dbUtils = config.GetSection("SQLiteUtils").Get<DbUtils>();
            string currentDirectory = Environment.CurrentDirectory;
            connectionString = $"Data Source={currentDirectory+dbUtils.ConnectionString}";
            connection = new SQLiteConnection(connectionString);
            CreateDatabaseIfNotExists();
            CreateSQLiteTable();
        }

        // Checks if the .SQLite files is created
        private void CreateDatabaseIfNotExists()
        {
            var databaseFilePath = connectionString.Split('=')[1].Split(';')[0];
            if (!File.Exists(databaseFilePath))
            {
                SQLiteConnection.CreateFile(databaseFilePath);
            }
        }
        private void CreateSQLiteTable()
        {
            string createTableQuery = @"
            CREATE TABLE IF NOT EXISTS usrs (
                srl INTEGER PRIMARY KEY AUTOINCREMENT,
                name TEXT NOT NULL,
                conn_id TEXT NOT NULL,
                isgrp INTEGER NOT NULL
            );
            CREATE TABLE IF NOT EXISTS msgs (
                srl INTEGER PRIMARY KEY AUTOINCREMENT,
                frm TEXT NOT NULL,
                too TEXT NOT NULL,
                msg TEXT NOT NULL,
                isgrp INTEGER NOT NULL
            );";
            ExecuteNonQuery(createTableQuery);
        }
        // Executes query without returning Results
        public void ExecuteNonQuery( string query )
        {
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                throw new Exception("Error executing query: " + ex.Message);
            }
        }

        // Executes query and also returns results as DataTable type
        public DataTable ExecuteQuery( string query )
        {
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    connection.Open();
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                    connection.Close();
                }
            }
            catch (SQLiteException ex)
            {
                throw new Exception("Error executing query: " + ex.Message);
            }
            return dt;
        }

        public void Dispose()
        {
            if (connection != null)
            {
                connection.Dispose();
                connection = null;
            }
        }
    }
}
