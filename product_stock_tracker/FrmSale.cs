using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace product_stock_tracker
{
    public partial class FrmSale : Form
    {
        private string connectionString;

        public FrmSale()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmSale_Load);
            this.listViewSales.SelectedIndexChanged += new EventHandler(listViewSales_SelectedIndexChanged);
            this.btnCalculate.Click += new EventHandler(btnCalculate_Click);
            this.btnAdd.Click += new EventHandler(btnAdd_Click);
            this.btnUpdate.Click += new EventHandler(btnUpdate_Click);
            this.btnDelete.Click += new EventHandler(btnDelete_Click);
            this.btnClear.Click += new EventHandler(btnClear_Click);
            this.dateTimePickerSaleDate.Validating += new System.ComponentModel.CancelEventHandler(this.dateTimePickerSaleDate_Validating);
            dateTimePickerSaleDate.Value = DateTime.Today;
            connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb";
        }

        private void FrmSale_Load(object sender, EventArgs e)
        {
            InitializeListView();
            LoadProducts();
            LoadSales();
        }

        private void InitializeListView()
        {
            listViewSales.View = View.Details;
            listViewSales.FullRowSelect = true;
            listViewSales.Columns.Clear();
            listViewSales.Columns.Add("SaleID", 0);
            listViewSales.Columns.Add("Product Name", 150);
            listViewSales.Columns.Add("Seller Name", 150);
            listViewSales.Columns.Add("Sale Date", 100);
            listViewSales.Columns.Add("Sale Quantity", 70);
            listViewSales.Columns.Add("Total Amount", 100);
        }

        private void LoadProducts()
        {
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("SELECT ProductID, ProductName, Price FROM Product WHERE IsDeleted = FALSE", oleDbConnection);
                    OleDbDataReader reader = command.ExecuteReader();

                    cbProduct.Items.Clear();
                    while (reader.Read())
                    {
                        cbProduct.Items.Add(new ComboBoxItem2(reader["ProductName"].ToString(), reader["ProductID"].ToString(), reader["Price"].ToString()));
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void LoadSales()
        {
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("SELECT Sale.SaleID, Product.ProductName, Sale.SellerName, Sale.SaleDate, Sale.Quantity, Sale.TotalAmount FROM Sale INNER JOIN Product ON Sale.ProductID = Product.ProductID WHERE Sale.IsDeleted = FALSE", oleDbConnection);
                    OleDbDataReader reader = command.ExecuteReader();

                    listViewSales.Items.Clear();
                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(reader["SaleID"].ToString());
                        item.SubItems.Add(reader["ProductName"].ToString());
                        item.SubItems.Add(reader["SellerName"].ToString());
                        item.SubItems.Add(Convert.ToDateTime(reader["SaleDate"]).ToString("yyyy-MM-dd"));
                        item.SubItems.Add(reader["Quantity"].ToString());
                        item.SubItems.Add(((decimal)reader["TotalAmount"]).ToString("C")); 
                        listViewSales.Items.Add(item);
                    }

                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                if (!int.TryParse(tbQuantity.Text, out int quantity) || quantity < 1)
                {
                    return;
                }

                var selectedProduct = (ComboBoxItem2)cbProduct.SelectedItem;
                if (selectedProduct == null)
                {
                    MessageBox.Show("Please select a product.");
                    return;
                }

                if (!IsStockAvailable(selectedProduct.Value, quantity))
                {
                    MessageBox.Show("Insufficient stock for the selected product.");
                    return;
                }

                decimal price;
                if (!decimal.TryParse(selectedProduct.Price, out price))
                {
                    MessageBox.Show("Invalid price format for the selected product.");
                    return;
                }
                decimal totalAmount = price * quantity;

                using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("INSERT INTO Sale (ProductID, SellerName, SaleDate, Quantity, TotalAmount, IsDeleted) VALUES (@ProductID, @SellerName, @SaleDate, @Quantity, @TotalAmount, FALSE)", oleDbConnection);
                    command.Parameters.AddWithValue("@ProductID", selectedProduct.Value);
                    command.Parameters.AddWithValue("@SellerName", tbSellerName.Text);
                    command.Parameters.AddWithValue("@SaleDate", dateTimePickerSaleDate.Value);
                    command.Parameters.AddWithValue("@Quantity", quantity);
                    command.Parameters.AddWithValue("@TotalAmount", totalAmount);

                    command.ExecuteNonQuery();
                }

                UpdateStockQuantity(selectedProduct.Value, -quantity);

                LoadSales();
                ClearFields();
                MessageBox.Show("Sale added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding sale: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (listViewSales.SelectedItems.Count > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to update this sale?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var selectedProduct = (ComboBoxItem2)cbProduct.SelectedItem;
                        int newQuantity = Convert.ToInt32(tbQuantity.Text);
                        int currentQuantity = Convert.ToInt32(listViewSales.SelectedItems[0].SubItems[4].Text);
                        int quantityDifference = newQuantity - currentQuantity;

                        decimal price = Convert.ToDecimal(selectedProduct.Price);
                        decimal totalAmount = price * newQuantity;

                        using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                        {
                            oleDbConnection.Open();
                            OleDbCommand command = new OleDbCommand("UPDATE Sale SET ProductID = @ProductID, SellerName = @SellerName, SaleDate = @SaleDate, Quantity = @Quantity, TotalAmount = @TotalAmount WHERE SaleID = @SaleID", oleDbConnection);
                            command.Parameters.AddWithValue("@ProductID", selectedProduct.Value);
                            command.Parameters.AddWithValue("@SellerName", tbSellerName.Text);
                            command.Parameters.AddWithValue("@SaleDate", dateTimePickerSaleDate.Value);
                            command.Parameters.AddWithValue("@Quantity", newQuantity);
                            command.Parameters.AddWithValue("@TotalAmount", totalAmount);
                            command.Parameters.AddWithValue("@SaleID", Convert.ToInt32(listViewSales.SelectedItems[0].Text));

                            command.ExecuteNonQuery();
                        }

                        UpdateStockQuantity(selectedProduct.Value, -quantityDifference);

                        LoadSales();
                        ClearFields();
                        MessageBox.Show("Sale updated successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating sale: " + ex.Message);
                    }
                }
            }
        }


        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listViewSales.SelectedItems.Count > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this sale?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        var selectedProduct = (ComboBoxItem2)cbProduct.SelectedItem;
                        int quantity = Convert.ToInt32(listViewSales.SelectedItems[0].SubItems[4].Text); 

                        using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                        {
                            oleDbConnection.Open();
                            OleDbCommand command = new OleDbCommand("UPDATE Sale SET IsDeleted = TRUE WHERE SaleID = @SaleID", oleDbConnection);
                            command.Parameters.AddWithValue("@SaleID", Convert.ToInt32(listViewSales.SelectedItems[0].Text));

                            command.ExecuteNonQuery();
                        }

                        UpdateStockQuantity(selectedProduct.Value, quantity);

                        LoadSales();
                        ClearFields();

                        listViewSales.SelectedItems.Clear();

                        MessageBox.Show("Sale deleted successfully.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting sale: " + ex.Message);
                    }
                }
            }
        }




        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            cbProduct.SelectedIndex = -1;
            tbSellerName.Clear();
            tbQuantity.Clear();
            lblTotalAmountResult.Text = "";
            dateTimePickerSaleDate.Value = DateTime.Now;
        }

        private void listViewSales_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listViewSales.SelectedItems.Count > 0)
            {
                var selectedItem = listViewSales.SelectedItems[0];
                cbProduct.SelectedIndex = cbProduct.FindStringExact(selectedItem.SubItems[1].Text);
                tbSellerName.Text = selectedItem.SubItems[2].Text;
                dateTimePickerSaleDate.Value = Convert.ToDateTime(selectedItem.SubItems[3].Text);
                tbQuantity.Text = selectedItem.SubItems[4].Text;
                lblTotalAmountResult.Text = selectedItem.SubItems[5].Text;
            }
        }

        private void btnCalculate_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tbSellerName.Text) && cbProduct.SelectedIndex != -1 && !string.IsNullOrWhiteSpace(tbQuantity.Text) && IsValidQuantity(tbQuantity.Text))
            {
                try
                {
                    var selectedProduct = (ComboBoxItem2)cbProduct.SelectedItem;
                    decimal price = Convert.ToDecimal(selectedProduct.Price);
                    int quantity = Convert.ToInt32(tbQuantity.Text);
                    decimal totalAmount = price * quantity;
                    lblTotalAmountResult.Text = totalAmount.ToString("C");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error calculating total amount: " + ex.Message);
                }
            }
        }

        private bool IsStockAvailable(string productId, int requestedQuantity)
        {
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("SELECT StockQuantity FROM Product WHERE ProductID = @ProductID", oleDbConnection);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    int currentStock = (int)command.ExecuteScalar();

                    return currentStock >= requestedQuantity;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error checking stock availability: " + ex.Message);
                return false;
            }
        }

        private bool IsValidQuantity(string quantityText)
        {
            if (int.TryParse(quantityText, out int quantity))
            {
                return quantity >= 1;
            }
            return false;
        }

        private void UpdateStockQuantity(string productId, int quantityDifference)
        {
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(connectionString))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("UPDATE Product SET StockQuantity = StockQuantity + @QuantityDifference WHERE ProductID = @ProductID", oleDbConnection);
                    command.Parameters.AddWithValue("@QuantityDifference", quantityDifference);
                    command.Parameters.AddWithValue("@ProductID", productId);
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating stock quantity: " + ex.Message);
            }
        }

        private void dateTimePickerSaleDate_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            DateTime selectedDate = dateTimePickerSaleDate.Value;
            DateTime today = DateTime.Today;

            if (selectedDate > today)
            {
                errorProvider1.SetError(dateTimePickerSaleDate, "The sale date cannot be in the future.");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(dateTimePickerSaleDate, null);
                e.Cancel = false;
            }
        }

        private void cbProduct_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (cbProduct.SelectedItem == null || string.IsNullOrWhiteSpace(cbProduct.Text))
            {
                errorProvider1.SetError(cbProduct, "Please select a product.");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(cbProduct, null);
                e.Cancel = false;
            }
        }

        private void tbQuantity_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!IsValidQuantity(tbQuantity.Text))
            {
                errorProvider1.SetError(tbQuantity, "Please enter a valid quantity (minimum 1).");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(tbQuantity, null);
                e.Cancel = false;
            }
        }

        private void tbSellerName_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbSellerName.Text))
            {
                errorProvider1.SetError(tbSellerName, "Please enter the seller's name.");
                e.Cancel = true;
            }
            else
            {
                errorProvider1.SetError(tbSellerName, null);
                e.Cancel = false;
            }
        }

    }

    public class ComboBoxItem2
    {
        public string Text { get; set; }
        public string Value { get; set; }
        public string Price { get; set; }

        public ComboBoxItem2(string text, string value, string price)
        {
            Text = text;
            Value = value;
            Price = price;
        }

        public override string ToString()
        {
            return Text;
        }
    }
}
