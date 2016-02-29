using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Management;
using Microsoft.Win32;
using System.Data.SQLite;
using System.IO;
using System.Runtime.Serialization.Json;
namespace 迅雷高速通道破解器
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
        }

     


        private void Start_Click(object sender, EventArgs e)
        {
            string Path = GetPath();
            Modify(Path + "\\Profiles\\TaskDb.dat");
            // label1.Text = Path;

        }



        //获取迅雷路径
        private string GetPath()
        {

            RegistryKey key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall", false);
            if (key != null)
            {
                foreach (string keyName in key.GetSubKeyNames())
                {
                    using (RegistryKey key2 = key.OpenSubKey(keyName, false))
                    {
                        if (key2 != null)
                        {
                            string softwareName = key2.GetValue("DisplayName", "").ToString();

                            if (softwareName.Contains("迅雷"))
                            {
                                return key2.GetValue("InstallLocation", "").ToString();
                            }


                        }
                    }
                }
            }

            return "";

        }


        private Boolean Modify(String path)
        {


            SQLiteConnection conn = new SQLiteConnection("Data Source=" + path + ";UTF8Encoding=True;Version=3;New=False;Compress=True;Synchronous=Off;");

            conn.Open();

            //获取表名，因为后缀为_1_1
            SQLiteCommand cmd1 = new SQLiteCommand("select name from sqlite_master where type='table' and name like '%_1_1%'", conn);


            //遍历表名
            SQLiteDataReader reader_tableName = cmd1.ExecuteReader();

            while (reader_tableName.Read())
            {

                String tablename = reader_tableName.GetString(0).ToString();
               
                SQLiteCommand cmd2 = new SQLiteCommand("select * from " + tablename, conn);

                //遍历表中数据
                SQLiteDataReader reader = cmd2.ExecuteReader();
                while (reader.Read())
                {

                   
                    //1.获取UserData的数据
                    MemoryStream streamImage = new MemoryStream(reader["UserData"] as byte[]);
                    byte[] blob = StreamToBytes(streamImage);
                    
                    
                    streamImage.Close();

               
                 
                    //2.获取出来的数据是二进制，要转为十六进制
                    String[] str_16 =new String[blob.Length];
                    String result = "";
                    for(int i = 0;i<blob.Length;i++){

                        str_16[i] = Convert.ToString(blob[i], 16);
                       
                        result = result + HexStringToString(str_16[i], Encoding.UTF8);
                      
                    }
                   // System.Diagnostics.Debug.WriteLine("sss"+result);
                    UserData userData = new UserData();
                    userData = ParseFormJson<UserData>(result);
                    userData.Result = 0;

                 
                    String result1 = GetJson<UserData>(userData);


                   

                    //要把这个转成十六进制的字符串
                    byte[] buff = System.Text.Encoding.ASCII.GetBytes(result1);
                   


                    //进行更新
                    SQLiteCommand cmd3 = new SQLiteCommand("update" + " " + tablename + " set UserData =@data where LocalSubFileIndex=" + reader.GetInt64(2), conn);
                    
                    SQLiteParameter para = new SQLiteParameter("@data", DbType.Binary);
                    para.Value = buff;
                    cmd3.Parameters.Add(para);
                    cmd3.ExecuteNonQuery();
                    cmd3.Cancel();
                }
                cmd2.Cancel();

            }

            // Response.Write("当前的总记录数：" + recordCount + "<br/>"); 
            cmd1.Cancel();
            conn.Close();

            label_info.Text = "破解成功，快开启迅雷下载吧";


            return false;
        }
        public static string HexStringToString(string hs, Encoding encode)
        {
            //以%分割字符串，并去掉空字符
            string[] chars = hs.Split(new char[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
            byte[] b = new byte[chars.Length];
            //逐个字符变为16进制字节数据
            for (int i = 0; i < chars.Length; i++)
            {
                b[i] = Convert.ToByte(chars[i], 16);
            }
            //按照指定编码将字节数组变为字符串
            return encode.GetString(b);
        }
      
       
        public byte[] StreamToBytes(Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, bytes.Length);
            // 设置当前流的位置为流的开始
            stream.Seek(0, SeekOrigin.Begin);
            return bytes;
        }

        public static T ParseFormJson<T>(string szJson)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(szJson)))
            {
                DataContractJsonSerializer dcj = new DataContractJsonSerializer(typeof(T));
                return (T)dcj.ReadObject(ms);
            }
        }

        public static string GetJson<T>(T obj)
        {
            //记住 添加引用 System.ServiceModel.Web 
            /**
             * 如果不添加上面的引用,System.Runtime.Serialization.Json; Json是出不来的哦
             * */
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                json.WriteObject(ms, obj);
                string szJson = Encoding.UTF8.GetString(ms.ToArray());

                return szJson;
            }
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }
      


    }
}
