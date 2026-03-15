using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace DSS_Avalonia.Models
{
    /// <summary>
    /// Ситуация — совокупность параметров, описывающих текущее состояние.
    /// </summary>
    public class Situation : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _name = string.Empty;
        private string _description = string.Empty;
        private DateTime _createdAt = DateTime.Now;
        private List<SituationParameter> _parameters = new();

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        /// <summary>Название ситуации.</summary>
        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        /// <summary>Текстовое описание.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        /// <summary>Дата и время создания записи.</summary>
        public DateTime CreatedAt
        {
            get => _createdAt;
            set { _createdAt = value; OnPropertyChanged(nameof(CreatedAt)); }
        }

        /// <summary>Параметры (признаки) ситуации.</summary>
        public List<SituationParameter> Parameters
        {
            get => _parameters;
            set { _parameters = value; OnPropertyChanged(nameof(Parameters)); }
        }

        // ---- Вспомогательные методы ----

        /// <summary>Получить значение параметра по имени (null если не найден).</summary>
        public double? GetParameterValue(string name)
        {
            var p = _parameters.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            return p?.Value;
        }

        /// <summary>Установить или добавить параметр.</summary>
        public void SetParameter(string name, double value, double weight = 1.0, string unit = "")
        {
            var existing = _parameters.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
                existing.Value = value;
            else
                _parameters.Add(new SituationParameter(name, value, weight, unit));
        }

        public Situation Clone()
        {
            return new Situation
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                CreatedAt = DateTime.Now,
                Parameters = Parameters.Select(p => p.Clone()).ToList()
            };
        }

        public override string ToString() => $"[{Name}] {Description}".Trim();

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
