using SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static GUI.TranslationChoice;
using static SDK.Translation.TranslationGroup;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    public class TranslationChoice
    {
        public struct UnitChoice
        {
            public required TranslationUnit Unit { get; init; }
            public required RadioButton Button { get; init; }
        }

        public List<UnitChoice> UnitChoices { get; set; } = new();
        public Translation Translation { get; }

        public TranslationChoice(Translation translation)
        {
            Translation = translation;
        }
    }

    public partial class MainWindow : Window
    {
        private List<TranslationChoice> translationChoices = new();
        private DependencyProperty IsIdleProperty = DependencyProperty.Register(nameof(IsIdle), typeof(bool), typeof(MainWindow));
        private bool IsIdle { get { return (bool)GetValue(IsIdleProperty); } set { SetValue(IsIdleProperty, value); } }
        private readonly DikiAccessor dikiAccessor = new();

        private FrameworkElement createTranslationContainer(Translation translation)
        {
            TranslationChoice translationChoice = new(translation);
            const double marginSize = 3;

            Grid grid = new Grid();

            Border border = new()
            {
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(1, 1, 1, 1),
                Child = grid
            };

            RowDefinition row = new RowDefinition
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };
            grid.RowDefinitions.Add(row);

            ColumnDefinition column1 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Auto)
            };
            grid.ColumnDefinitions.Add(column1);

            ColumnDefinition column2 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Pixel)
            };
            grid.ColumnDefinitions.Add(column2);

            ColumnDefinition column3 = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star)
            };
            grid.ColumnDefinitions.Add(column3);

            TextBlock searchedTextBlock = new()
            {
                Text = translation.SearchedPhrase,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap
            };
            Grid.SetColumn(searchedTextBlock, 0);
            Grid.SetColumnSpan(searchedTextBlock, 3);
            Grid.SetRow(searchedTextBlock, 0);
            grid.Children.Add(searchedTextBlock);

            row = new()
            {
                Height = new GridLength(1, GridUnitType.Auto)
            };
            grid.RowDefinitions.Add(row);

            Separator separator = new();
            Grid.SetColumn(separator, 0);
            Grid.SetColumnSpan(separator, 3);
            Grid.SetRow(separator, 1);
            grid.Children.Add(separator);

            if (translation.Groups.Any())
            {
                separator = new()
                {
                    LayoutTransform = new RotateTransform(90),
                    Margin = new Thickness(0, marginSize, 0, 0)
                };
                Grid.SetColumn(separator, 1);
                Grid.SetRow(separator, 1);
                Grid.SetRowSpan(separator, int.MaxValue);
                grid.Children.Add(separator);
            }

            var radioGroup = Guid.NewGuid().ToString();

            for (int groupIndex = 0; groupIndex < translation.Groups.Count; ++groupIndex)
            {
                var group = translation.Groups[groupIndex];
                var currentRowIndex = groupIndex == 0 ? (groupIndex + 2) : (2 * groupIndex + 2);

                row = new()
                {
                    Height = new GridLength(1, GridUnitType.Auto)
                };
                grid.RowDefinitions.Add(row);

                StringBuilder sb = new();
                sb.AppendLine($"Fraza: {group.Phrase}");

                if (group.PartOfSpeech != string.Empty)
                {
                    sb.AppendLine($"Część mowy: {group.PartOfSpeech}");
                }

                TextBlock groupTextBlock = new()
                {
                    Text = sb.ToString().Trim(),
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(marginSize)
                };
                Grid.SetColumn(groupTextBlock, 0);
                Grid.SetRow(groupTextBlock, currentRowIndex);
                grid.Children.Add(groupTextBlock);

                sb.Clear();

                StackPanel meaningsStackPanel = new()
                {
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetRow(meaningsStackPanel, currentRowIndex);
                Grid.SetColumn(meaningsStackPanel, 2);
                grid.Children.Add(meaningsStackPanel);

                for (int unitIndex = 0; unitIndex < group.Units.Count; ++unitIndex)
                {
                    var unit = group.Units[unitIndex];
                    sb.AppendLine($"Tłumaczenie: {unit.Phrase}");

                    if (unit.Meanings.Any())
                    {
                        sb.AppendLine($"Znaczenia: {string.Join(", ", unit.Meanings)}");
                    }
                    if (unit.Plurals.Any())
                    {
                        sb.AppendLine($"Liczby mnogie: {string.Join(", ", unit.Plurals)}");
                    }

                    RadioButton radioButton = new()
                    {
                        VerticalAlignment = VerticalAlignment.Center,
                        GroupName = radioGroup,
                        IsChecked = groupIndex == 0 && unitIndex == 0
                    };
                    translationChoice.UnitChoices.Add(new UnitChoice() { Button = radioButton, Unit = unit });

                    TextBlock unitTextblock = new()
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = sb.ToString().Trim()
                    };

                    sb.Clear();

                    StackPanel unitStackPanel = new()
                    {
                        Orientation = Orientation.Horizontal
                    };
                    unitStackPanel.MouseLeftButtonDown += (object sender, MouseButtonEventArgs e) =>
                    {
                        radioButton.IsChecked = true;
                    };
                    unitStackPanel.Children.Add(radioButton);
                    unitStackPanel.Children.Add(unitTextblock);

                    meaningsStackPanel.Children.Add(unitStackPanel);

                    if (unitIndex < group.Units.Count - 1)
                    {
                        meaningsStackPanel.Children.Add(new Separator());
                    }
                }

                if (groupIndex < translation.Groups.Count - 1)
                {
                    row = new RowDefinition
                    {
                        Height = new GridLength(1, GridUnitType.Auto)
                    };
                    grid.RowDefinitions.Add(row);

                    separator = new Separator();
                    Grid.SetColumn(separator, 0);
                    Grid.SetColumnSpan(separator, 3);
                    Grid.SetRow(separator, currentRowIndex + 1);
                    grid.Children.Add(separator);
                }
            }

            translationChoices.Add(translationChoice);

            return border;
        }

        private string clearEmptyLines(string str)
        {
            return Regex.Replace(str, @"^\s+$[\r\n]*", string.Empty, RegexOptions.Multiline);
        }

        private void clickGetTranslationButton(object sender, RoutedEventArgs e)
        {
            IsIdle = false;
            textBlockStatus.Text = "Tłumaczenie...";
            buttonGetTranslations.Content = "Anuluj";

            textBoxInput.Text = clearEmptyLines(textBoxInput.Text);

            var input = textBoxInput.Text;
            var phrases = input.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();

            translationChoices.Clear();

            progressBar.Value = 0;
            double maxProgress = phrases.Count;
            IProgress<int> progress = new Progress<int>(p => progressBar.Value = p / maxProgress);

            CancellationTokenSource cancellationTokenSource = new();
            buttonGetTranslations.Click -= clickGetTranslationButton;
            buttonGetTranslations.Click += (object sender, RoutedEventArgs e) => { cancellationTokenSource.Cancel(); };

            var translationTask = dikiAccessor.Request(phrases, progress, cancellationTokenSource.Token);
            translationTask.ContinueWith(task =>
            {
                StackPanel stackPanel = new();
                stackPanel.Orientation = Orientation.Vertical;

                if (task.IsFaulted)
                {
                    progressBar.Value = 0;
                    textBlockStatus.Text = "Błąd: brak połączenia";
                }
                else if (task.IsCanceled)
                {
                    progressBar.Value = 0;
                    textBlockStatus.Text = "Anulowano";
                }
                else if (task.IsCompleted)
                {
                    textBlockStatus.Text = "Gotowe";

                    var results = task.Result;
                    var rejestedPhrases = new List<string>();

                    foreach (var result in results)
                    {
                        if (result.Groups.Count == 0)
                        {
                            var index = phrases.IndexOf(result.SearchedPhrase);
                            if (index >= 0)
                            {
                                phrases.RemoveAt(index);
                            }

                            rejestedPhrases.Add(result.SearchedPhrase);
                            continue;
                        }

                        var translationContainer = createTranslationContainer(result);
                        stackPanel.Children.Add(translationContainer);
                    }

                    textBoxInput.Text = string.Join(Environment.NewLine, phrases);

                    var currentRejectedPhrases = textBoxRejected.Text.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    currentRejectedPhrases = currentRejectedPhrases.Concat(rejestedPhrases).Distinct().ToList();
                    textBoxRejected.Text = string.Join(Environment.NewLine, currentRejectedPhrases);
                }

                scrollViewer.Content = stackPanel;

                if (checkBoxEnableScrolling.IsChecked ?? false)
                {
                    scrollViewer.ScrollToEnd();
                }

                buttonGetTranslations.Content = "Pobierz";
                buttonGetTranslations.Click += clickGetTranslationButton;
                IsIdle = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void clickFormatTranslationsButton(object sender, RoutedEventArgs e)
        {
            textBoxOutput.Text = string.Empty;
            StringBuilder sb = new();

            foreach (var translationChoice in translationChoices)
            {
                if (translationChoice.UnitChoices.Count == 0)
                {
                    continue;
                }

                if (sb.Length != 0)
                {
                    sb.Append("\n");
                }

                var unitChoice = translationChoice.UnitChoices.Single(uc => uc.Button.IsChecked == true);
                var unit = unitChoice.Unit;
                var translation = translationChoice.Translation;
                var group = translation.Groups.Single(g => g.Units.Contains(unit));

                sb.Append(group.Type == GroupType.Foreign ? group.Phrase : unit.Phrase);
                if (unit.Plurals.Count > 0)
                {
                    // It needs to be that way: for interpolation \t is not inserted correctly.
                    var plurals = ", " + string.Join(", ", unit.Plurals);
                    sb.Append(plurals);
                }
                sb.Append("\t");
                sb.Append(group.Type == GroupType.Foreign ? unit.Phrase : group.Phrase);
            }

            textBoxOutput.Text += sb.ToString();
        }

        private void insertTextToInputTextBox(string text)
        {
            var startCarretIndex = textBoxInput.CaretIndex;
            var selectionEnd = textBoxInput.SelectionStart + textBoxInput.SelectionLength;

            StringBuilder sb = new();
            sb.Append(textBoxInput.Text.Substring(0, textBoxInput.SelectionStart));
            sb.Append(text);
            sb.Append(textBoxInput.Text.Substring(selectionEnd, textBoxInput.Text.Length - selectionEnd));

            textBoxInput.Text = sb.ToString();
            textBoxInput.CaretIndex += (startCarretIndex + 1);
            textBoxInput.Focus();
        }

        private void clickButtonAUmlaut(object sender, RoutedEventArgs e)
        {
            insertTextToInputTextBox("ä");
        }

        private void clickButtonOUmlaut(object sender, RoutedEventArgs e)
        {
            insertTextToInputTextBox("ö");
        }

        private void clickButtonUUmlaut(object sender, RoutedEventArgs e)
        {
            insertTextToInputTextBox("ü");
        }

        private void clickButtonSharpS(object sender, RoutedEventArgs e)
        {
            insertTextToInputTextBox("ß");
        }

        private Hyperlink createHyperlink(string link, string text)
        {
            Hyperlink hyperlink = new Hyperlink();
            hyperlink.NavigateUri = new Uri(link);
            hyperlink.ToolTip = link;
            hyperlink.RequestNavigate += (s, e) =>
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri) { UseShellExecute = true });
            };
            hyperlink.Inlines.Add(text);
            return hyperlink;
        }

        private void clickInfoButton(object sender, RoutedEventArgs e)
        {
            var bitmapImage = new BitmapImage(new Uri("pack://application:,,,/GUI;component/Resources/german_128px.png"));
            var scaleRatio = 150.0 / bitmapImage.Width;

            TextBlock textBlockAppName = new()
            {
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Text = "Diki Dictionary Scrapper"
            };

            Image image = new()
            {
                Source = bitmapImage,
                Width = bitmapImage.Width * scaleRatio,
                Height = bitmapImage.Height * scaleRatio,
                Margin = new Thickness(0, -20, 0, -20)
            };

            TextBlock textBlock = new TextBlock
            {
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            textBlock.Inlines.Add($"Wersja: {UpdateChecker.GetCurrentVersion().ToString()}\n\n");
            textBlock.Inlines.Add($"Autor: Andrzej Serwotka\n\n");
            textBlock.Inlines.Add($"Data kompilacji: {SDK.AssemblyInfo.GetLinkerTime(Assembly.GetEntryAssembly())}\n\n");
            textBlock.Inlines.Add("Aplikacja do tłumaczenia wykorzystuje internetowy słownik Diki:\n\n");
            textBlock.Inlines.Add(createHyperlink("https://www.diki.pl/dictionary/about", "Diki\n\n"));
            textBlock.Inlines.Add("Aplikacja nie jest w żaden sposób biznesowo powiązana z Diki ani nie korzysta z jego API, lecz używa metody scrappingu z publicznie dostępnego kodu HTML.\n\n");
            textBlock.Inlines.Add("Zewnętrzne materiały wykorzystane w aplikacji:\n\n");
            textBlock.Inlines.Add(createHyperlink("https://www.flaticon.com/free-icon/german-flag_8617292", "German flag icons created by rizal2109 - Flaticon (Flaticon license).\n"));
            textBlock.Inlines.Add(createHyperlink("https://www.flaticon.com/free-icon/info_1041728", "Info icons created by Freepik - Flaticon (Flaticon license).\n\n"));
            textBlock.Inlines.Add("Wszelkie uwagi i błędy proszę zgłaszać na stronie repozytorium:\n\n");
            textBlock.Inlines.Add(createHyperlink("https://github.com/aserwotka/DikiDictionaryScrapper", "@DikiDictionaryScrapper"));

            StackPanel stackPanel = new()
            {
                Orientation = Orientation.Vertical
            };
            stackPanel.Children.Add(textBlockAppName);
            stackPanel.Children.Add(image);
            stackPanel.Children.Add(textBlock);

            Window window = new()
            {
                Title = "Informacje",
                WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner,
                Icon = bitmapImage,
                SizeToContent = SizeToContent.Height,
                Width = 500,
                ResizeMode = ResizeMode.NoResize,
                Content = stackPanel,
                Owner = this
            };
            window.ShowDialog();
        }

        private void CheckUpdateFinished(Task<bool> task)
        {
            textBlockUpdate.Inlines.Clear();

            if (task.IsFaulted)
            {
                textBlockUpdate.Inlines.Add("Błąd sprawdzania aktualizacji.");
            }
            else if (task.IsCompleted && task.Result == true)
            {
                textBlockUpdate.Inlines.Add(createHyperlink(UpdateChecker.GetUrl(), "Dostępna jest nowsza wersja!"));
            }
            else if (task.IsCompleted && task.Result == false)
            {
                textBlockUpdate.Inlines.Add("Wersja jest aktualna.");
            }
        }

        public MainWindow()
        {
            UpdateChecker updateChecker = new UpdateChecker();
            var task = updateChecker.CheckNewVersionAvailable();

            InitializeComponent();
            IsIdle = true;

            task.ContinueWith(CheckUpdateFinished, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
