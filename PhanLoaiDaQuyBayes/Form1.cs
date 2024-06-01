using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace PhanLoaiDaQuyBayes
{
    public partial class Form1 : Form
    {
        struct PredictionClass
        {
            public string name;
            public int count;
            public double p;
        }

        struct DataTrain
        {
            public string name;
            public int[] count;
            public double[] p;
            public int column;
        }

        PredictionClass[] predictionClass;
        DataTrain[] dataTrain;
        int totalPredictionClass;
        int totalDataTrain;

        public Form1()
        {
            InitializeComponent();
            flowLayoutPanel1.FlowDirection = FlowDirection.TopDown;
            flowLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            flowLayoutPanel1.Dock = DockStyle.Top;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            predictionClass = new PredictionClass[100];
            dataTrain = new DataTrain[100];
            totalPredictionClass = 1;
            totalDataTrain = 0;

            richTextBox1.Clear();
            dataGridView1.Refresh();

            while (flowLayoutPanel1.Controls.Count > 0)
            {
                flowLayoutPanel1.Controls[flowLayoutPanel1.Controls.Count - 1].Dispose();
            }
            // Mở hộp thoại chọn file
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // Đọc dữ liệu từ tệp CSV
                using (var reader = new StreamReader(openFileDialog1.FileName))
                {
                    string headerLine = reader.ReadLine();
                    string[] headers = headerLine.Split(',');
                    DataTable dataTable = new DataTable();
                    foreach (string header in headers)
                    {
                        dataTable.Columns.Add(header);
                    }
                    while (!reader.EndOfStream)
                    {
                        string dataLine = reader.ReadLine();
                        string[] values = dataLine.Split(',');
                        DataRow row = dataTable.NewRow();
                        for (int i = 0; i < headers.Length; i++)
                        {
                            row[i] = values[i];
                        }
                        dataTable.Rows.Add(row);
                    }

                    // Hiển thị dữ liệu trong DataGridView
                    dataGridView1.DataSource = dataTable;
                }
            }

            // Tìm lớp dự báo
            DataTable dt = dataGridView1.DataSource as DataTable;
            if (dt != null)
            {
                predictionClass[0].name = dt.Rows[0]["Loai_da_quy"].ToString();
                predictionClass[0].count = 0;
                int indexColumn = 0;

                foreach (DataRow row in dt.Rows)
                {
                    string name = row["Loai_da_quy"].ToString();
                    for (int i = 0; i < totalPredictionClass; i++)
                    {
                        if (name.Equals(predictionClass[i].name))
                        {
                            predictionClass[i].count++;
                            break;
                        }
                        if (i == totalPredictionClass - 1)
                        {
                            predictionClass[totalPredictionClass].name = name;
                            predictionClass[totalPredictionClass].count = 0;
                            totalPredictionClass++;
                        }
                    }
                }

                dataTrain[0].name = dt.Rows[0][1].ToString();
                dataTrain[0].count = new int[totalPredictionClass];
                dataTrain[0].column = 0;

                for (int i = 1; i < dataGridView1.ColumnCount; i++)
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        string item = row[i].ToString();
                        string itemPredictionClass = row["Loai_da_quy"].ToString();
                        int index = 0;
                        bool store = false;

                        for (int j = 0; j < totalPredictionClass; j++)
                        {
                            if (itemPredictionClass.Equals(predictionClass[j].name))
                            {
                                index = j;
                                break;
                            }
                        }

                        for (int j = 0; j < totalDataTrain; j++)
                        {
                            if (item.Equals(dataTrain[j].name) && dataTrain[j].column == i)
                            {
                                dataTrain[j].count[index]++;
                                store = true;
                                break;
                            }
                        }

                        if (store == false)
                        {
                            dataTrain[totalDataTrain].name = item;
                            dataTrain[totalDataTrain].count = new int[totalPredictionClass];
                            dataTrain[totalDataTrain].count[index]++;
                            dataTrain[totalDataTrain].column = i;
                            totalDataTrain++;
                        }
                    }
                }

                for (int i = 0; i < totalPredictionClass; i++)
                {
                    predictionClass[i].p = predictionClass[i].count * 1.0 / dataGridView1.Height;
                    richTextBox1.Text += "Name: " + predictionClass[i].name + " Count:" + predictionClass[i].count + " P: " + predictionClass[i].p + "\n";
                }

                for (int i = 0; i < totalDataTrain; i++)
                {
                    richTextBox1.Text += "Name: " + dataTrain[i].name + " column: " + dataTrain[i].column + "\n";
                    dataTrain[i].p = new double[totalPredictionClass];
                    for (int j = 0; j < totalPredictionClass; j++)
                    {
                        dataTrain[i].p[j] = dataTrain[i].count[j] * 1.0 / predictionClass[j].count;
                        richTextBox1.Text += "Count " + predictionClass[j].name + ": " + dataTrain[i].count[j] + " p: " + dataTrain[i].p[j] + "\n";
                    }
                    richTextBox1.Text += "\n";
                }

                foreach (DataGridViewColumn column in dataGridView1.Columns)
                {
                    if (indexColumn == 0)
                    {
                        indexColumn++;
                        continue;
                    }
                    Label label = new Label();
                    label.Text = column.Name;
                    TextBox textBox = new TextBox();

                    // Thiết lập Text của Label là tên nhãn cho TextBox tương ứng
                    textBox.Name = "textBox1";
                    textBox.Width = flowLayoutPanel1.Width / 2;

                    // Thêm Label và TextBox vào FlowLayoutPanel
                    flowLayoutPanel1.Controls.Add(label);
                    flowLayoutPanel1.Controls.Add(textBox);
                    indexColumn++;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int column = 1;
            double maxF = 0;
            string className = predictionClass[0].name;
            foreach (Control control in flowLayoutPanel1.Controls)
            {
                if (control is TextBox)
                {
                    bool check = false;
                    if (control.Text == "")
                    {
                        MessageBox.Show("Trường " + dataGridView1.Columns[column].Name + " Không được để trống!", "Hộp thoại nhập trường dự đoán", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    for (int j = 0; j < totalDataTrain; j++)
                    {
                        if (dataTrain[j].column == column && dataTrain[j].name.Equals(control.Text))
                        {
                            check = true;
                            break;
                        }
                    }

                    if (!check)
                    {
                        MessageBox.Show("Trường " + dataGridView1.Columns[column].Name + " Không hợp lệ!", "Hộp thoại nhập trường dự đoán", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                    column++;
                }
            }

            column = 1;

            for (int i = 0; i < totalPredictionClass; i++)
            {
                double maxFCon = predictionClass[i].p;

                foreach (Control control in flowLayoutPanel1.Controls)
                {
                    if (control is TextBox)
                    {
                        for (int j = 1; j < totalDataTrain; j++)
                        {
                            if (dataTrain[j].name.Equals(control.Text))
                            {
                                maxFCon = maxFCon * dataTrain[j].p[i];
                            }
                        }
                        column++;
                    }
                }
                if (maxF < maxFCon)
                {
                    maxF = maxFCon;
                    className = predictionClass[i].name;
                }
            }
            lbKDD.Text = "Loại đá quý được dự đoán: " + className;
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
            richTextBox1.SelectionStart = richTextBox1.Text.Length;
            richTextBox1.ScrollToCaret();
        }
    }
}
