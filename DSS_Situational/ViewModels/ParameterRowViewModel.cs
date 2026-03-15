using DSS_Situational.Models;

namespace DSS_Situational.ViewModels
{
    /// <summary>ViewModel для одной строки параметра в DataGrid.</summary>
    public class ParameterRowViewModel : BaseViewModel
    {
        private string _name = string.Empty;
        private double _value;
        private double _weight = 1.0;
        private string _unit = string.Empty;

        public string Name { get => _name; set => Set(ref _name, value); }
        public double Value { get => _value; set => Set(ref _value, value); }
        public double Weight { get => _weight; set => Set(ref _weight, value); }
        public string Unit { get => _unit; set => Set(ref _unit, value); }

        public ParameterRowViewModel() { }

        public ParameterRowViewModel(SituationParameter p)
        {
            _name = p.Name;
            _value = p.Value;
            _weight = p.Weight;
            _unit = p.Unit;
        }

        public SituationParameter ToModel() =>
            new SituationParameter(Name, Value, Weight, Unit);
    }
}
