using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Reflection;
using System.Windows;

namespace FireBoost.Domain.Data
{
    /// <summary></summary>
    public class DBContext
    {
        private readonly string _dbConnectionString;

        /// <summary></summary>
        public DBContext()
        {
            _dbConnectionString = $"Data Source={Path.GetDirectoryName(Assembly.GetCallingAssembly().Location)}\\FPBoost.db";
        }

        /// <summary></summary>
        public string[] GetFamilies()
        {
            List<string> families = new List<string>();
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_dbConnectionString))
                {
                    connection.Open();

                    using (SQLiteDataReader reader = new SQLiteCommand($@"select [Name] from RfaFamilies", connection).ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reader.FieldCount > 0)
                                {
                                    families.Add(reader.GetValue(0).ToString());
                                }
                            }
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return families.ToArray();
        }

        /// <summary></summary>
        public (string Family, string FamilyType) Get(int hostCategoryId, int shapeId, int type, int materialId, int structuralDesignId, int minutes)
        {
            (string, string) result = default;
            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(_dbConnectionString))
                {
                    connection.Open();
                    SQLiteCommand command = connection.CreateCommand();
                    command.CommandText = $@"
SELECT rfa.Name,fType.TypeName
FROM Sealing seal 
INNER JOIN RfaFamilies rfa ON seal.FamilyId = rfa.Id
INNER JOIN RfaFamilyTypes fType ON seal.FamilyTypeId = fType.Id
WHERE HostCategoryId = {hostCategoryId}
AND OpeningShapeId == {shapeId}
AND Type == {type}
AND StructuralDesignId == {structuralDesignId}
AND MaterialId == {materialId}
AND FireResistance == {minutes};";

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            while (reader.Read())
                            {
                                if (reader.FieldCount == 2)
                                {
                                    result = (reader.GetValue(0).ToString(), reader.GetValue(1).ToString());
                                }
                                break;
                            }
                        }
                    }
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return result;
        }
    }
}
