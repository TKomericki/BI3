using System;
using System.Collections.Generic;
using System.Text;

namespace BI3
{
    class TableData
    {
        public string nazSQLTablica;
        public string imeSQLAtrib;
        public string imeAtrib;
        public string nazAgrFun;

        public TableData(string nazSQLTablica, string imeSQLAtrib, string imeAtrib, string nazAgrFun)
        {
            this.nazSQLTablica = nazSQLTablica;
            this.imeSQLAtrib = imeSQLAtrib;
            this.imeAtrib = imeAtrib;
            this.nazAgrFun = nazAgrFun;
        }

    }
}
