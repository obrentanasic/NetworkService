using System;
using System.Windows.Media;

namespace NetworkService.Model
{
    public class CircleMarker : ClassINotifyPropertyChanged
    {
        private int cmValue;
        private string cmDate;
        private string cmTime;
        private Brush cmColor;
        private int cmWidthAndHeight;
        private double x;
        private double y;
        private double nextX;
        private double nextY;

        public double X
        {
            get => x;
            set
            {
                x = value;
                OnPropertyChanged(nameof(X));
                OnPropertyChanged(nameof(CenterX));
            }
        }

        public double Y
        {
            get => y;
            set
            {
                y = value;
                OnPropertyChanged(nameof(Y));
                OnPropertyChanged(nameof(CenterY));
            }
        }

        public double NextX
        {
            get => nextX;
            set { nextX = value; OnPropertyChanged(nameof(NextX)); }
        }

        public double NextY
        {
            get => nextY;
            set { nextY = value; OnPropertyChanged(nameof(NextY)); }
        }

        public double CenterX => X + (CmWidthAndHeight / 2.0);
        public double CenterY => Y + (CmWidthAndHeight / 2.0);

        public CircleMarker()
        {
            CmValue = 1;
        }

        public CircleMarker(int cmValue, string cmDate, string cmTime)
        {
            CmValue = cmValue;
            CmDate = cmDate;
            CmTime = cmTime;
        }

        public int CmValue
        {
            get => cmValue;
            set
            {
                cmValue = value;
                CmWidthAndHeight = (int)Math.Round(cmValue / 15.0);

                if (cmValue >= 670 && cmValue <= 735)
                    CmColor = Brushes.Green;
                else if ((cmValue > 0 && cmValue < 670) || cmValue > 735)
                    CmColor = Brushes.Red;
                else
                    CmColor = Brushes.CadetBlue;

                OnPropertyChanged(nameof(CmValue));
            }
        }

        public string CmDate
        {
            get => cmDate;
            set { cmDate = value; OnPropertyChanged(nameof(CmDate)); }
        }

        public int CmWidthAndHeight
        {
            get => cmWidthAndHeight;
            set
            {
                cmWidthAndHeight = value;
                OnPropertyChanged(nameof(CmWidthAndHeight));
                OnPropertyChanged(nameof(CenterX));
                OnPropertyChanged(nameof(CenterY));
            }
        }

        public string CmTime
        {
            get => cmTime;
            set { cmTime = value; OnPropertyChanged(nameof(CmTime)); }
        }

        public Brush CmColor
        {
            get => cmColor;
            set { cmColor = value; OnPropertyChanged(nameof(CmColor)); }
        }
    }
}