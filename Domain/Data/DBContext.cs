using System.Data.SQLite;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
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
                using (var connection = new SQLiteConnection(_dbConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $@"select [Name] from RfaFamilies";

                    using (var reader = command.ExecuteReader())
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
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return families.ToArray();
        }

        /// <summary></summary>
        public (string Family, string FamilyType) Get(int DBId, int Shape, int Type, int SealingMaterialType, int StructuralDesign, int Minutes)
        {
            (string, string) result = default;
            try
            {
                using (var connection = new SQLiteConnection(_dbConnectionString))
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = $@"
SELECT rfa.Name,fType.TypeName
FROM Sealing seal 
INNER JOIN RfaFamilies rfa ON seal.FamilyId = rfa.Id
INNER JOIN RfaFamilyTypes fType ON seal.FamilyTypeId = fType.Id
WHERE HostCategoryId = {DBId}
AND OpeningShapeId == {Shape}
AND Type == {Type}
AND StructuralDesignId == {StructuralDesign}
AND MaterialId == {SealingMaterialType}
AND FireResistance == {Minutes};";

                    using (var reader = command.ExecuteReader())
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
