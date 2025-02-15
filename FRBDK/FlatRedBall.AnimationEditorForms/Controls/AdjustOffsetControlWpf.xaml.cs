﻿using FlatRedBall.AnimationEditorForms.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FlatRedBall.AnimationEditorForms.Controls
{
    /// <summary>
    /// Interaction logic for AdjustOffsetControlWpf.xaml
    /// </summary>
    public partial class AdjustOffsetControlWpf : UserControl
    {
        public AdjustOffsetViewModel ViewModel => DataContext as AdjustOffsetViewModel;

        public event Action OkClick;
        public event Action CancelClick;

        public AdjustOffsetControlWpf()
        {
            InitializeComponent();

            this.DataContext = new AdjustOffsetViewModel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OkClick();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CancelClick();
        }


    }
}
