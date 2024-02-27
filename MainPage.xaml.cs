using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Data.Sqlite;
using System.Text;
using System.Collections.ObjectModel;
using Windows.UI.ViewManagement;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using NCalc;

namespace Calculator
{
    public sealed partial class MainPage : Page
    {
        private StringBuilder currentInput = new StringBuilder();
        public string DisplayText => currentInput.ToString();
        private ObservableCollection<CalculationHistory> calculationHistory = new ObservableCollection<CalculationHistory>();
        public ObservableCollection<CalculationHistory> CalculationHistoryList => calculationHistory;

        public MainPage()
        {
            this.InitializeComponent();
            ApplicationView.PreferredLaunchViewSize = new Size(480, 800);
            ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
            InitializeDatabase();
            LoadCalculationHistory();
        }

        private void LoadCalculationHistory()
        {
            using (var connection = new SqliteConnection("Data Source=calculator.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM CalculationHistories";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        calculationHistory.Add(new CalculationHistory
                        {
                            Expression = reader.GetString(1),
                            Result = reader.GetString(2)
                        });
                    }
                }
            }
        }


        private void Number_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            currentInput.Append(button.Content.ToString());
            Display.Text = DisplayText;
        }

        private void Operator_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            string operatorValue = button.Content.ToString();

            if (currentInput.Length > 0 && !IsOperator(currentInput[currentInput.Length - 1]))
            {
                currentInput.Append(operatorValue);
                Display.Text = currentInput.ToString();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            currentInput.Clear();
            Display.Text = "";
        }

        private bool IsOperator(char c)
        {
            return c == '+' || c == '-' || c == '*' || c == '/';
        }


        private void Equals_Click(object sender, RoutedEventArgs e)
        {
            string expression = currentInput.ToString();
            Expression expr = new Expression(expression);

            try
            {
                object result = expr.Evaluate();
                DisplayResult(result.ToString());
                SaveCalculationHistory(expression, result.ToString());
            }
            catch (EvaluationException ex)
            {
                DisplayResult("Error");
            }
        }

        private void DisplayResult(string result)
        {
            Display.Text = result;
        }


        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection("Data Source=calculator.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE IF NOT EXISTS CalculationHistories (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT, 
                    Expression TEXT NOT NULL, 
                    Result TEXT NOT NULL
                )";
                command.ExecuteNonQuery();
            }
        }

        private void SaveCalculationHistory(string expression, string result)
        {
            using (var connection = new SqliteConnection("Data Source=calculator.db"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = $"INSERT INTO CalculationHistories (Expression, Result) VALUES ('{expression}', '{result}')";
                command.ExecuteNonQuery();
            }
        }

        public class CalculationHistory
        {
            public int Id { get; set; }
            public string Expression { get; set; }
            public string Result { get; set; }
        }

    }
}

