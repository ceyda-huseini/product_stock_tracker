using System;
using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Windows.Forms;

namespace product_stock_tracker
{
    public partial class FrmProduct : Form
    {
        private OleDbConnection oleDbConnection;

        public FrmProduct()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmProduct_Load);
            this.listViewProducts.SelectedIndexChanged += new EventHandler(listViewProducts_SelectedIndexChanged);
            oleDbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb");
        }

        private void FrmProduct_Load(object sender, EventArgs e)
        {
            InitializeListView();
            LoadCategories();
            LoadProducts();
        }

        private void InitializeListView()
        {
            listViewProducts.View = View.Details;
            listViewProducts.FullRowSelect = true;
            listViewProducts.Columns.Clear();
            listViewProducts.Columns.Add("ProductID", 0);
            listViewProducts.Columns.Add("Product Name", 150);
            listViewProducts.Columns.Add("Description", 150);
            listViewProducts.Columns.Add("Price", 70);
            listViewProducts.Columns.Add("Stock Quantity", 70);
            listViewProducts.Columns.Add("Brand", 100);
            listViewProducts.Columns.Add("Category Name", 100);
           // listViewProducts.Columns.Add("CategoryID", 70);
        }

        private void LoadCategories()
        {
            try
            {
                oleDbConnection.Open();
                OleDbCommand command = new OleDbCommand("SELECT * FROM Category WHERE IsDeleted = FALSE", oleDbConnection);
                OleDbDataReader reader = command.ExecuteReader();

                cbCategories.Items.Clear();
                while (reader.Read())
                {
                    cbCategories.Items.Add(new ComboBoxItem(reader["CategoryName"].ToString(), reader["CategoryID"].ToString()));
                }

                reader.Close();
                oleDbConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }
        private void LoadProducts()
        {
            try
            {
                oleDbConnection.Open();
                OleDbCommand command = new OleDbCommand("SELECT Product.ProductID, Product.ProductName, Product.Description, Product.Price, Product.StockQuantity, Product.Brand, Category.CategoryName " +
                                                        "FROM Product " +
                                                        "INNER JOIN Category ON Product.CategoryID = Category.CategoryID " +
                                                        "WHERE Product.IsDeleted = FALSE", oleDbConnection);
                OleDbDataReader reader = command.ExecuteReader();

                listViewProducts.Items.Clear();
                while (reader.Read())
                {
                    ListViewItem item = new ListViewItem(reader["ProductID"].ToString());
                    item.SubItems.Add(reader["ProductName"].ToString());
                    item.SubItems.Add(reader["Description"].ToString());
                    item.SubItems.Add(reader["Price"].ToString());
                    item.SubItems.Add(reader["StockQuantity"].ToString());
                    item.SubItems.Add(reader["Brand"].ToString());
                    item.SubItems.Add(reader["CategoryName"].ToString()); 
                    listViewProducts.Items.Add(item);
                }

                reader.Close();
                oleDbConnection.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbProductName.Text) || string.IsNullOrWhiteSpace(tbPrice.Text) || string.IsNullOrWhiteSpace(tbQuantity.Text) || cbCategories.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            try
            {
                oleDbConnection.Open();
                OleDbCommand command = new OleDbCommand("INSERT INTO Product (ProductName, Description, Price, StockQuantity, Brand, CategoryID, IsDeleted) VALUES (@ProductName, @Description, @Price, @StockQuantity, @Brand, @CategoryID, FALSE)", oleDbConnection);
                command.Parameters.AddWithValue("@ProductName", tbProductName.Text);
                command.Parameters.AddWithValue("@Description", tbDescription.Text);
                command.Parameters.AddWithValue("@Price", Convert.ToDecimal(tbPrice.Text));
                command.Parameters.AddWithValue("@StockQuantity", Convert.ToInt32(tbQuantity.Text));
                command.Parameters.AddWithValue("@Brand", tbBrand.Text);
                command.Parameters.AddWithValue("@CategoryID", ((ComboBoxItem)cbCategories.SelectedItem).Value);

                command.ExecuteNonQuery();
                oleDbConnection.Close();

                LoadProducts();
                ClearFields();
                MessageBox.Show("Product added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (listViewProducts.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a product to update.");
                return;
            }

            if (string.IsNullOrWhiteSpace(tbProductName.Text) || string.IsNullOrWhiteSpace(tbPrice.Text) || string.IsNullOrWhiteSpace(tbQuantity.Text) || cbCategories.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all required fields.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to update this product?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("UPDATE Product SET ProductName = @ProductName, Description = @Description, Price = @Price, StockQuantity = @StockQuantity, Brand = @Brand, CategoryID = @CategoryID WHERE ProductID = @ProductID", oleDbConnection);
                    command.Parameters.AddWithValue("@ProductName", tbProductName.Text);
                    command.Parameters.AddWithValue("@Description", tbDescription.Text);
                    command.Parameters.AddWithValue("@Price", Convert.ToDecimal(tbPrice.Text));
                    command.Parameters.AddWithValue("@StockQuantity", Convert.ToInt32(tbQuantity.Text));
                    command.Parameters.AddWithValue("@Brand", tbBrand.Text);
                    command.Parameters.AddWithValue("@CategoryID", ((ComboBoxItem)cbCategories.SelectedItem).Value);
                    command.Parameters.AddWithValue("@ProductID", Convert.ToInt32(listViewProducts.SelectedItems[0].Text));

                    command.ExecuteNonQuery();
                    oleDbConnection.Close();

                    LoadProducts();
                    ClearFields();
                    MessageBox.Show("Product updated successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewProducts.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a product to delete.");
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this product?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                try
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("UPDATE Product SET IsDeleted = TRUE WHERE ProductID = @ProductID", oleDbConnection);
                    command.Parameters.AddWithValue("@ProductID", Convert.ToInt32(listViewProducts.SelectedItems[0].Text));

                    command.ExecuteNonQuery();
                    oleDbConnection.Close();

                    LoadProducts();
                    ClearFields();
                    MessageBox.Show("Product deleted successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            tbProductName.Clear();
            tbDescription.Clear();
            tbPrice.Clear();
            tbQuantity.Clear();
            tbBrand.Clear();
            cbCategories.SelectedIndex = -1;
        }
        private void listViewProducts_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewProducts.SelectedItems.Count > 0)
            {
                tbProductName.Text = listViewProducts.SelectedItems[0].SubItems[1].Text;
                tbDescription.Text = listViewProducts.SelectedItems[0].SubItems[2].Text;
                tbPrice.Text = listViewProducts.SelectedItems[0].SubItems[3].Text;
                tbQuantity.Text = listViewProducts.SelectedItems[0].SubItems[4].Text;
                tbBrand.Text = listViewProducts.SelectedItems[0].SubItems[5].Text;

                string selectedCategoryName = listViewProducts.SelectedItems[0].SubItems[6].Text;
                for (int i = 0; i < cbCategories.Items.Count; i++)
                {
                    if (((ComboBoxItem)cbCategories.Items[i]).Text == selectedCategoryName)
                    {
                        cbCategories.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void tbQuantity_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            int value;
            if (int.TryParse(tbQuantity.Text, out value) && value > 0)
            {
                errorProvider1.SetError(tbQuantity, null);
                e.Cancel = false;
            }
            else
            {
                errorProvider1.SetError(tbQuantity, "Please enter a valid positive integer.");
                e.Cancel = true;
            }
        }

        private void tbPrice_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            decimal value;
            if (decimal.TryParse(tbPrice.Text, NumberStyles.Currency, CultureInfo.CurrentCulture, out value) && value > 0)
            {
                errorProvider1.SetError(tbPrice, null);
                e.Cancel = false;
            }
            else
            {
              
                errorProvider1.SetError(tbPrice, "Please enter a valid positive currency amount."); 
                e.Cancel = true;
            }
        }
    }

    public class ComboBoxItem
    {
        public string Text { get; set; }
        public string Value { get; set; }

        public ComboBoxItem(string text, string value)
        {
            Text = text;
            Value = value;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
