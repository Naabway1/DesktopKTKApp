using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DesktopKTKApp.Model
{
    public static class KTKdb
    {
        private static readonly SqlConnection db = new SqlConnection("Server=localhost;Database=master;Trusted_Connection=True"); //для подключения дома
        //private static SqlConnection db = new SqlConnection("Server=10.14.206.27;Database=user5;User Id=user5;Password=Lu%5%4e4"); //для подключения в ктк

        public async static void SQLExecuteAsync(string sql)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();
            SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = db,
                CommandText = sql
            };
            await sqlCommand.ExecuteNonQueryAsync();
            db.Close();
        }
        public static void SQLExecute(string sql)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();
            SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = db,
                CommandText = sql
            };
            sqlCommand.ExecuteNonQuery();
            db.Close();
        }
        public static bool SQLRead(string sql)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();
            SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = db,
                CommandText = sql
            };
            var reader = sqlCommand.ExecuteReader();
            bool result = reader.Read();
            reader.Close();
            db.Close();
            return result;
        }
        public static object SQLReadValue(string sql)
        {
            try
            {
                db.Open();
                var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
                cmd.ExecuteNonQuery();

                var sqlCommand = new SqlCommand()
                {
                    Connection = db,
                    CommandText = sql
                };

                var reader = sqlCommand.ExecuteReader();

                // Проверка наличия строк
                if (reader.Read())
                {
                    object obj = reader.GetValue(0);
                    reader.Close();
                    return obj;
                }
                else
                {
                    reader.Close();
                    throw new Exception("Запрос не вернул данных.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                db.Close();
            }
        }
        public static DataTable SQLGetData(string sql)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();
            SqlCommand sqlCommand = new SqlCommand()
            {
                Connection = db,
                CommandText = sql
            };
            var dataTable = new DataTable();
            using (var adapter = new SqlDataAdapter(sqlCommand))
            {
                adapter.Fill(dataTable);
            }
            db.Close();
            return dataTable;
        } // Использование: DataTable data = KTKdb.SQLGetData("SELECT * FROM Users");
        public static List<string> SQLGetListOfData (string sql)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();
            var list = new List<string>();
            var sqlCommand = new SqlCommand() 
            { 
                Connection = db,
                CommandText = sql
            };
            var reader = sqlCommand.ExecuteReader();
            while (reader.Read())
            {
                list.Add(reader.GetValue(0).ToString());
            }
            db.Close();
            return list;
        }
        public static void SQLInsertData(string tableName, Dictionary<string, object> values)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();

            string columns = string.Join(", ", values.Keys);
            string parameters = string.Join(", ", values.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            SqlCommand sqlCommand = new SqlCommand(sql, db);
            foreach (var pair in values)
            {
                sqlCommand.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
            }

            sqlCommand.ExecuteNonQuery();
            db.Close();
        }  // Использование: KTKdb.InsertData("Users", new Dictionary<string, object> { {"...", "..."}, {"...", "..."} });
        public static async void SQLInsertDataAsync(string tableName, Dictionary<string, object> values)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();

            string columns = string.Join(", ", values.Keys);
            string parameters = string.Join(", ", values.Keys.Select(k => $"@{k}"));
            string sql = $"INSERT INTO {tableName} ({columns}) VALUES ({parameters})";

            SqlCommand sqlCommand = new SqlCommand(sql, db);
            foreach (var pair in values)
            {
                sqlCommand.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
            }

            sqlCommand.ExecuteNonQuery();
            db.Close();
        }
        public static void SQLUpdateData(string tableName, Dictionary<string, object> values, string condition)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();

            string setClause = string.Join(", ", values.Keys.Select(k => $"{k} = @{k}"));
            string sql = $"UPDATE {tableName} SET {setClause} WHERE {condition}";

            SqlCommand sqlCommand = new SqlCommand(sql, db);
            foreach (var pair in values)
            {
                sqlCommand.Parameters.AddWithValue($"@{pair.Key}", pair.Value);
            }

            sqlCommand.ExecuteNonQuery();
            db.Close();
        } // Использование: как и сверху, но по другому
        public static bool SQLRecordExists(string tableName, string condition)
        {
            db.Open();
            var cmd = new SqlCommand() { Connection = db, CommandText = "USE KTKdb" };
            cmd.ExecuteNonQuery();

            string sql = $"SELECT 1 FROM {tableName} WHERE {condition}";
            SqlCommand sqlCommand = new SqlCommand(sql, db);
            var result = sqlCommand.ExecuteScalar();
            db.Close();

            return result != null;
        } // Использование: bool exists = KTKdb.RecordExists("Users", "Login = 'ivanov'");
    }
}
