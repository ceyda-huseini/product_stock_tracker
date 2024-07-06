using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace product_stock_tracker
{
    public partial class FrmLogin : Form
    {
        public FrmLogin()
        {
            InitializeComponent();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = tbUsername.Text.Trim();
            string password = tbPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.");
                return;
            }

            if (ValidateUser(username, password))
            {
                MessageBox.Show("Login successful.");
                FrmHomePage frmHomePage = new FrmHomePage();
                frmHomePage.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }
        }

        private bool ValidateUser(string username, string password)
        {
            bool isValid = false;

            try
            {
                
                string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb";

                
                string query = "SELECT COUNT(*) FROM SystemUser WHERE UserName = @username AND Password = @password";


                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@username", username);
                        command.Parameters.AddWithValue("@password", password);
                        int count = (int)command.ExecuteScalar();
                        if (count > 0)
                        {
                            isValid = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return isValid;
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            FrmUpdatePassword frmUpdatePassword = new FrmUpdatePassword();
            frmUpdatePassword.ShowDialog();
        }
    }
}
