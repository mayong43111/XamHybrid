using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms;

namespace XHApp.Models
{
    public class FaceClientResultModel : INotifyPropertyChanged
    {
        /// <summary>
        /// 
        /// </summary>
        public string FaceID { get; set; }

        public string _name;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;

                if (this.PropertyChanged != null)
                {
                    this.PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Name"));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public ImageSource Image { get; set; }

        public SKData ImageData { get; internal set; }
    }
}
