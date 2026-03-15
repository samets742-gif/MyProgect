using System;
using System.ComponentModel;

namespace DSS_Avalonia.Models
{
    /// <summary>
    /// Запись в базе знаний: эталонная ситуация + соответствующее решение.
    /// </summary>
    public class KnowledgeBaseEntry : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private Situation _referenceSituation = new();
        private Decision _decision = new();
        private int _usageCount;
        private DateTime _lastUsed = DateTime.Now;
        private string _category = string.Empty;

        /// <summary>Уникальный идентификатор записи.</summary>
        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        /// <summary>Эталонная (прецедентная) ситуация.</summary>
        public Situation ReferenceSituation
        {
            get => _referenceSituation;
            set { _referenceSituation = value; OnPropertyChanged(nameof(ReferenceSituation)); }
        }

        /// <summary>Решение, принятое для данной ситуации.</summary>
        public Decision Decision
        {
            get => _decision;
            set { _decision = value; OnPropertyChanged(nameof(Decision)); }
        }

        /// <summary>Количество использований этой записи.</summary>
        public int UsageCount
        {
            get => _usageCount;
            set { _usageCount = value; OnPropertyChanged(nameof(UsageCount)); }
        }

        /// <summary>Дата последнего обращения.</summary>
        public DateTime LastUsed
        {
            get => _lastUsed;
            set { _lastUsed = value; OnPropertyChanged(nameof(LastUsed)); }
        }

        /// <summary>Категория/домен знаний.</summary>
        public string Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public void RecordUsage()
        {
            UsageCount++;
            LastUsed = DateTime.Now;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
