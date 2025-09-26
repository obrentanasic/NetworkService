using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NetworkService.ViewModel
{
    public class MainWindowViewModel : ClassINotifyPropertyChanged
    {
        #region Initialize
        public ClassICommand<string> NavCommand { get; private set; }

        private NetworkEntitiesViewModel networkEntitiesViewModel = new NetworkEntitiesViewModel();
        private NetworkDisplayViewModel networkDisplayViewModel = new NetworkDisplayViewModel();
        private MeasurementGraphViewModel measurementGraphViewModel = new MeasurementGraphViewModel();

        private ClassINotifyPropertyChanged currentViewModel;
        private ClassINotifyPropertyChanged alwaysOnViewModel;
        public ClassINotifyPropertyChanged CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                SetProperty(ref currentViewModel, value);
            }
        }
        public ClassINotifyPropertyChanged AlwaysOnViewModel
        {
            get { return alwaysOnViewModel; }
            set
            {
                SetProperty(ref alwaysOnViewModel, value);
            }
        }
        private void OnNav(string destination)
        {
            switch (destination)
            {
                case "1_Entities":
                    CurrentViewModel = networkEntitiesViewModel;
                    break;
                case "2_Graph":
                    CurrentViewModel = measurementGraphViewModel;
                    break;
            }
        }
        public MainWindowViewModel()
        {
            
            NavCommand = new ClassICommand<string>(OnNav);

            CurrentViewModel = networkEntitiesViewModel;
            AlwaysOnViewModel = networkDisplayViewModel;

            createListener(); //Povezivanje sa serverskom aplikacijom

            networkEntitiesViewModel.Entities.CollectionChanged += this.OnCollectionChanged;

            networkEntitiesViewModel.Entities.CollectionChanged += this.OnCollectionChangedMeasurementGraphViewModel;
        }
        #endregion

        #region NotifyCollection
        private void OnCollectionChangedMeasurementGraphViewModel(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (Entity newEntity in e.NewItems)
                {
                    if (!measurementGraphViewModel.EntitiesInList.Contains(newEntity))
                    {
                        measurementGraphViewModel.EntitiesInList.Add(newEntity);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (Entity oldEntity in e.OldItems)
                {
                    if (measurementGraphViewModel.EntitiesInList.Contains(oldEntity))
                    {
                        measurementGraphViewModel.EntitiesInList.Remove(oldEntity);
                    }
                }
            }
        }
        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
           if (e.NewItems != null)
            {
                foreach (Entity newEntity in e.NewItems)
                {
                    if (!networkDisplayViewModel.EntitiesInList.Contains(newEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Add(newEntity);
                    }
                }
            }
            if (e.OldItems != null)
            {
                foreach (Entity oldEntity in e.OldItems)
                {
                    if (networkDisplayViewModel.EntitiesInList.Contains(oldEntity))
                    {
                        networkDisplayViewModel.EntitiesInList.Remove(oldEntity);
                    }
                    else
                    {
                        int canvasIndex = networkDisplayViewModel.GetCanvasIndexForEntityId(oldEntity.Id);
                        networkDisplayViewModel.DeleteEntityFromCanvas(oldEntity);
                    }
                }
            }
        }
        #endregion

        #region TCP_Server
        private void createListener()
        {
            var tcp = new TcpListener(IPAddress.Any, 25675);
            tcp.Start();

            var listeningThread = new Thread(() =>
            {
                while (true)
                {
                    var tcpClient = tcp.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(param =>
                    {
                        //Prijem poruke
                        NetworkStream stream = tcpClient.GetStream();
                        string incomming;
                        byte[] bytes = new byte[1024];
                        int i = stream.Read(bytes, 0, bytes.Length);
                        //Primljena poruka je sacuvana u incomming stringu
                        incomming = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        //Ukoliko je primljena poruka pitanje koliko objekata ima u sistemu -> odgovor
                        if (incomming.Equals("Need object count"))
                        {
                            //Response
                            /* Umesto sto se ovde salje count.ToString(), potrebno je poslati 
                             * duzinu liste koja sadrzi sve objekte pod monitoringom, odnosno
                             * njihov ukupan broj (NE BROJATI OD NULE, VEC POSLATI UKUPAN BROJ)
                             * */
                            Byte[] data = System.Text.Encoding.ASCII.GetBytes(networkEntitiesViewModel.Entities.Count.ToString());
                            stream.Write(data, 0, data.Length);

                            if (File.Exists("Log.txt"))
                            {
                                File.WriteAllText("Log.txt", String.Empty);
                            }
                            else
                            {
                                File.Create("Log.txt");
                            }                     
                        }
                        else
                        {
                            //U suprotnom, server je poslao promenu stanja nekog objekta u sistemu
                            Console.WriteLine(incomming); //Na primer: "Entitet_1:272"

                            //################ IMPLEMENTACIJA ####################
                            // Obraditi poruku kako bi se dobile informacije o izmeni
                            // Azuriranje potrebnih stvari u aplikaciji

                            if (networkEntitiesViewModel.Entities.Count > 0)
                            {
                                var splited = incomming.Split(':');
                                DateTime dt = DateTime.Now;
                                using (StreamWriter sw = File.AppendText("Log.txt"))
                                    sw.WriteLine(dt + "; " + splited[0] + ", " + splited[1]);

                                int id = Int32.Parse(splited[0].Split('_')[1]);
                                networkEntitiesViewModel.Entities[id].Value = Double.Parse(splited[1]);

                                networkDisplayViewModel.UpdateEntityOnCanvas(networkEntitiesViewModel.Entities[id]);
                                measurementGraphViewModel.AutoShow();
                            }    
                        }
                    }, null);
                }
            });
            listeningThread.IsBackground = true;
            listeningThread.Start();
        }
        #endregion
    }
}
