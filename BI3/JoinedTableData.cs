using System;
using System.Collections.Generic;
using System.Text;

namespace BI3
{
    class JoinedTableData
    {
        public string nazDimSQLTablica;
        public string nazCinjSQLTablica;
        public string cinjTabKljuc;
        public string dimTabKljuc;
        public string imeSQLAtrib;

        public JoinedTableData(string nazDimSQLTablica, string nazCinjSQLTablica, string cinjTabKljuc, string dimTabKljuc, string imeSQLAtrib)
        {
            this.nazDimSQLTablica = nazDimSQLTablica;
            this.nazCinjSQLTablica = nazCinjSQLTablica;
            this.cinjTabKljuc = cinjTabKljuc;
            this.dimTabKljuc = dimTabKljuc;
            this.imeSQLAtrib = imeSQLAtrib;
        }

    }
}
