using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.IO;

namespace BI3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public string ConnectionString = "";
        SqlConnection conn;
        Dictionary<string, int> tablesID;
        int currentFactTableId = -1;
        string currentFactTable = "";


        Dictionary<string, TableData> tablesData;
        Dictionary<Tuple<string, string>, JoinedTableData> joinedTablesData;

        List<string> usedMeasures;
        List<Tuple<string, string>> usedDimensions;

        public MainWindow()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            IConfigurationRoot configuration = builder.Build();

            ConnectionString = configuration.GetConnectionString("DB");

            Console.WriteLine(configuration.GetConnectionString("DB"));


            conn = new SqlConnection(ConnectionString);
            conn.Open();
            SqlCommand com = conn.CreateCommand();
            com.CommandText = "SELECT * FROM tablica WHERE sifTipTablica = 1";
            SqlDataReader reader = com.ExecuteReader();

            InitializeComponent();

            tablesID = new Dictionary<string, int>();

            int maxLen = 0;
            while (reader.Read())
            {
                string tableName = reader.GetValue(1).ToString().Trim(' ');
                string tableNameSQL = reader.GetValue(2).ToString().Trim(' ');
                int tableID = Convert.ToInt32(reader.GetValue(0));

                tablesID[tableNameSQL] = tableID;
                maxLen = maxLen < tableName.Length ? tableName.Length : maxLen;
                RadioButton rb = new RadioButton() { Content = tableName, Name = tableNameSQL, GroupName = "factTables", VerticalContentAlignment = VerticalAlignment.Center};
                rb.Checked += GetDimensionsAndMeasures;
                factsPanel.Children.Add(rb);
            }

            int fontSize = Math.Min(800 / maxLen, 24);
            foreach (RadioButton rb in factsPanel.Children)
            {
                rb.FontSize = fontSize;
            }
            reader.Close();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (conn.State == System.Data.ConnectionState.Open)
            {
                conn.Close();
                conn.Dispose();
                base.OnClosing(e);
            }
        }

        //Get all the dimensions and measures based on the selected factual table
        private void GetDimensionsAndMeasures(object sender, RoutedEventArgs e)
        {
            functionsPanel.Children.Clear();
            dimensionPanel.Children.Clear();
            testBlock.Text = "";
            searchButton.Visibility = Visibility.Hidden;

            tablesData = new Dictionary<string, TableData>();
            joinedTablesData = new Dictionary<Tuple<string, string>, JoinedTableData>();
            currentFactTable = (sender as RadioButton).Name;
            currentFactTableId = tablesID[currentFactTable];
            usedMeasures = new List<string>();
            usedDimensions = new List<Tuple<string, string>>();


            SqlCommand com = conn.CreateCommand();
            com.CommandText = "SELECT imeSQLAtrib, tabAtribut.imeAtrib, nazAgrFun, nazSQLTablica, tabAtributAgrFun.imeAtrib" +
            "\nFROM tabAtribut, agrFun, tablica, tabAtributAgrFun WHERE tablica.sifTablica = " + currentFactTableId +
            "\nAND tabAtribut.sifTablica = tabAtributAgrFun.sifTablica" +
            "\nAND tabAtribut.rbrAtrib = tabAtributAgrFun.rbrAtrib" +
            "\nAND tabAtributAgrFun.sifAgrFun = agrFun.sifAgrFun" +
            "\nAND tabAtribut.sifTablica = tablica.sifTablica" +
            "\nAND sifTipAtrib = 1" +
            "\nORDER BY tabAtribut.rbrAtrib";
    
            SqlDataReader reader = com.ExecuteReader();
            int maxLen = 1;
            while (reader.Read())
            {
                string imeSQLAtrib = reader.GetValue(0).ToString().Trim(' ');
                string imeAtrib = reader.GetValue(1).ToString().Trim(' ');
                string nazAgrFun = reader.GetValue(2).ToString().Trim(' ');
                string nazSQLTablica = reader.GetValue(3).ToString().Trim(' ');
                string funcName = reader.GetValue(4).ToString().Trim(' ');

                tablesData[funcName] = new TableData(nazSQLTablica, imeSQLAtrib, funcName, nazAgrFun);

                maxLen = maxLen < funcName.Length ? funcName.Length : maxLen;
                CheckBox cb = new CheckBox() { Content = funcName, VerticalContentAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Normal};
                cb.Checked += AddMeasure;
                cb.Unchecked += RemoveMeasure;
                functionsPanel.Children.Add(cb);
            }
            reader.Close();

            int fontSize = Math.Min(500 / maxLen, 20);
            foreach (CheckBox cb in functionsPanel.Children)
            {
                cb.FontSize = fontSize;
            }


            com.CommandText = "SELECT dimTablica.nazTablica, dimTablica.nazSQLTablica AS nazSqlDimTablica, cinjTablica.nazSQLTablica AS nazSqlCinjTablica" +
                ", cinjTabAtribut.imeSqlAtrib AS cinjTabKljuc, dimTabAtribut.imeSqlAtrib AS dimTabKljuc, tabAtribut.imeSQLAtrib AS imeSQLAtrib, tabAtribut.imeAtrib AS imeAtrib, tabAtribut.rbrAtrib" +
                "\nFROM tabAtribut, dimCinj, tablica dimTablica, tablica cinjTablica, tabAtribut cinjTabAtribut, tabAtribut dimTabAtribut" +
                "\nWHERE dimCinj.sifDimTablica = dimTablica.sifTablica" +
                "\nAND dimCinj.sifCinjTablica = cinjTablica.sifTablica" +
                "\nAND dimCinj.sifCinjTablica = cinjTabAtribut.sifTablica" +
                "\nAND dimCinj.rbrCinj = cinjTabAtribut.rbrAtrib" +
                "\nAND dimCinj.sifDimTablica = dimTabAtribut.sifTablica" +
                "\nAND dimCinj.rbrDim = dimTabAtribut.rbrAtrib" +
                "\nAND tabAtribut.sifTablica = dimCinj.sifDimTablica" +
                "\nAND sifCinjTablica = " + currentFactTableId +
                "\nAND tabAtribut.sifTipAtrib = 2" +
                "\nORDER BY dimTablica.nazTablica, rbrAtrib";

            reader = com.ExecuteReader();
            maxLen = 1;
            Expander curExpander = null;
            StackPanel curStackPanel = null;
            while (reader.Read())
            {
                string dimTablica = reader.GetValue(0).ToString().Trim(' ');
                string nazDimSQLTablica = reader.GetValue(1).ToString().Trim(' ');
                string nazCinjSQLTablica = reader.GetValue(2).ToString().Trim(' ');
                string cinjTabKljuc = reader.GetValue(3).ToString().Trim(' ');
                string dimTabKljuc = reader.GetValue(4).ToString().Trim(' ');
                string imeSQLAtrib = reader.GetValue(5).ToString().Trim(' ');
                string imeAtrib = reader.GetValue(6).ToString().Trim(' ');

                joinedTablesData[new Tuple<string, string>(imeAtrib, dimTablica)] = new JoinedTableData(nazDimSQLTablica, nazCinjSQLTablica, cinjTabKljuc, dimTabKljuc, imeSQLAtrib);
                maxLen = maxLen < imeAtrib.Length ? imeAtrib.Length : maxLen;

                if (curExpander == null || curExpander.Header.ToString() != dimTablica)
                {
                    curStackPanel = new StackPanel() { Margin = new Thickness(30, 0, 0, 0)};
                    curExpander = new Expander() { Header = dimTablica, Content = curStackPanel, FontSize = 22, Margin = new Thickness(15, 0, 0, 0) };
                    dimensionPanel.Children.Add(curExpander);
                }
                CheckBox cb = new CheckBox() { Content = imeAtrib, VerticalContentAlignment = VerticalAlignment.Center, FontWeight = FontWeights.Normal};
                cb.Checked += AddDimension;
                cb.Unchecked += RemoveDimension;

                curStackPanel.Children.Add(cb);
            }

            fontSize = Math.Min(600 / maxLen, 20);
            foreach(Expander ex in dimensionPanel.Children)
            {
                foreach(CheckBox cb in (ex.Content as StackPanel).Children)
                {
                    cb.FontSize = fontSize;
                }
            }
            reader.Close();
        }

        private void AddMeasure(object sender, RoutedEventArgs e)
        {
            usedMeasures.Add((sender as CheckBox).Content.ToString());
            UpdateText();
        }

        private void RemoveMeasure(object sender, RoutedEventArgs e)
        {
            usedMeasures.Remove((sender as CheckBox).Content.ToString());
            UpdateText();
        }

        private void AddDimension(object sender, RoutedEventArgs e) 
        {
            usedDimensions.Add(new Tuple<string, string>((sender as CheckBox).Content.ToString(), (((sender as CheckBox).Parent as StackPanel).Parent as Expander).Header.ToString()));
            UpdateText();
        }

        private void RemoveDimension(object sender, RoutedEventArgs e)
        {
            string atribut = (sender as CheckBox).Content.ToString();
            string dimTablica = (((sender as CheckBox).Parent as StackPanel).Parent as Expander).Header.ToString();
            Tuple<string, string> toRemove = usedDimensions.Where(s => s.Item1.Equals(atribut) && s.Item2.Equals(dimTablica)).First();
            usedDimensions.Remove(toRemove);
            UpdateText();
        }

        //Execute the query
        private void SearchQuery(object sender, RoutedEventArgs e)
        {
            try
            {
                SqlCommand com = conn.CreateCommand();
                com.CommandText = testBlock.Text;
                com.ExecuteNonQuery();

                SqlDataAdapter adapter = new SqlDataAdapter(com);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                dataGrid.ItemsSource = dt.DefaultView;
                dataGrid.RowBackground = new SolidColorBrush(Color.FromRgb(190, 199, 183));
                dataGrid.FontSize = 16;
            }
            catch(Exception ex)
            {
                PopupWindow wind = new PopupWindow();
                wind.Owner = this;
                wind.ShowDialog();
            }
         }

        //Change query text whenever an attribute is added or removed
        private void UpdateText()
        {
            if(usedDimensions.Count == 0 && usedMeasures.Count == 0)
            {
                testBlock.Text = "";
                searchButton.Visibility = Visibility.Hidden;
                return;
            } 

            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");

            TableData tb;
            JoinedTableData jtd;

            for (int i = 0; i < usedMeasures.Count; i++)
            {
                tb = tablesData[usedMeasures[i]];
                sb.Append(tb.nazAgrFun);
                sb.Append("(");
                sb.Append(tb.imeSQLAtrib);
                sb.Append(") AS '");
                sb.Append(tb.imeAtrib);
                sb.Append("',\n\t");
            }

            if(usedDimensions.Count == 0)
            {
                sb.Remove(sb.Length - 3, 3);
                sb.Append("\n");
            }
            else
            {
                for (int i = 0; i < usedDimensions.Count; i++)
                {
                    jtd = joinedTablesData[usedDimensions[i]];
                    sb.Append(jtd.nazDimSQLTablica);
                    sb.Append(".");
                    sb.Append(jtd.imeSQLAtrib);
                    sb.Append(" AS '");
                    sb.Append(usedDimensions[i].Item1);
                    sb.Append("',\n\t");
                }

                sb.Remove(sb.Length - 3, 3);
                sb.Append("\n");
            }
            

            sb.Append("FROM ");
            sb.Append(currentFactTable);

            List<string> usedDimTables = new List<string>();
            for(int i = 0; i < usedDimensions.Count; i++)
            {
                jtd = joinedTablesData[usedDimensions[i]];
                if (!usedDimTables.Contains(jtd.nazDimSQLTablica))
                {
                    sb.Append(", ");
                    sb.Append(jtd.nazDimSQLTablica);
                    usedDimTables.Add(jtd.nazDimSQLTablica);
                }
                
            }

            usedDimTables.Clear();
            if(usedDimensions.Count > 0)
            {
                jtd = joinedTablesData[usedDimensions[0]];
                sb.Append("\nWHERE ");
                sb.Append(currentFactTable);
                sb.Append(".");
                sb.Append(jtd.cinjTabKljuc);
                sb.Append(" = ");
                sb.Append(jtd.nazDimSQLTablica);
                sb.Append(".");
                sb.Append(jtd.dimTabKljuc);
                usedDimTables.Add(jtd.nazDimSQLTablica);

                for(int i = 1; i < usedDimensions.Count; i++)
                {
                    jtd = joinedTablesData[usedDimensions[i]];
                    if (!usedDimTables.Contains(jtd.nazDimSQLTablica))
                    {
                        sb.Append("\n\tAND ");
                        sb.Append(currentFactTable);
                        sb.Append(".");
                        sb.Append(jtd.cinjTabKljuc);
                        sb.Append(" = ");
                        sb.Append(jtd.nazDimSQLTablica);
                        sb.Append(".");
                        sb.Append(jtd.dimTabKljuc);
                    } 
                }

                jtd = joinedTablesData[usedDimensions[0]];
                sb.Append("\nGROUP BY ");
                sb.Append(jtd.nazDimSQLTablica);
                sb.Append(".");
                sb.Append(jtd.imeSQLAtrib);

                for (int i = 1; i < usedDimensions.Count; i++)
                {
                    jtd = joinedTablesData[usedDimensions[i]];
                    sb.Append(", ");
                    sb.Append(jtd.nazDimSQLTablica);
                    sb.Append(".");
                    sb.Append(jtd.imeSQLAtrib);
                }
            }

            testBlock.Text = sb.ToString();
            searchButton.Visibility = Visibility.Visible;
        }        
    }
}
