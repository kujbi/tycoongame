using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using Model;
using ViewModel.Main;
using ViewModel.Util;

namespace View.Util
{
    /// <summary>
    /// Kattintható Grid
    /// </summary>
    public class ClickableGrid : Grid
    {
        private GridPoint? _cursorPosition = null;
        private double _cellSize;
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        /// <summary>
        /// Inicializál egy új ClickableGrid példányt
        /// </summary>
        public ClickableGrid()
        {
            Loaded += ClickableGrid_Loaded;
            SizeChanged += ClickableGrid_SizeChanged;
        }

        private void ClickableGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _cellSize = ActualWidth / Size;
        }
        
        /// <summary>
        /// Pályaméret
        /// </summary>
        public int Size
        {
            get => (int)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        // Using a DependencyProperty as the backing store for Size.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(int), typeof(ClickableGrid), new PropertyMetadata(1));


        private void ClickableGrid_Loaded(object sender, RoutedEventArgs e)
        {
            MouseDown += Grid_MouseDown;
            MouseMove += ClickableGrid_MouseMove;
            MouseEnter += ClickableGrid_MouseEnter;
            MouseLeave += ClickableGrid_MouseLeave;

            ColumnDefinitions.Clear();
            RowDefinitions.Clear();
            for(var i = 0; i < Size; i++)
            {
                ColumnDefinitions.Add(new ColumnDefinition());
                RowDefinitions.Add(new RowDefinition());
            }

            _cellSize = ActualWidth / Size;
        }

        private void ClickableGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            _cursorPosition = null;
            ViewModel.Grid_CursorAction(MouseActionType.Leave, new(0,0));
        }

        private void ClickableGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            _cursorPosition = FindLocation(mousePosition);


            ViewModel.Grid_CursorAction(MouseActionType.Enter, _cursorPosition.Value);

        }

        private void ClickableGrid_MouseMove(object sender, MouseEventArgs e)
        {
            var mousePosition = e.GetPosition(this);
            var point = FindLocation(mousePosition);

            if(point != _cursorPosition)
            {
                _cursorPosition = point;
                ViewModel.Grid_CursorAction(MouseActionType.Move, point);
            }

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(this);

            _cursorPosition = FindLocation(mousePosition);

            ViewModel.Grid_CursorAction(MouseActionType.Click, _cursorPosition.Value);
        }

        private GridPoint FindLocation(Point mousePosition)
        {
            return new GridPoint(
                column: (int)(mousePosition.X / _cellSize),
                row: (int)(mousePosition.Y / _cellSize));
        }

    }
}
