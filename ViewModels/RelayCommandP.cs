
namespace CarnetDepenses.ViewModels
{
    public class RelayCommandP : System.Windows.Input.ICommand
    {
        private readonly Action<object?> _exec;
        private readonly Func<object?, bool>? _canExecute;

        public RelayCommandP(Action<object?> exec, Func<object?, bool>? canExecute = null)
        {
            _exec = exec ?? throw new ArgumentNullException(nameof(exec));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? p) => _canExecute?.Invoke(p) ?? true;
        public void Execute(object? p) => _exec(p);
        public event EventHandler? CanExecuteChanged;

        public void Rafraichir() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
