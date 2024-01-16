using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Zen.Barcode;



namespace DataGridAddStudent
{
    public partial class Form1 : Form
    {

        // create a connetion 
        MySqlConnection connection = new MySqlConnection("Server=localhost;Database=bscs3db;User=root;Password='';");

        public Form1()
        {

            InitializeComponent();

            try
            {
                connection.Open();
                string query = "SELECT * FROM studentstbl ORDER by StudentID DESC ";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                StudentsGrid.DataSource = dt;
                StudentsGrid.CellClick += DataGridView1_CellClick;

                // Set StudentID TextBox as read-only
                StudentID.ReadOnly = true;


            }
            catch (Exception ex)
            {

                MessageBox.Show("Hoy may error : " + ex.Message);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void DataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                StudentsGrid.Rows[e.RowIndex].Selected = true;

                DataGridViewRow clickedRow = StudentsGrid.Rows[e.RowIndex];
                string rowDataStudentid = clickedRow.Cells["Studentid"].Value.ToString();
                string Name = clickedRow.Cells["Name"].Value.ToString();
                string Age = clickedRow.Cells["Age"].Value.ToString();
                var IsSingle = clickedRow.Cells["IsSingle"].Value;

                // Set the values to the corresponding controls
                StudentID.Text = rowDataStudentid;
                StudentName.Text = Name;
                StudentAge.Text = Age;
                isSingleCheckBox.Checked = Convert.ToBoolean(IsSingle);

                // Display the QR code for the current student
                GenerateQRCode(rowDataStudentid, pictureBoxQRCode);

                Save.Visible = false;
                BtnUpdate.Visible = true;
                btnDelete.Enabled = true;



            }
            else
            {
                // No row selected, reset controls and toggle button visibility
                StudentID.Text = "";
                StudentName.Text = "";
                StudentAge.Text = "";
                isSingleCheckBox.Checked = false;
                pictureBoxQRCode.Image = null;

            }
        }

        private void Save_Click(object sender, EventArgs e)
        {
            try
            {
                string query = "INSERT INTO studentstbl (Name, Age, isSingle) VALUES (@name, @Age, @isSingle)";
                MySqlCommand cmd = new MySqlCommand(query, connection);

                // Set parameters
                cmd.Parameters.AddWithValue("@name", StudentName.Text);
                cmd.Parameters.AddWithValue("@Age", int.Parse(StudentAge.Text));
                cmd.Parameters.AddWithValue("@isSingle", isSingleCheckBox.Checked);

                // Execute the query
                cmd.ExecuteNonQuery();

                MessageBox.Show("Data inserted successfully.");

                StudentsGrid.DataSource = null;
                string query2 = "SELECT * FROM studentstbl ORDER by StudentID DESC ";
                MySqlDataAdapter adapter = new MySqlDataAdapter(query2, connection);
                DataTable dt = new DataTable();
                adapter.Fill(dt);
                StudentsGrid.DataSource = dt;

                StudentID.Text = "";
                StudentName.Text = "";
                StudentAge.Text = "";
                isSingleCheckBox.Checked = false;

                Save.Visible = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hoyy hndi naka insert!!!" + ex.Message);
            }

        }

        private void StudentID_TextChanged_1(object sender, EventArgs e)
        {
            if (!(string.IsNullOrEmpty(StudentName.Text)) && !(string.IsNullOrEmpty(StudentAge.Text)))
            {
                Save.Enabled = true;
                BtnUpdate.Visible = true;
                btnDelete.Enabled = true;


            }
            else
            {
                Save.Enabled = false;
                BtnUpdate.Visible = false;
                btnDelete.Enabled = false;

            }
        }

        private void StudentName_TextChanged(object sender, EventArgs e)
        {
            if (!(string.IsNullOrEmpty(StudentName.Text)) && !(string.IsNullOrEmpty(StudentAge.Text)))
            {
                Save.Visible = false;
                Save.Enabled = false;
                btnDelete.Enabled = true;
            }
            else
            {
                Save.Visible = true;
                Save.Enabled = true;
                btnDelete.Enabled = false;
            }
        }

