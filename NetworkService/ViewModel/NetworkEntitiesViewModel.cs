using NetworkService.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;

namespace NetworkService.ViewModel
{
    public class NetworkEntitiesViewModel : ClassINotifyPropertyChanged
    {
        #region Initialize
        private int _id = 0;
        private string errorMSg = "";
        public List<string> ComboBoxItems { get; set; } = new List<string>()
        {
            "Zapreminski",
            "Turbinski",
            "Elektronski"
        };
        public ObservableCollection<Entity> EntitiesToShow { get; set; }
        public ObservableCollection<Entity> Entities { get; set; }
        public ObservableCollection<Entity> EntitiesSearched { get; set; }

        private ObservableCollection<string> searchedHistory = new ObservableCollection<string>();
        public ClassICommand AddEntityCommand { get; set; }
        public ClassICommand DeleteEntityCommand { get; set; }
        public ClassICommand SearchEntityCommand { get; set; }
        public ClassICommand RefreshEntityCommand { get; set; }
        public ClassICommand SaveSearchCommand { get; set; }

        // For add entity 
        private EntityType currentEntityType = new EntityType();

        // Entity selected in table
        private Entity selectedEntity;

        //Search
        private string searchBox;
        private bool isTypeRBSelected;
        private bool isNameRBSelected = true;
        
        public NetworkEntitiesViewModel()
        {
            Entities = new ObservableCollection<Entity>();
            EntitiesSearched = new ObservableCollection<Entity>();
            EntitiesToShow = Entities;

            AddEntityCommand = new ClassICommand(OnAdd);
            DeleteEntityCommand = new ClassICommand(OnDelete, CanDelete);
            SearchEntityCommand = new ClassICommand(onSearch);
            RefreshEntityCommand = new ClassICommand(onRefresh);
            SaveSearchCommand = new ClassICommand(onSaveHistory);
        }
        #endregion

        #region SaveHistory

        public ObservableCollection<string> SearchedHistory
        {
            get { return searchedHistory; }
            set
            {
                searchedHistory = value;
                OnPropertyChanged("SearchedHistory");
            }
        }

        public void onSaveHistory()
        {
            if (!SearchedHistory.Contains(SearchBox) && SearchBox != null && SearchBox.Trim() != "")
            {
                SearchedHistory.Add(SearchBox);
                MessageBox.Show("Successfully added to history", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                OnPropertyChanged("SearchedHistory");
            }
        }

        #endregion

        #region Refresh
        private void onRefresh()
        {
            if(EntitiesToShow != Entities)
            {
                EntitiesToShow = Entities;
                OnPropertyChanged("EntitiesToShow");
            }      
        }
        #endregion

        #region SearchBTN

        public string SearchBox
        {
            get { return searchBox; }
            set
            {
                searchBox = value;
                OnPropertyChanged("SearchBox");
            }
        }

        public bool IsTypeRBSelected
        {
            get { return isTypeRBSelected; }
            set
            {
                isTypeRBSelected = value;
                OnPropertyChanged("IsTypeRBSelected");
            }
        }

        public bool IsNameRBSelected
        {
            get { return isNameRBSelected; }
            set
            {
                isNameRBSelected = value;
                OnPropertyChanged("IsNameRBSelected");
            }
        }

        private void onSearch()
        {
            EntitiesSearched.Clear();
            try
            {
                if (IsTypeRBSelected)
                {
                    if (SearchBox.Trim() == "")
                    {
                        if (EntitiesToShow != Entities)
                        {
                            EntitiesToShow = Entities;
                            OnPropertyChanged("EntitiesToShow");
                        }
                    }
                    else
                    {
                        foreach (var entity in Entities)
                        {
                            if (entity.Type.Type.Contains(SearchBox))
                            {
                                EntitiesSearched.Add(entity);
                            }
                        }
                        EntitiesToShow = EntitiesSearched;
                        OnPropertyChanged("EntitiesToShow");
                        
                    }
                }
                else
                {
                    if (SearchBox.Trim() == "")
                    {
                        if (EntitiesToShow != Entities)
                        {
                            EntitiesToShow = Entities;
                            OnPropertyChanged("EntitiesToShow");
                        }
                    }
                    else
                    {
                        foreach (var entity in Entities)
                        {
                            if (entity.Name.Contains(SearchBox))
                            {
                                EntitiesSearched.Add(entity);
                            }
                        }
                        EntitiesToShow = EntitiesSearched;
                        OnPropertyChanged("EntitiesToShow");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region DeletEntityBTN

        private void RestartSimulator()
        {
            try
            {
                
                string simulatorRelativePath = @"..\..\..\..\..\MeteringSimulator\MeteringSimulator\obj\Debug\MeteringSimulator.exe";
                string simulatorPath = System.IO.Path.GetFullPath(simulatorRelativePath);


                // Kill
                foreach (var process in Process.GetProcessesByName("MeteringSimulator"))
                {
                    process.Kill();
                }
                

                // Start
                Process.Start(simulatorPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not restart simulator: {ex.Message}");
            }
        }

        public Entity SelectedEntity
        {
            get { return selectedEntity; }
            set
            {
                selectedEntity = value;
                DeleteEntityCommand.RaiseCanExecuteChanged();
            }
        }
        private void OnDelete()
        {
            Entities.Remove(SelectedEntity);
            if (EntitiesSearched.Contains(SelectedEntity))
            {
                EntitiesSearched.Remove(SelectedEntity);
            }
            MessageBox.Show("Successfully deleted entity", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            RestartSimulator();
        }

        private bool CanDelete()
        {
            return SelectedEntity != null;
        }
        #endregion

        #region AddEntityBTN
        public string ErrorMSg
        {
            get { return errorMSg; }
            set
            {
                errorMSg = value;
                OnPropertyChanged("ErrorMSg");
            }
        }
        public EntityType CurrentEntityType
        {
            get { return currentEntityType; }
            set
            {
                currentEntityType = value;
                OnPropertyChanged("CurrentEntityType");
            }
        }

        public void OnAdd()
        {
            string imgPath = "";
            if(CurrentEntityType.Type == null)
            {
                ErrorMSg = "Need To Choose Type!";
                return;
            }
            else if(CurrentEntityType.Type == "Zapreminski")
            {
                imgPath = "pack://application:,,,/NetworkService;component/Images/Zapreminski.png";
            }
            else if( CurrentEntityType.Type == "Turbinski")
            {
                imgPath = "pack://application:,,,/NetworkService;component/Images/Turbinski.png";
            }
            else
            {
                imgPath = "pack://application:,,,/NetworkService;component/Images/Elektronski.png";
            }
            ErrorMSg = "";
            try
            {
                Entities.Add(new Entity() 
                            { Id = _id,
                              Name = $"Entity_{_id++}",
                              Value = 0,
                              Type = new EntityType() { Type = CurrentEntityType.Type, ImgSrc = imgPath }
                            });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{DateTime.Now} - {ex.Message}");
            }
            RestartSimulator();
        }
        #endregion
    }
}
