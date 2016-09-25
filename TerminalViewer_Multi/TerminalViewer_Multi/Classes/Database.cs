using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace TerminalViewer
{
    class Database
    {
        private SqlConnection sqlConnection;

        public Database(String dataSource, String username, String password)
        {
            String ConnectionString = String.Format("Data Source={0}; User ID={1}; Password={2}", dataSource, username, password);
            this.sqlConnection = new SqlConnection(ConnectionString);
        }

        public List<ActionPoint> getLiftDropFromDate(DateTime dateDebut)
        {
            List<ActionPoint> listeActions = new List<ActionPoint>();

            using (SqlCommand cmd = new SqlCommand("SELECT Type_LD, LatGPS, LonGPS FROM PLC WHERE DateHeure > @DateHeure AND PosGPS_Slot like '%RF%'", this.sqlConnection))
            {
                cmd.Parameters.AddWithValue("@DateHeure", dateDebut);

                this.sqlConnection.Open();

                try
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                String action = reader.GetString(0);
                                double lat = reader.GetDouble(1);
                                double lon = reader.GetDouble(2);

                                listeActions.Add(new ActionPoint(action, lat, lon));
                            }
                            catch (Exception e)
                            {

                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
                finally
                {
                    if (this.sqlConnection.State == System.Data.ConnectionState.Open)
                        this.sqlConnection.Close();
                }
            }
            return listeActions;
        }
    }
}