        private void StudentAge_TextChanged(object sender, EventArgs e)
        {
            if (!(string.IsNullOrEmpty(StudentName.Text)) && !(string.IsNullOrEmpty(StudentAge.Text)))
            {
                Save.Enabled = true; // Enable Submitbtn when both textboxes have data
                BtnUpdate.Visible = true;
                btnDelete.Enabled = false;
            }
            else
            {
                Save.Enabled = false; // Disable Submitbtn if any of the textboxes is empty
                BtnUpdate.Visible = false;
                btnDelete.Enabled = false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            StudentID.Text = "";
            StudentName.Text = "";
            StudentAge.Text = "";
            isSingleCheckBox.Checked = false;
            pictureBoxQRCode.Image = null;

            Save.Visible = true;
            Save.Enabled = false;

        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to delete the selected student(s)?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (StudentsGrid.SelectedRows.Count > 0)
                {
                    foreach (DataGridViewRow row in StudentsGrid.SelectedRows)
                    {
                        int studentId = Convert.ToInt32(row.Cells["Studentid"].Value); // Replace "Studentid" with the actual column name

                        // Assuming you have a method to delete the record from the database
                        if (DeleteStudent(studentId))
                        {
                            // Remove the row from the DataGridView
                            StudentsGrid.Rows.Remove(row);

                            // Clear the textboxes
                            StudentID.Text = "";
                            StudentName.Text = "";
                            StudentAge.Text = "";
                            isSingleCheckBox.Checked = false;

                            Save.Visible = true;



                            MessageBox.Show("Student with ID " + studentId + " has been deleted successfully.");
                        }
                        else
                        {
                            MessageBox.Show("Error deleting student with ID: " + studentId);
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to delete.");
                }
            }

        }

        // Replace this method with your actual logic to delete a student from the database
        private bool DeleteStudent(int studentId)
        {
            try
            {
                using (MySqlCommand deleteCmd = new MySqlCommand("DELETE FROM Studentstbl WHERE Studentid = @Studentid", connection))
                {
                    deleteCmd.Parameters.AddWithValue("@Studentid", studentId);
                    deleteCmd.ExecuteNonQuery();
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it as needed
                Console.WriteLine("Error deleting student: " + ex.Message);
                return false;
            }
        }


        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Are you sure you want to update the selected student?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                if (StudentsGrid.SelectedRows.Count > 0)
                {
                    DataGridViewRow selectedRow = StudentsGrid.SelectedRows[0];

                    // Get the student ID from the selected row
                    int studentId = Convert.ToInt32(selectedRow.Cells["Studentid"].Value);

                    try
                    {
                        // Open a new connection for the update operation
                        using (MySqlConnection updateConnection = new MySqlConnection("Server=localhost;Database=bscs3db;User=root;Password='';"))
                        {
                            updateConnection.Open();

                            // Update query
                            string updateQuery = "UPDATE Studentstbl SET Name = @Name, Age = @Age, IsSingle = @IsSingle WHERE Studentid = @Studentid";
                            MySqlCommand updateCmd = new MySqlCommand(updateQuery, updateConnection);

                            // Set parameters
                            updateCmd.Parameters.AddWithValue("@Name", StudentName.Text);
                            updateCmd.Parameters.AddWithValue("@Age", int.Parse(StudentAge.Text));
                            updateCmd.Parameters.AddWithValue("@IsSingle", isSingleCheckBox.Checked);
                            updateCmd.Parameters.AddWithValue("@Studentid", studentId);

                            // Execute the update query
                            updateCmd.ExecuteNonQuery();

                            // Refresh the DataGridView
                            StudentsGrid.DataSource = null;
                            string selectQuery = "SELECT * FROM studentstbl ORDER BY Studentid DESC";
                            MySqlDataAdapter adapter = new MySqlDataAdapter(selectQuery, updateConnection);
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            StudentsGrid.DataSource = dt;

                            // Clear the textboxes
                            StudentID.Text = "";
                            StudentName.Text = "";
                            StudentAge.Text = "";
                            isSingleCheckBox.Checked = false;

                            Save.Visible = true;


                            MessageBox.Show("Data Updated Successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error updating student: " + ex.Message);
                    }
                }
                else
                {
                    MessageBox.Show("Please select a row to update.");
                }
            }
        }

        private void GenerateQRCode(int studentID)
        {
            Zen.Barcode.CodeQrBarcodeDraw qrcode = Zen.Barcode.BarcodeDrawFactory.CodeQr;
            pictureBoxQRCode.Image = qrcode.Draw(studentID.ToString(), 50);
        }


        private void GenerateQRCode(string data, PictureBox pictureBox)
        {
            try
            {
                // Use the Zen Barcode Framework to create a QR code
                Zen.Barcode.CodeQrBarcodeDraw qrCode = Zen.Barcode.BarcodeDrawFactory.CodeQr;

                // Adjust the size of the QR code based on the PictureBox dimensions
                int pictureBoxSize = Math.Min(pictureBox.Width, pictureBox.Height);

                // Draw the QR code and set it as the PictureBox's image
                pictureBox.Image = qrCode.Draw(data, pictureBoxSize);

                pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox.Image = pictureBox.Image;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error generating QR code: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                {
                    if (!string.IsNullOrEmpty(StudentID.Text))
                    {
                        // Assuming the student ID is stored in studentidtextbox.Text
                        int studentId = Convert.ToInt32(StudentName.Text);

                    }
                    else
                    {
                        MessageBox.Show("Invalid QR code. Please select a student first.");
                    }

                }
            }
        }
    }
}