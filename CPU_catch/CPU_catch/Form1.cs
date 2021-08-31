using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Data.SqlClient;
using System.Collections;

namespace CPU_catch
{
    public partial class Form1 : Form
    {
        private delegate void mydalegate(float cpu_need, float memory_need);
        
        public Form1()
        {
            InitializeComponent();

        }

        private PerformanceCounter cpu = new PerformanceCounter("Processor", "% Processor Time", "_Total");

        private PerformanceCounter memory = new PerformanceCounter("Memory", "% Committed Bytes in Use");

        private void Getrow(float cpu_need, float memory_need)
        {
            if (this.InvokeRequired)
            {
                mydalegate del = new mydalegate(Getrow);
                this.Invoke(del, cpu_need, memory_need);
            }
            else
            {
                //Console.WriteLine("CPU: {0:n1}%", cpu.NextValue());
                //Console.WriteLine("Memory: {0:n0}%", memory.NextValue());
                String insert_str;
                SqlConnection myConn = new SqlConnection("Data Source=DSTY-122448\\SQLEXPRESS;Initial Catalog=master.dbo.cpu_db;Integrated Security=True;Pooling=False");

                if (cpu_need != 0)
                {
                    insert_str = "INSERT INTO CPU_DB(CPU,Memory) VALUES(" + (int)cpu_need + "," + (int)memory_need + ")";
                    SqlCommand myCommand = new SqlCommand(insert_str, myConn);
                    myConn.Open();
                    myCommand.ExecuteNonQuery();


                    int cpu_int, mem_int;
                    cpu_int = (int)cpu_need;
                    mem_int = (int)memory_need;
                    dataGridView1.Rows.Add((cpu_int.ToString()+"%"), (mem_int.ToString()+"%"));
                    
                }
                dataGridView1.Refresh();   
            }
        }

        private void Addrow()
        {
            while (!start.Enabled)
            {
                Getrow(cpu.NextValue(), memory.NextValue());
                Thread.Sleep(1000);
            }
        }
        public void start_Click(object sender, EventArgs e)
        {
            
            start.Enabled = false;
            stop.Enabled = true;

            Thread thrStart = new Thread(Addrow);
            thrStart.Start();

        }
        public void stop_Click(object sender, EventArgs e)
        {
            stop.Enabled = false;
            start.Enabled = true;
            
        }

        private void clear_DB_Click(object sender, EventArgs e)
        {

            String clear_query;
            SqlConnection myConn = new SqlConnection("Data Source=DSTY-122448\\SQLEXPRESS;Initial Catalog=master.dbo.cpu_db;Integrated Security=True;Pooling=False");
            clear_query = "truncate table CPU_DB";
            SqlCommand myCommand = new SqlCommand(clear_query, myConn);
            try
            {
                myConn.Open();
                myCommand.ExecuteNonQuery();
                MessageBox.Show("CPU_DB is clear Successfully");
                dataGridView1.Rows.Clear();
                dataGridView1.Refresh();
                dataGridView2.Rows.Clear();
                dataGridView2.Refresh();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("CPU_DB clear error");
            }
          
        }

        private static ArrayList CPU_info = new ArrayList();
        private static ArrayList Memory_info = new ArrayList();
        private void search_Click(object sender, EventArgs e)
        {
            dataGridView2.Rows.Clear();
            dataGridView2.Refresh();
            CPU_info.Clear();
            Memory_info.Clear();

            GetData();
            if (CPU_info.Count > 0)
            {
                updateDatagrid();
            }
            else
            {}
        }

        private void GetData()
        {
            SqlConnection myConn = new SqlConnection("Data Source=DSTY-122448\\SQLEXPRESS;Initial Catalog=master.dbo.cpu_db;Integrated Security=True;Pooling=False");
            myConn.Open();

            SqlCommand myCommand = new SqlCommand(textBox1.Text, myConn);
            try
            {
                //myConn.Open();
                using (SqlDataReader myReader = myCommand.ExecuteReader())
                {
                    if (myReader.HasRows)
                    {
                        while (myReader.Read())
                        {
                            if (myReader["CPU"].ToString() != "0")
                            {
                                CPU_info.Add(myReader["CPU"].ToString());
                                Memory_info.Add(myReader["Memory"].ToString());
                            }
                        }
                    }
                    else 
                    {
                        MessageBox.Show("Data not found or error query!");
                    }
                    myConn.Close();
                }
        
                //select * from CPU_DB where CPU > 4

            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Data not found or error query!");
            }
        }
        private void updateDatagrid()
        {
            
            for (int i = 0; i < CPU_info.Count; i++)
            {
                DataGridViewRow newRow = new DataGridViewRow();

                newRow.CreateCells(dataGridView2);
                newRow.Cells[0].Value = (CPU_info[i]+"%");
                newRow.Cells[1].Value = (Memory_info[i]+"%");
                dataGridView2.Rows.Add(newRow);
            }
        }
    }
}
