using System;
using System.Data;
using System.Data.OleDb;
using System.Windows.Forms;

namespace product_stock_tracker
{
    public partial class FrmCategory : Form
    {
        public FrmCategory()
        {
            InitializeComponent();
            this.Load += new EventHandler(FrmCategory_Load);
            this.listView1.SelectedIndexChanged += new EventHandler(listView1_SelectedIndexChanged);
        }

        private void FrmCategory_Load(object sender, EventArgs e)
        {
            InitializeListView();
            LoadData();
        }

        private void InitializeListView()
        {
            listView1.View = View.Details;
            listView1.FullRowSelect = true;
            listView1.Columns.Clear();
            listView1.Columns.Add("CategoryID", 0);
            listView1.Columns.Add("CategoryName", 150);
        }

        private void LoadData()
        {
            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb"))
                {
                    oleDbConnection.Open();
                    OleDbCommand command = new OleDbCommand("SELECT * FROM Category WHERE IsDeleted = FALSE", oleDbConnection);
                    OleDbDataReader reader = command.ExecuteReader();
                    listView1.Items.Clear();

                    while (reader.Read())
                    {
                        ListViewItem item = new ListViewItem(reader["CategoryID"].ToString());
                        item.SubItems.Add(reader["CategoryName"].ToString());
                        listView1.Items.Add(item);
                    }
                    oleDbConnection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
            {
                MessageBox.Show("Category Name cannot be empty.");
                return;
            }

            try
            {
                using (OleDbConnection oleDbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb"))
                {
                    oleDbConnection.Open();
                    OleDbCommand oleDbCommand = new OleDbCommand("INSERT INTO Category (CategoryName, IsDeleted) VALUES (@CategoryName, FALSE)", oleDbConnection);
                    oleDbCommand.Parameters.AddWithValue("@CategoryName", txtCategoryName.Text);
                    oleDbCommand.ExecuteNonQuery();
                    oleDbConnection.Close();
                }
                LoadData();
                txtCategoryName.Clear();
                MessageBox.Show("Record added successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                if (string.IsNullOrWhiteSpace(txtCategoryName.Text))
                {
                    MessageBox.Show("Category Name cannot be empty.");
                    return;
                }

                DialogResult result = MessageBox.Show("Are you sure you want to update this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (OleDbConnection oleDbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb"))
                        {
                            oleDbConnection.Open();
                            OleDbCommand oleDbCommand = new OleDbCommand("UPDATE Category SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID", oleDbConnection);
                            oleDbCommand.Parameters.AddWithValue("@CategoryName", txtCategoryName.Text);
                            oleDbCommand.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(listView1.SelectedItems[0].Text));
                            oleDbCommand.ExecuteNonQuery();
                            oleDbConnection.Close();
                        }
                        LoadData();
                        txtCategoryName.Clear();
                        MessageBox.Show("Record updated successfully.");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }

                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                DialogResult result = MessageBox.Show("Are you sure you want to delete this record?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        using (OleDbConnection oleDbConnection = new OleDbConnection(@"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb"))
                        {
                            oleDbConnection.Open();
                            OleDbCommand oleDbCommand = new OleDbCommand("UPDATE Category SET IsDeleted = TRUE WHERE CategoryID = @CategoryID", oleDbConnection);
                            oleDbCommand.Parameters.AddWithValue("@CategoryID", Convert.ToInt32(listView1.SelectedItems[0].Text));
                            oleDbCommand.ExecuteNonQuery();
                            oleDbConnection.Close();
                        }
                        LoadData();
                        txtCategoryName.Clear();
                        MessageBox.Show("Record marked as deleted.");

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtCategoryName.Clear();
           
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                txtCategoryName.Text = listView1.SelectedItems[0].SubItems[1].Text;
            }
            
        }
    }
}
