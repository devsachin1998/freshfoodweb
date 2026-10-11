using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace Admin.Models
{
    public class Database
    {
        private static readonly string ConnectionString =
            ConfigurationManager
                .ConnectionStrings["DBConfig"]
                .ConnectionString;

        private const int CommandTimeout = 60;

        private static SqlConnection CreateConnection()
        {
            return new SqlConnection(ConnectionString);
        }

        // ============================================================
        // GET MAX ID
        // ============================================================
        public static int computeMaxID(string table)
        {
            if (string.IsNullOrWhiteSpace(table))
                return 0;

            try
            {
                if (!IsSafeIdentifier(table))
                    return 0;

                string query =
                    "SELECT ISNULL(MAX(ID), 0) FROM [" + table + "]";

                using (SqlConnection con = CreateConnection())
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = CommandTimeout;

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return 0;

                    int maxId;

                    if (int.TryParse(result.ToString(), out maxId))
                        return maxId;

                    return 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.computeMaxID Error: " + ex
                );

                return 0;
            }
        }

        // ============================================================
        // INSERT / UPDATE / DELETE
        // ============================================================
        public static string insertData(string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
                return "Invalid SQL query.";

            try
            {
                using (SqlConnection con = CreateConnection())
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.CommandTimeout = CommandTimeout;

                    con.Open();

                    cmd.ExecuteNonQuery();

                    return "200";
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.insertData SQL Error: " + ex
                );

                return ex.Message;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.insertData Error: " + ex
                );

                return ex.Message;
            }
        }

        // ============================================================
        // READ SINGLE COLUMN LIST
        // ============================================================
        public static List<string> readData(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            try
            {
                List<string> data = new List<string>();

                using (SqlConnection con = CreateConnection())
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = CommandTimeout;

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            if (reader.IsDBNull(0))
                            {
                                data.Add(string.Empty);
                            }
                            else
                            {
                                data.Add(reader[0].ToString());
                            }
                        }
                    }
                }

                return data;
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.readData SQL Error: " + ex
                );

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.readData Error: " + ex
                );

                return null;
            }
        }

        // ============================================================
        // GET DATASET
        // ============================================================
        public static DataSet getDataSet(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new DataSet();

            try
            {
                DataSet dataSet = new DataSet();

                using (SqlConnection con = CreateConnection())
                using (SqlDataAdapter adapter =
                    new SqlDataAdapter(query, con))
                {
                    adapter.SelectCommand.CommandTimeout =
                        CommandTimeout;

                    adapter.Fill(dataSet);
                }

                return dataSet;
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.getDataSet SQL Error: " + ex
                );

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.getDataSet Error: " + ex
                );

                return null;
            }
        }

        // ============================================================
        // GET SINGLE VALUE
        // ============================================================
        public static string getValue(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return null;

            try
            {
                using (SqlConnection con = CreateConnection())
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.CommandTimeout = CommandTimeout;

                    con.Open();

                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return null;

                    return result.ToString();
                }
            }
            catch (SqlException ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.getValue SQL Error: " + ex
                );

                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    "Database.getValue Error: " + ex
                );

                return null;
            }
        }

        // ============================================================
        // VALIDATE TABLE NAME
        // ============================================================
        private static bool IsSafeIdentifier(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            return true;
        }
    }
}