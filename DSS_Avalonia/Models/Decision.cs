using System.Collections.Generic;
using System.ComponentModel;

namespace DSS_Avalonia.Models
{
    /// <summary>
    /// Решение, рекомендуемое в ответ на распознанную ситуацию.
    /// </summary>
    public class Decision : INotifyPropertyChanged
    {
        private string _title = string.Empty;
        private string _description = string.Empty;
        private int _priority = 1;
        private List<string> _actions = new();

        /// <summary>Краткое название решения.</summary>
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        /// <summary>Детальное описание.</summary>
        public string Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        /// <summary>Приоритет (1 — наивысший).</summary>
        public int Priority
        {
            get => _priority;
            set { _priority = value; OnPropertyChanged(nameof(Priority)); }
        }

        /// <summary>Список конкретных действий для выполнения.</summary>
        public List<string> Actions
        {
            get => _actions;
            set { _actions = value; OnPropertyChanged(nameof(Actions)); }
        }

        public Decision() { }

        public Decision(string title, string description, int priority = 1)
        {
            Title = title;
            Description = description;
            Priority = priority;
        }

        public override string ToString() => $"[P{Priority}] {Title}";

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string prop) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    }
}
