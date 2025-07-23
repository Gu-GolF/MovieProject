using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;

namespace MovieProject
{
    public partial class FrmMovie : Form
    {
        // สร้างตัวแปรเก็บรูปภาพของภาพยนต์
        string connectionString = @"Server=DESKTOP-HMDSMC8\SQLEXPRESS;Database=movie_collection_db;Trusted_Connection=True";

        // สร้างตัวแปรเก็บรูปที่แปลงเป็น byte array ลง DB
        byte[] movieImage;
        byte[] movieDirectorImage;

        int movieId = 0; // Variable to member ID

        public FrmMovie()
        {
            InitializeComponent();
        }

        private void resetForm()
        {

            btDeleteMovie.Enabled = false;
            btUpdateMovie.Enabled = false;
            btSaveMovie.Enabled = true;
            lbMovieId.Text = "";
            tbMovieName.Text = "";
            tbMovieDetail.Text = "";
            tbMovieDirectorName.Text = "";
            dtpMovieDate.Value = DateTime.Now;
            nudMovieHour.Value = 0;
            nudMovieMinute.Value = 0;
            cbbMovieType.SelectedIndex = 0; // ล้างการเลือกประเภทภาพยนต์
            pcbMovieImage.Image = null; // ล้างรูปภาพของภาพยนต์
            pcbMovieDirectorImage.Image = null; // ล้างรูปภาพของผู้กำกับภาพยนต์


        }
        private Image convertByteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
            {
                return null;
            }
            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    return Image.FromStream(ms);
                }
            }
            catch (ArgumentException ex)
            {
                // อาจเกิดขึ้นถ้า byte array ไม่ใช่ข้อมูลรูปภาพที่ถูกต้อง
                Console.WriteLine("Error converting byte array to image: " + ex.Message);
                return null;
            }
        }

        private byte[] convertImageToByteArray(Image image, ImageFormat ImageFormat)
        {
            if (image == null)
                return null;

            using (MemoryStream ms = new MemoryStream())
            {
                using (Bitmap bmp = new Bitmap(image)) // 🔒 Clone ภาพ!
                {
                    bmp.Save(ms, ImageFormat);
                }
                return ms.ToArray();
            }
        }


        private void getAllMoiveToListView()
        {

            // Connect String เพื่อเชื่อมต่อฐานข้อมูล ตามยี่ห้อของฐานข้อมูลที่ใช้
            //string connectionString = @"Server=DESKTOP-9U4FO0V\SQLEXPRESS;Database=coffee_cafe_db;Trusted_Connection=True";
            // Create connection object ไปยังฐานข้อมูลที่ต้องการ
            using (SqlConnection sqlConnection = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConnection.Open(); // เปิดการเชื่อมต่อกับฐานข้อมูล

                    // SELECT, INSERT, UPDATE, DELETE
                    // สร้างคำสั่ง SQL เพื่อดึงข้อมูลจากตาราง product_tb
                    string strSQL = "SELECT movieId, movieImage, movieName, movieDetail, movieDate, movieType, movieDirectorName FROM movie_tb";

                    // สร้าง SqlCommand เพื่อรันคำสั่ง SQL
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(strSQL, sqlConnection))
                    {
                        // สร้าง DataTable แปลงจากเป็นก้อนมาเป็นตาราง
                        DataTable dataTable = new DataTable();
                        dataAdapter.Fill(dataTable);

                        // ตั้งค่าทั่วไปของ All Movie ListView
                        lvShowAllMovie.Items.Clear(); // ล้างข้อมูลเก่าใน ListView
                        lvShowAllMovie.Columns.Clear(); // ล้างคอลัมน์เก่าใน ListView
                        lvShowAllMovie.FullRowSelect = true; // เลือกแถวทั้งหมดเมื่อคลิกที่แถวใดแถวหนึ่ง
                        lvShowAllMovie.View = View.Details; // ตั้งค่าให้แสดงผลแบบรายละเอียด

                        // ตั้งค่าทั่วไปของ Search Movie ListView
                        lvShowSearchMovie.Items.Clear(); // ล้างข้อมูลเก่าใน ListView
                        lvShowSearchMovie.Columns.Clear(); // ล้างคอลัมน์เก่าใน ListView
                        lvShowSearchMovie.FullRowSelect = true; // เลือกแถวทั้งหมดเมื่อคลิกที่แถวใดแถวหนึ่ง
                        lvShowSearchMovie.View = View.Details; // ตั้งค่าให้แสดงผลแบบรายละเอียด

                        // ตั้งค่าการแสดงรูปใน ListView
                        if (lvShowAllMovie.SmallImageList == null)
                        {
                            lvShowAllMovie.SmallImageList = new ImageList();
                            lvShowAllMovie.SmallImageList.ImageSize = new Size(80, 80); // กำหนดขนาดของรูปภาพ
                            lvShowAllMovie.SmallImageList.ColorDepth = ColorDepth.Depth32Bit; // กำหนดความลึกของสี
                        }
                        lvShowAllMovie.SmallImageList.Images.Clear(); // ล้างรูปภาพเก่าใน ImageList

                        // กำหนดรายละเอียดของ Column ใน ListView
                        lvShowAllMovie.Columns.Add("รูปภาพยนตร์", 120, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("ฃื่อภาพยนตร์", 200, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("ชื่อผู้กำกับ", 100, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("วันที่เข้าฉาย", 120, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowAllMovie.Columns.Add("ประเภทภาพยนตร์", 120, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่

                        // กำหนดรายละเอียดของ Column ใน Search ListView
                        lvShowSearchMovie.Columns.Add("รหัสภาพยนตร์", 80, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่
                        lvShowSearchMovie.Columns.Add("ฃื่อภาพยนตร์", 200, HorizontalAlignment.Left); // เพิ่มคอลัมน์ใหม่


                        // LOOP เพื่อเพิ่มข้อมูลจาก DataTable ลงใน ListView
                        foreach (DataRow dataRow in dataTable.Rows)
                        {
                            ListViewItem item = new ListViewItem(); // สร้าง item เก็บข้อมูลแต่ละรายการ
                            item.Tag = dataRow["movieId"];
                            Image movieImage = null; // ตัวแปรสำหรับเก็บรูปภาพ
                            if (dataRow["movieImage"] != DBNull.Value)
                            {
                                byte[] imgByte = (byte[])dataRow["movieImage"];
                                // แปลงข้อมูลรูปภาพจากฐานข้อมูลเป็น byte array
                                movieImage = convertByteArrayToImage(imgByte); // แปลง byte array เป็น Image
                            }

                            string imagekey = null;// ตัวแปรสำหรับเก็บ key ของรูปภาพ
                            if (movieImage != null)
                            {
                                imagekey = $"movie_{dataRow["movieId"]}"; // สร้าง key สำหรับรูปภาพ
                                lvShowAllMovie.SmallImageList.Images.Add(imagekey, movieImage); // เพิ่มรูปภาพลงใน ImageList
                                item.ImageKey = imagekey; // กำหนด key ของรูปภาพให้กับ item
                            }
                            else
                            {
                                item.ImageIndex = -1;
                            }
                            //เพิ่มรายการลงใน item ตามข้อมูลใน DataRow

                            item.SubItems.Add(dataRow["movieName"].ToString());
                            item.SubItems.Add(dataRow["movieDirectorName"].ToString());
                            item.SubItems.Add(Convert.ToDateTime(dataRow["movieDate"]).ToString("dd/MM/yyyy"));
                            item.SubItems.Add(dataRow["movieType"].ToString());

                            // เพิ่ม item ลงใน ListView
                            lvShowAllMovie.Items.Add(item);

                        }
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถเชื่อมต่อฐานข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                }
            }
        }

        private void FrmMovie_Load(object sender, System.EventArgs e)
        {
            resetForm();
            getAllMoiveToListView();
            lvShowAllMovie.DoubleClick += lvShowAllMovie_DoubleClick;

            // เรียกใช้เมธอดเพื่อดึงข้อมูลภาพยนต์ทั้งหมดมาแสดงใน ListView
        }

        private void showWarningMessage(string message)
        {
            MessageBox.Show(message, "คำเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private void btSaveMovie_Click(object sender, EventArgs e)
        {
            if (tbMovieName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อภาพยนตร์");
            }
            else if (tbMovieDetail.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกรายละเอียดภาพยนตร์");
            }
            else if (nudMovieHour.Value == 0 && nudMovieMinute.Value == 0)
            {
                showWarningMessage("กรุณากรอกเวลาความยาวของภาพยนตร์");
            }
            else if (pcbMovieImage == null)
            {
                showWarningMessage("กรุณาเลือกรูปภาพของภาพยนตร์");
            }
            else if (pcbMovieDirectorImage == null)
            {
                showWarningMessage("กรุณาเลือกรูปภาพของผู้กำกับภาพยนตร์");
            }
            else if (tbMovieDirectorName.Text.Length == 0)
            {
                showWarningMessage("กรุณากรอกชื่อผู้กำกับ");
            }
            else
            {
                using (SqlConnection sqlConnection = new SqlConnection(connectionString))
                {
                    try
                    {
                        sqlConnection.Open(); // เปิดการเชื่อมต่อกับฐานข้อมูล

                        // For Insert, Update, Delete
                        SqlTransaction sqlTransaction = sqlConnection.BeginTransaction();

                        string strSQL = "INSERT INTO movie_tb (movieName, movieDetail, movieDate, movieHour, movieMinute, movieType, movieImage, movieDirectorImage, movieDirectorName) " +
                                        "VALUES (@movieName, @movieDetail, @movieDate, @movieHour, @movieMinute, @movieType, @movieImage, @movieDirectorImage, @movieDirectorName)";

                        using (SqlCommand sqlCommand = new SqlCommand(strSQL, sqlConnection, sqlTransaction))
                        {
                            sqlCommand.Parameters.Add("@movieName", SqlDbType.NVarChar, 150).Value = tbMovieName.Text;
                            sqlCommand.Parameters.Add("@movieDetail", SqlDbType.NVarChar, 500).Value = tbMovieDetail.Text;
                            sqlCommand.Parameters.Add("@movieDate", SqlDbType.Date).Value = dtpMovieDate.Value;
                            sqlCommand.Parameters.Add("@movieHour", SqlDbType.Int).Value = nudMovieHour.Value;
                            sqlCommand.Parameters.Add("@movieMinute", SqlDbType.Int).Value = nudMovieMinute.Value;
                            sqlCommand.Parameters.Add("@movieType", SqlDbType.NVarChar, 150).Value = cbbMovieType.SelectedItem;
                            sqlCommand.Parameters.Add("@movieImage", SqlDbType.Image).Value = movieImage;
                            sqlCommand.Parameters.Add("@movieDirectorImage", SqlDbType.Image).Value = movieDirectorImage;
                            sqlCommand.Parameters.Add("@movieDirectorName", SqlDbType.NVarChar, 150).Value = tbMovieDirectorName.Text;

                            // รันคำสั่ง SQL
                            sqlCommand.ExecuteNonQuery();
                            sqlTransaction.Commit();

                            MessageBox.Show("บันทึกข้อมูลภาพยนตร์เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            getAllMoiveToListView();
                            resetForm();
                        }

                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถบันทึกข้อมูลได้ กรุณาลองใหม่หรือติดต่อ IT\n" + ex.Message);
                    }
                }


            }
        }

        private void btMovieImage_Click(object sender, EventArgs e)
        {
            // open file dialog เพื่อเลือกไฟล์รูปภาพ jpg, png
            // ถ้าเลือกไฟล์ได้ ให้แสดงรูปภาพใน pcbMovieImage
            // แปลงเป็น byte array เก็บไว้ในตัวแปรเพื่อใช้ในการบันทึกฐานข้อมูล
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\\"; // กำหนดโฟลเดอร์เริ่มต้น Drive C
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // แสดงรูปภาพใน PictureBox
                pcbMovieImage.Image = Image.FromFile(openFileDialog.FileName);

                // ตรวจสอบ Formant ของรูปภาพ แล้วแปลงเป็น byte array
                if (pcbMovieImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Png);
                }
            }
        }

        private void btMovieDirectorImage_Click(object sender, EventArgs e)
        {
            // open file dialog เพื่อเลือกไฟล์รูปภาพ jpg, png
            // ถ้าเลือกไฟล์ได้ ให้แสดงรูปภาพใน pcbMovieDirectorImage
            // แปลงเป็น byte array เก็บไว้ในตัวแปรเพื่อใช้ในการบันทึกฐานข้อมูล
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"C:\\"; // กำหนดโฟลเดอร์เริ่มต้น Drive C
            openFileDialog.Filter = "Image Files|*.jpg;*.png;";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // แสดงรูปภาพใน PictureBox
                pcbMovieDirectorImage.Image = Image.FromFile(openFileDialog.FileName);

                // ตรวจสอบ Formant ของรูปภาพ แล้วแปลงเป็น byte array
                if (pcbMovieDirectorImage.Image.RawFormat == ImageFormat.Jpeg)
                {
                    movieDirectorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);
                }
                else
                {
                    movieDirectorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Png);
                }
            }
        }

        private void btSearchMovie_Click(object sender, EventArgs e)
        {
            string keyword = tbSearchMovie.Text.Trim();
            if (keyword.Length == 0)
            {
                showWarningMessage("กรุณาป้อนชื่อภาพยนตร์ที่ต้องการค้นหา");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT movieId, movieName FROM movie_tb WHERE movieName LIKE @keyword";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();
                        lvShowSearchMovie.Items.Clear();

                        while (reader.Read())
                        {
                            ListViewItem item = new ListViewItem(reader["movieId"].ToString());
                            item.SubItems.Add(reader["movieName"].ToString());
                            lvShowSearchMovie.Items.Add(item);
                        }

                        if (lvShowSearchMovie.Items.Count == 0)
                        {
                            MessageBox.Show("ไม่พบข้อมูลที่ค้นหา", "ผลการค้นหา", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการค้นหา\n" + ex.Message);
                    }
                }
            }
        }

        private void lvShowSearchMovie_DoubleClick(object sender, EventArgs e)
        {
            if (lvShowSearchMovie.SelectedItems.Count == 0) return;

            string selectedId = lvShowSearchMovie.SelectedItems[0].Text;
            movieId = int.Parse(selectedId);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM movie_tb WHERE movieId = @id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", movieId);
                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            lbMovieId.Text = movieId.ToString();
                            tbMovieName.Text = reader["movieName"].ToString();
                            tbMovieDetail.Text = reader["movieDetail"].ToString();
                            dtpMovieDate.Value = Convert.ToDateTime(reader["movieDate"]);
                            nudMovieHour.Value = Convert.ToInt32(reader["movieHour"]);
                            nudMovieMinute.Value = Convert.ToInt32(reader["movieMinute"]);
                            tbMovieDirectorName.Text = reader["movieDirectorName"].ToString();
                            cbbMovieType.SelectedItem = reader["movieType"].ToString();

                            if (reader["movieImage"] != DBNull.Value)
                                pcbMovieImage.Image = convertByteArrayToImage((byte[])reader["movieImage"]);

                            if (reader["movieDirectorImage"] != DBNull.Value)
                                pcbMovieDirectorImage.Image = convertByteArrayToImage((byte[])reader["movieDirectorImage"]);

                            btSaveMovie.Enabled = false;
                            btUpdateMovie.Enabled = true;
                            btDeleteMovie.Enabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการโหลดข้อมูล\n" + ex.Message);
                    }
                }
            }
        }

        private void btDeleteMovie_Click(object sender, EventArgs e)
        {
            if (movieId == 0) return;

            DialogResult result = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบข้อมูลนี้?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string sql = "DELETE FROM movie_tb WHERE movieId = @id";
                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", movieId);
                        try
                        {
                            conn.Open();
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("ลบข้อมูลเรียบร้อยแล้ว", "ลบสำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            resetForm();
                            getAllMoiveToListView();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("เกิดข้อผิดพลาดในการลบ\n" + ex.Message);
                        }
                    }
                }
            }

        }


        private void btUpdateMovie_Click(object sender, EventArgs e)
        {
            if (movieId == 0)
            {
                showWarningMessage("กรุณาเลือกภาพยนตร์จากรายการค้นหาก่อน");
                return;
            }

            if (tbMovieName.Text.Trim().Length == 0 ||
                tbMovieDetail.Text.Trim().Length == 0 ||
                pcbMovieImage.Image == null ||
                pcbMovieDirectorImage.Image == null ||
                tbMovieDirectorName.Text.Trim().Length == 0)
            {
                showWarningMessage("กรุณากรอกและเลือกรูปให้ครบถ้วนก่อนแก้ไข");
                return;
            }

            movieImage = convertImageToByteArray(pcbMovieImage.Image, ImageFormat.Jpeg);
            movieDirectorImage = convertImageToByteArray(pcbMovieDirectorImage.Image, ImageFormat.Jpeg);

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = @"UPDATE movie_tb SET
                            movieName=@name,
                            movieDetail=@detail,
                            movieDate=@date,
                            movieHour=@hour,
                            movieMinute=@minute,
                            movieType=@type,
                            movieImage=@img,
                            movieDirectorImage=@dimg,
                            movieDirectorName=@movieDirectorName
                            WHERE movieId=@id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", tbMovieName.Text.Trim());
                    cmd.Parameters.AddWithValue("@detail", tbMovieDetail.Text.Trim());
                    cmd.Parameters.AddWithValue("@date", dtpMovieDate.Value.Date);
                    cmd.Parameters.AddWithValue("@hour", nudMovieHour.Value);
                    cmd.Parameters.AddWithValue("@minute", nudMovieMinute.Value);
                    cmd.Parameters.AddWithValue("@type", cbbMovieType.SelectedItem.ToString());
                    cmd.Parameters.AddWithValue("@img", movieImage);
                    cmd.Parameters.AddWithValue("@dimg", movieDirectorImage);
                    cmd.Parameters.AddWithValue("@id", movieId);
                    cmd.Parameters.AddWithValue("@movieDirectorName", tbMovieDirectorName.Text.Trim());

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("อัปเดตข้อมูลเรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        resetForm();
                        getAllMoiveToListView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการอัปเดต\n" + ex.Message);
                    }
                }
            }
        }


        private void btResetMovie_Click(object sender, EventArgs e)
        {
            resetForm();
        }

        private void btExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void lvShowAllMovie_DoubleClick(object sender, EventArgs e)
        {

            if (lvShowAllMovie.SelectedItems.Count == 0) return;

            int movieId = Convert.ToInt32(lvShowAllMovie.SelectedItems[0].Tag);
            this.movieId = movieId;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string sql = "SELECT * FROM movie_tb WHERE movieId = @id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@id", movieId);
                    try
                    {
                        conn.Open();
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            lbMovieId.Text = movieId.ToString();
                            tbMovieName.Text = reader["movieName"].ToString();
                            tbMovieDetail.Text = reader["movieDetail"].ToString();
                            dtpMovieDate.Value = Convert.ToDateTime(reader["movieDate"]);
                            nudMovieHour.Value = Convert.ToInt32(reader["movieHour"]);
                            nudMovieMinute.Value = Convert.ToInt32(reader["movieMinute"]);

                            // กรณีคุณใช้แบบไม่มี field movieDirectorName ในฐานข้อมูล ให้ลบหรือคอมเมนต์บรรทัดนี้
                            tbMovieDirectorName.Text = reader["movieDirectorName"].ToString();

                            cbbMovieType.SelectedItem = reader["movieType"].ToString();

                            if (reader["movieImage"] != DBNull.Value)
                                pcbMovieImage.Image = convertByteArrayToImage((byte[])reader["movieImage"]);

                            if (reader["movieDirectorImage"] != DBNull.Value)
                                pcbMovieDirectorImage.Image = convertByteArrayToImage((byte[])reader["movieDirectorImage"]);

                            // ปุ่มใช้งาน
                            btSaveMovie.Enabled = false;
                            btUpdateMovie.Enabled = true;
                            btDeleteMovie.Enabled = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("เกิดข้อผิดพลาดในการโหลดข้อมูล\n" + ex.Message);
                    }
                }
            }
        }

    }
}


