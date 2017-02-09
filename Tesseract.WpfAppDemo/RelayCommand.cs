﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Tesseract
{
    class RelayCommand : ICommand
    {
        private readonly Action<object> _action;

        public RelayCommand(Action<object> action)
        {
            _action = action;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _action(parameter ?? string.Empty);
        }

        #endregion
    }
}