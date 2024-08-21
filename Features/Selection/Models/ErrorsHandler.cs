using System.Windows;

namespace FireBoost.Features.Selection.Models
{
    internal class ErrorsHandler
    {
        private readonly string _invalidDiameter = "Значение поля \"Диаметр\" не является числом.\nИзменить исходные данные?";
        private readonly string _invalidHeight = "Значение поля \"Ширина\" не является числом.\nИзменить исходные данные?";
        private readonly string _invalidWidth = "Значение поля \"Высота\" не является числом.\nИзменить исходные данные?";
        private readonly string _invalidOffset = "Значение поля \"Зазор\" не является числом.\nИзменить исходные данные?";
        private readonly string _invalidFamily = "Семейство не найдено.\nИзменить исходные данные?";
        private readonly string _hasFamilies = "Файлы .rfa не найдены. Хотите выбрать путь к другой папке?";

        public string InvalidDiameter => _invalidDiameter;
        public string InvalidHeight => _invalidHeight;
        public string InvalidWidth => _invalidWidth;
        public string InvalidOffset => _invalidOffset;
        public string InvalidFamily => _invalidFamily;
        public string HasFamilies => _hasFamilies;

        public MessageBoxResult ShowQuestion(string text) =>
            MessageBox.Show(text, "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

        public MessageBoxResult ShowZeroValue(string property) =>
            MessageBox.Show($"Значение поля \"{property}\" должно быть больше нуля.\nИзменить исходные данные?", "Предупреждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
    }
}
