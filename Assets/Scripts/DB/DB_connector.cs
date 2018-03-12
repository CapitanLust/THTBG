using System.Collections;
using System.Data.SqlClient;
using System.Data;
using System.Collections.Generic;
using UnityEngine;

public static class DB_connector {
    static SqlConnection connection;
    static SqlCommand sqlCommand;

    //public static void lol()
    //{
    //    string connectionString =
    //        "Server=DESKTOP-9NE07U8;" +
    //        "Database=THBG;" +
    //        "User ID=MySqlServerUserId;" +
    //        "Password=MySqlServerPassword;";

    //    IDbConnection dbcon;

    //    using (dbcon = new SqlConnection(connectionString))
    //    {
    //        dbcon.Open();

    //        using (var dbcmd = dbcon.CreateCommand())
    //        {
    //            dbcmd.CommandText = "SELECT name, description, price, xp, item_type FROM " +
    //                                "items JOIN item_type ON item_type_key = item_type_id";
    //            using (var reader = dbcmd.ExecuteReader())
    //            {
    //                while (reader.Read())
    //                {
    //                    Debug.Log("test");
    //                }
    //            }
    //        }
    //    }
    //}

    public static void OpenConnection()
    {
        var sConnStr = new SqlConnectionStringBuilder()
        {
            DataSource = "DESKTOP-9NE07U8",
            InitialCatalog = "THBG",
            IntegratedSecurity = false
        }.ConnectionString;

        connection = new SqlConnection(sConnStr);

        sqlCommand = new SqlCommand();
        sqlCommand.Connection = connection;

        connection.Open();
    }

    /// <summary>
    /// parse DB and return initialized items
    /// </summary>
    /// <param name="player_id">need to find bought items</param>
    /// <returns>initialized items</returns>
    public static List<Item> GetItemsInfo(int player_id)
    {
        List<Item> listItems = new List<Item>();

        sqlCommand.CommandText = "SELECT name, description, price, xp, item_type FROM " +
                                 "items JOIN item_type ON item_type_key = item_type_id";

        using (var reader = sqlCommand.ExecuteReader())
        {
            while (reader.Read())
            {
                listItems.Add(new Item
                {
                    //from DB
                    name = (string)reader[0],
                    description = (string)reader[1],
                    price = System.Convert.ToInt32(reader[2]),
                    level = System.Convert.ToInt32(reader[3]) / 100,//TODO level convertation
                    type = (string)reader[4],
                    //not from DB
                    isBought = false,
                    image = null
                });
            }
        }

        //sqlCommand.CommandText = "SELECT name FROM player_item JOIN items " +
        //                         "ON items.item_id = player_item.item_key " +
        //                         "WHERE player_item.player_key = @player_id";

        //sqlCommand.Parameters.Clear();
        //sqlCommand.Parameters.AddWithValue("@player_id", player_id);

        //using (var reader = sqlCommand.ExecuteReader())
        //{
        //    string name = "";

        //    while (reader.Read())
        //    {
        //        name = (string)reader[0];

        //        foreach (var item in listItems)
        //        {
        //            if (item.name == name)
        //            {
        //                item.isBought = true;
        //                break;
        //            }
        //        }    
        //    }
        //}

        return listItems;
    }
}
