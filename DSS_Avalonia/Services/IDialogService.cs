using System.Threading.Tasks;
using DSS_Avalonia.Models;

namespace DSS_Avalonia.Services
{
    /// <summary>
    /// Абстракция над UI-диалогами — позволяет ViewModel оставаться независимой от View.
    /// </summary>
    public interface IDialogService
    {
        Task<bool> ConfirmAsync(string message, string title = "Подтверждение");
        Task AlertAsync(string message, string title = "Внимание");
        Task<LearnDialogResult?> ShowLearnDialogAsync();
        Task OpenKnowledgeBaseAsync(KnowledgeBaseService kb, DecisionEngine engine);
    }
}
