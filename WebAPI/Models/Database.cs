using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace WebAPI.Models
{
    public class Database
    {
        public static int computeMaxID(string table)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConfig"].ConnectionString);
            SqlCommand cmd;
            SqlDataReader dr;
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                int max_id = 0;
                string query = "select max(id) from " + table + "";
                cmd = new SqlCommand(query, con);
                cmd.CommandTimeout = 180;
                con.Open();
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    max_id = Int32.Parse(dr[0].ToString());
                }
                con.Close();
                return max_id;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (con.State == ConnectionState.Open)
                    con.Close();
                return 0;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public static string insertData(string sql)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConfig"].ConnectionString);
            SqlCommand cmd;
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Open();
                cmd = new SqlCommand(sql, con);
                cmd.CommandTimeout = 180;
                cmd.ExecuteNonQuery();
                con.Close();
                return "200";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                if (con.State == ConnectionState.Open)
                    con.Close();
                return e.Data + "..." + e.Message;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public static List<string> readData(string query)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConfig"].ConnectionString);
            SqlCommand cmd;
            SqlDataReader dr;
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                List<string> data = new List<string>();
                con.Open();
                cmd = new SqlCommand(query, con);
                cmd.CommandTimeout = 180;
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    data.Add(dr[0].ToString());
                }
                con.Close();

                return data;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (con.State == ConnectionState.Open)
                    con.Close();
                return null;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public static DataSet getDataSet(string query)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConfig"].ConnectionString);
            SqlCommand cmd;
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Open();
                cmd = new SqlCommand(query, con);
                cmd.CommandTimeout = 180;
                DataSet ds = new DataSet();
                Console.WriteLine(query);
                SqlDataAdapter da = new SqlDataAdapter(query, con);
                da.Fill(ds);
                con.Close();
                return ds;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                if (con.State == ConnectionState.Open)
                    con.Close();
                return null;
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        public static string getValue(string query)
        {
            SqlConnection con = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings["DBConfig"].ConnectionString);
            SqlCommand cmd;
            //FileStream fs1 = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
            //StreamWriter writer = new StreamWriter(fs1);
            try
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
                con.Open();
                cmd = new SqlCommand(query, con);
                //writer.WriteLine("query: " + query);
               object result= cmd.ExecuteScalar();
                string value = String.Empty;
                if(result!=null)
                    value=result.ToString();
               //writer.WriteLine("op: " + value);
               con.Close();
                return value;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //writer.WriteLine(e.Message + "\n" + e.StackTrace);
                if (con.State == ConnectionState.Open)
                    con.Close();
                return null;
            }
            finally
            {
                //writer.Close();
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }
    }
}