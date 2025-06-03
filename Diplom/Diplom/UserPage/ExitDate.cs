using Diplom.BaseData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Diplom.UserPage
{

    public class Licenses
    {
        public int Id { get; set; }
        public string SoftwareName { get; set; }
        public string StatusName { get; set; }
        public string LicenseKey { get; set; }
    }
    
    public class Device
    {
        public int DeviceId { get; set; }
        public string Model { get; set; }
        public string ProducerN { get; set; }

        public string FullName => $"{ProducerN} {Model}";
    }
    

    public class DataService
    {
        private readonly string connectionString = "Data Source=DESKTOP-OTKVL60;Initial Catalog=Diplom;Integrated Security=True";

        public (ObservableCollection<Device> Devices, ObservableCollection<Licenses> Licenses) GetLicenses(string nickname)
        {
            var devices = new ObservableCollection<Device>();
            var licenses = new ObservableCollection<Licenses>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
        SELECT
            p.Name_producer,
            d.model
        FROM Device d
        JOIN Producer p ON d.Producer_id = p.Id_producer
        JOIN [User] u ON d.User_id = u.Id_user
        WHERE u.nickname = @nickname;

        SELECT              
            s.Name_soft AS SoftName,
            st.status AS StatusName,
            l.license_key
        FROM Licenses l
        JOIN Soft s ON l.Soft_id = s.Id_soft
        JOIN Status st ON l.status_id = st.Id_status
        JOIN [User] u ON l.user_id = u.Id_user
        WHERE u.nickname = @nickname;
        ";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@nickname", nickname);

                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    // Читаем первый результат — устройства
                    while (reader.Read())
                    {
                        devices.Add(new Device
                        {
                            ProducerN = reader["Name_producer"].ToString(),
                            Model = reader["model"].ToString()
                        });
                    }

                    // Переходим ко второму результату — лицензии
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            licenses.Add(new Licenses
                            {
                                SoftwareName = reader["SoftName"].ToString(),
                                StatusName = reader["StatusName"].ToString(),
                                LicenseKey = reader["license_key"].ToString()
                            });
                        }
                    }
                }
            }

            return (devices, licenses);
        }
        


        public class MainViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<Licenses> Licenses { get; set; }
            public ObservableCollection<Device> Devices { get; set; }

            private string _nickname;
            public string Nickname
            {
                get => _nickname;
                set
                {
                    if (_nickname != value)
                    {
                        _nickname = value;
                        OnPropertyChanged(nameof(Nickname));
                    }
                }
            }

            public MainViewModel(string nickname)
            {
                Nickname = nickname;
                DataService service = new DataService();
                var data = service.GetLicenses(nickname);
                Devices = data.Devices;
                Licenses = data.Licenses;
             
            }
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string propertyName) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
    }   

}

