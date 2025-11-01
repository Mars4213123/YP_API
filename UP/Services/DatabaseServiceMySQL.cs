//using System;
//using System.Collections.Generic;
//using MySql.Data.MySqlClient;
//using Newtonsoft.Json;

//namespace UP.Services
//{
//    public class DatabaseServiceMySQL
//    {
//        private readonly string _connectionString;

//        public DatabaseServiceMySQL(string server = "localhost", string database = "recipes_db", string user = "root", string password = "root")
//        {
//            _connectionString = $"Server={server};Database={database};Uid={user};Pwd={password};Charset=utf8mb4;";
//            InitializeDatabase();
//        }

//        private void InitializeDatabase()
//        {
//            var conn = new MySqlConnection(_connectionString);
//            conn.Open();

//            var cmd = conn.CreateCommand();
//            cmd.CommandText = @"
//                CREATE TABLE IF NOT EXISTS recipes (
//                    id VARCHAR(36) PRIMARY KEY,
//                    title VARCHAR(255) NOT NULL,
//                    ingredients_json TEXT,
//                    steps_json TEXT,
//                    tags_json TEXT,
//                    calories INT DEFAULT 0,
//                    photo_path VARCHAR(255)
//                );

//                CREATE TABLE IF NOT EXISTS ingredients (
//                    id VARCHAR(36) PRIMARY KEY,
//                    name VARCHAR(255) NOT NULL,
//                    category VARCHAR(100),
//                    allergens_json TEXT
//                );

//                CREATE TABLE IF NOT EXISTS menus (
//                    id VARCHAR(36) PRIMARY KEY,
//                    user_id VARCHAR(36),
//                    created_at DATETIME,
//                    menu_json TEXT
//                );

//                CREATE TABLE IF NOT EXISTS favorites (
//                    recipe_id VARCHAR(36) PRIMARY KEY,
//                    added_at DATETIME
//                );
//            ";
//            cmd.ExecuteNonQuery();
//        }

//        private MySqlConnection GetConnection()
//        {
//            var conn = new MySqlConnection(_connectionString);
//            conn.Open();
//            return conn;
//        }

//        // Добавление или обновление рецепта
//        public void InsertOrUpdateRecipe(string id, string title, string ingredientsJson, string stepsJson, string tagsJson, int calories = 0, string photoPath = null)
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = @"
//                INSERT INTO recipes (id, title, ingredients_json, steps_json, tags_json, calories, photo_path)
//                VALUES (@id, @title, @ingredients_json, @steps_json, @tags_json, @calories, @photo_path)
//                ON DUPLICATE KEY UPDATE
//                    title = VALUES(title),
//                    ingredients_json = VALUES(ingredients_json),
//                    steps_json = VALUES(steps_json),
//                    tags_json = VALUES(tags_json),
//                    calories = VALUES(calories),
//                    photo_path = VALUES(photo_path);
//            ";
//            cmd.Parameters.AddWithValue("@id", id);
//            cmd.Parameters.AddWithValue("@title", title);
//            cmd.Parameters.AddWithValue("@ingredients_json", ingredientsJson ?? "");
//            cmd.Parameters.AddWithValue("@steps_json", stepsJson ?? "");
//            cmd.Parameters.AddWithValue("@tags_json", tagsJson ?? "");
//            cmd.Parameters.AddWithValue("@calories", calories);
//            cmd.Parameters.AddWithValue("@photo_path", photoPath ?? "");
//            cmd.ExecuteNonQuery();
//        }

//        public IEnumerable<(string id, string title, string ingredientsJson, string stepsJson, string tagsJson, int calories, string photo)> GetAllRecipes()
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = "SELECT id, title, ingredients_json, steps_json, tags_json, calories, photo_path FROM recipes ORDER BY title;";
//            var reader = cmd.ExecuteReader();

//            while (reader.Read())
//            {
//                yield return (
//                    reader.GetString("id"),
//                    reader.GetString("title"),
//                    reader.IsDBNull("ingredients_json") ? "" : reader.GetString("ingredients_json"),
//                    reader.IsDBNull("steps_json") ? "" : reader.GetString("steps_json"),
//                    reader.IsDBNull("tags_json") ? "" : reader.GetString("tags_json"),
//                    reader.IsDBNull("calories") ? 0 : reader.GetInt32("calories"),
//                    reader.IsDBNull("photo_path") ? "" : reader.GetString("photo_path")
//                );
//            }
//        }

//        public void SaveMenu(string id, string userId, string menuJson)
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = @"
//                INSERT INTO menus (id, user_id, created_at, menu_json)
//                VALUES (@id, @user_id, @created_at, @menu_json)
//                ON DUPLICATE KEY UPDATE
//                    menu_json = VALUES(menu_json),
//                    created_at = VALUES(created_at);
//            ";
//            cmd.Parameters.AddWithValue("@id", id);
//            cmd.Parameters.AddWithValue("@user_id", userId ?? "");
//            cmd.Parameters.AddWithValue("@created_at", DateTime.UtcNow);
//            cmd.Parameters.AddWithValue("@menu_json", menuJson ?? "");
//            cmd.ExecuteNonQuery();
//        }

//        public void AddFavorite(string recipeId)
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = "INSERT INTO favorites (recipe_id, added_at) VALUES (@id, @added_at) ON DUPLICATE KEY UPDATE added_at = VALUES(added_at);";
//            cmd.Parameters.AddWithValue("@id", recipeId);
//            cmd.Parameters.AddWithValue("@added_at", DateTime.UtcNow);
//            cmd.ExecuteNonQuery();
//        }

//        public void RemoveFavorite(string recipeId)
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = "DELETE FROM favorites WHERE recipe_id = @id;";
//            cmd.Parameters.AddWithValue("@id", recipeId);
//            cmd.ExecuteNonQuery();
//        }

//        public bool IsFavorite(string recipeId)
//        {
//            var conn = GetConnection();
//            var cmd = conn.CreateCommand();
//            cmd.CommandText = "SELECT 1 FROM favorites WHERE recipe_id = @id LIMIT 1;";
//            cmd.Parameters.AddWithValue("@id", recipeId);
//                var reader = cmd.ExecuteReader();
//            return reader.Read();
//        }
//    }
//}
