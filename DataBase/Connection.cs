using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CinemaApp.DataBase
{
    internal class Connection
    {
        public static CinemaDBEntities db = new CinemaDBEntities();

        public static bool TestConnection()
        {
            try
            {
                db.Database.Connection.Open();
                db.Database.Connection.Close();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }


}
