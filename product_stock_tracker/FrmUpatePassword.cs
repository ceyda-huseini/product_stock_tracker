using System;
using System.Data.OleDb;
using System.Windows.Forms;

namespace product_stock_tracker
{
    public partial class FrmUpdatePassword : Form
    {
        public FrmUpdatePassword()
        {
            InitializeComponent();
        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            string username = tbUsername.Text.Trim();
            string currentPassword = tbCurrentPassword.Text.Trim();
            string newPassword = tbNewPassword.Text.Trim();
            string confirmPassword = tbConfirmPassword.Text.Trim();

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(currentPassword) || string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                MessageBox.Show("Please enter all fields.");
                return;
            }

            if (newPassword != confirmPassword)
            {
                MessageBox.Show("New password and confirm password do not match.");
                return;
            }

            if (!ValidateCurrentPassword(username, currentPassword))
            {
                MessageBox.Show("Invalid username or current password.");
                return;
            }

            if (currentPassword == newPassword)
            {
                MessageBox.Show("New password must be different from the current password.");
                return;
            }

            if (IsPasswordUsedBefore(username, newPassword))
            {
                MessageBox.Show("Please choose a password that you haven't used before.");
                return;
            }

            if (ChangePassword(username, newPassword))
            {
                MessageBox.Show("Password changed successfully.");
                this.Close(); 
            }
            else
            {
                MessageBox.Show("Failed to change password. Please try again.");
            }
        }

        private bool ValidateCurrentPassword(string username, string currentPassword)
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
                        command.Parameters.AddWithValue("@password", currentPassword);
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

        private bool IsPasswordUsedBefore(string username, string newPassword)
        {
            bool isUsedBefore = false;

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
                        command.Parameters.AddWithValue("@password", newPassword);
                        int count = (int)command.ExecuteScalar();
                        if (count > 0)
                        {
                            isUsedBefore = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return isUsedBefore;
        }

        private bool ChangePassword(string username, string newPassword)
        {
            bool isSuccess = false;

            try
            {
                string connectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Application.StartupPath + "\\Database1.accdb";
                string query = "UPDATE SystemUser SET [Password] = ? WHERE UserName = ?";

                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@newPassword", newPassword);
                        command.Parameters.AddWithValue("@username", username);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            isSuccess = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            return isSuccess;
        }

    }
}
