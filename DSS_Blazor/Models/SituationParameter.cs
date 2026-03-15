using System;
using System.ComponentModel;

namespace DSS_Blazor.Models
{
    /// <summary>
    /// Параметр ситуации — числовое или категориальное значение одного признака.
    /// </summary>
    public class SituationParameter : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private double _value;
        private double _weight = 1.0;
        private string _unit = string.Empty;

        /// <summary>Имя параметра (признака ситуации).</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>Числовое значение параметра.</summary>
        public double Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        /// <summary>Вес параметра при вычислении сходства (0..1).</summary>
        public double Weight
        {
            get => _weight;
            set
            {
                _weight = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged(nameof(Weight));
            }
        }

        /// <summary>Единица измерения (необязательно).</summary>
        public string Unit
        {
            get => _unit;
            set { _unit = value; OnPropertyChanged(nameof(Unit)); }
        }

        public SituationParameter() { }

        public SituationParameter(string name, double value, double weight = 1.0, string unit = "")
        {
            Name = name;
            Value = value;
            Weight = weight;
            Unit = unit;
        }

        public SituationParameter Clone() =>
            new SituationParameter(Name, Value, Weight, Unit);

        public override string ToString() =>
            string.IsNullOrEmpty(Unit)
                ? $"{Name} = {Value}"
                : $"{Name} = {Value} {Unit}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
