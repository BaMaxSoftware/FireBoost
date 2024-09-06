using System.Windows;

namespace FireBoost.Features.Selection.Models
{
    internal class VMErrorsHandler
    {
        private Window WindowOwner { get; }
        public VMErrorsHandler(Window owner)
        { 
            WindowOwner = owner;
        }

        private readonly string _titleInfo = "Информация";
        private readonly string _titleWarning = "Предупреждение";

        public string EmptyOpeningsSettings => "В настройках отсутствует информация о семействах отверстий.";
        public string EmptyOpeningTaskSettings => "В настройках отсутствует информация о семействе задания на отверстия.";
        public string EmptyHost => "Место установки не выбрано.";
        public string UnnecessaryHost => "Для отверстий или заданий на отверстия не требуется выбирать элементы основы.";
        public string NullMepType => "Тип коммуникаций не выбран.";
        public string UnnecessaryMEP => "Выбор коммуникаций не требуется.";
        public string UnselectedMEP => "Элементы коммуникаций не выбраны.";
        public string UnselectedHost => "Элементы основы не выбраны.";
        public string UnselectedOpenings => "Элементы отверстий или заданий на отверстия не выбраны.";
        public string EmptyInitialData => "Исходные данные не заполнены.";


        public MessageBoxResult ShowWarning(string text) =>
            MessageBox.Show(WindowOwner, text, _titleWarning, MessageBoxButton.OK, MessageBoxImage.Warning);

        public MessageBoxResult ShowInformation(string text) =>
            MessageBox.Show(WindowOwner, text, _titleInfo, MessageBoxButton.OK, MessageBoxImage.Information);

    }
}
