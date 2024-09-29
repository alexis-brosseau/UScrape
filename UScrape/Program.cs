using MySql.Data.MySqlClient;
using System.Diagnostics;
using System.Text;

namespace UScrape
{
    class Program
    {
        private static IniFile Config { get; } = new IniFile("config.ini");

        static void Main(string[] args)
        {
            string header = "╔═════════════════════════════════════════════════════════╗\r\n║██╗   ██╗███████╗ ██████╗██████╗  █████╗ ██████╗ ███████╗║\r\n║██║   ██║██╔════╝██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔════╝║\r\n║██║   ██║███████╗██║     ██████╔╝███████║██████╔╝█████╗  ║\r\n║██║   ██║╚════██║██║     ██╔══██╗██╔══██║██╔═══╝ ██╔══╝  ║\r\n║╚██████╔╝███████║╚██████╗██║  ██║██║  ██║██║     ███████╗║\r\n║ ╚═════╝ ╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚══════╝║\r\n╚═════════════════════════════════════════════════════════╝";
            Interface ui = new Interface(header, Enum.Parse<ConsoleColor>(Config.Read("Options", "UIColor")), 0);
            ui.Start();
            ShowMenu(ui);

            string host = Config.Read("DB", "Host");
            string port = Config.Read("DB", "Port");
            string database = Config.Read("DB", "Name");
            string username = Config.Read("DB", "Username");
            string password = Config.Read("DB", "Password");

            DB.Set(host, port, database, username, password);
        }

        // ---------- Menus ----------
        static void ShowMenu(Interface ui)
        {
            ui.Clear();
            ui.SkipLine(1);
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Scrape", () => ShowScrape(ui)),
                ("Options", () => ShowOptions(ui)),
                ("", () => { }),
                ("Exit", () => {
                    ui.Clear();
                    ui.WriteLine("Exiting...");
                    ui.Stop();
                }),
            });
        }

        static void ShowScrape(Interface ui)
        {
            ui.Clear();
            ui.SkipLine(1);
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Scrape Evenko", () => ScrapeEvenko(ui)),
                ("", () => { }),
                ("Back", () => ShowMenu(ui)),
            });
        }

        static void ShowProgress(Interface ui, ProgressTracker progress, TimeSpan interval)
        {
            ui.Clear();
            ui.SkipLine(1);
            ui.WriteLine("Status: Running");
            ui.SkipLine(2);

            progress.OnError(() =>
            {
                ui.Clear();
                ui.SkipLine(1);
                ui.WriteLine("Status: Error");
                ui.WriteLine($"{progress.Message} - {(progress.Progression * 100).ToString("0.##")}%");
                ui.WriteLine($"{progress.Data}");
                ui.SkipLine(1);
                ui.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Back", () =>
                    {
                        progress.Abort();
                    }),
                });
            });

            progress.OnUpdate(() =>
            {
                if (progress.Status != ProgressTracker.Statuses.Running)
                    return;

                ui.UpdateLine($"{progress.Message} - {(progress.Progression * 100).ToString("0.##")}%", ui.HeaderHeight + 3);

                for (int i = 0; i < progress.Data.Length / Console.WindowWidth; i++)
                    ui.SkipLine(1);

                ui.UpdateLine($"{progress.Data}", ui.HeaderHeight + 4);
                ui.SkipLine(1);
                ui.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Abort", () => {
                        progress.Abort();
                    }),
                });
            }, interval);
        }

        static void ShowSaveOptions(Interface ui, List<ISavable> data)
        {
            List<string> a = data.Select(e => e.ToSQL("evenement")).ToList();

            ui.Clear();
            ui.SkipLine(1);
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Save in Json", () => SaveData(ui, data, SaveFormat.JSON)),
                ("Save in SQL", () => SaveData(ui, data, SaveFormat.SQL)),
                ("Push to database", () => PushData(ui, data, "ajouterEvenement")),
                ("", () => { }),
                ("Back", () => ShowMenu(ui)),
            });
        }

        static void ShowOptions(Interface ui)
        {
            ui.Clear();
            ui.SkipLine(1);
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Output folder", () => ShowOutputFolder(ui)),
                ("UI's color", () =>
                {
                    if (IsColorDark(ui.Color))
                        ShowDarkColors(ui);
                    else
                        ShowLightColors(ui);
                }),
                ("", () => { }),
                ("Back", () => ShowMenu(ui)),
            });
        }

        static void ShowOutputFolder(Interface ui)
        {
            ui.Clear();
            ui.SkipLine(1);
            ui.WriteLine($"Current folder: {Config.Read("Options", "OutputFolder")}\n");
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Change", () => { 
                    ui.UpdateLine("Enter new folder path: ", ui.HeaderHeight + 1);
                    ui.ReadLine((value) =>
                    {
                        Config.Write("Options", "OutputFolder", value);
                        ShowOutputFolder(ui);
                    });
                }),
                ("", () => { }),
                ("Back", () => ShowOptions(ui)),
            }); 
        }

        static void ShowLightColors(Interface ui)
        {
            string mode = IsColorDark(ui.Color) ? "Dark" : "Light";

            ui.Clear();
            ui.WriteLine($"Current color: {ui.Color}");
            ui.WriteLine($"Current palette: {mode}\n");
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("Red", () =>
                {
                    ui.Color = ConsoleColor.Red;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("Green", () =>
                {
                    ui.Color = ConsoleColor.Green;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("Blue", () =>
                {
                    ui.Color = ConsoleColor.Blue;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("Yellow", () =>
                {
                    ui.Color = ConsoleColor.Yellow;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("Cyan", () =>
                {
                    ui.Color = ConsoleColor.Cyan;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("Magenta", () =>
                {
                    ui.Color = ConsoleColor.Magenta;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("White", () =>
                {
                    ui.Color = ConsoleColor.White;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowLightColors(ui);
                }),
                ("", () => { }),
                ("See dark colors", () => ShowDarkColors(ui)),
                ("Back", () => {
                    ShowOptions(ui);
                }),
            });
        }

        static void ShowDarkColors(Interface ui)
        {
            string mode = IsColorDark(ui.Color) ? "Dark" : "Light";

            ui.Clear();
            ui.WriteLine($"Current color: {ui.Color}");
            ui.WriteLine($"Current palette: {mode}\n");
            ui.ShowNavigation(new List<(string, Action)>
            {
                ("DarkRed", () =>
                {
                    ui.Color = ConsoleColor.DarkRed;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("DarkGreen", () =>
                {
                    ui.Color = ConsoleColor.DarkGreen;
                    Config.Write("Options", "UIColor", ui.Color.ToString()); 
                    ShowDarkColors(ui);
                }),
                ("DarkBlue", () =>
                {
                    ui.Color = ConsoleColor.DarkBlue;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("DarkYellow", () =>
                {
                    ui.Color = ConsoleColor.DarkYellow;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("DarkCyan", () =>
                {
                    ui.Color = ConsoleColor.DarkCyan;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("DarkMagenta", () =>
                {
                    ui.Color = ConsoleColor.DarkMagenta;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("Gray", () =>
                {
                    ui.Color = ConsoleColor.Gray;
                    Config.Write("Options", "UIColor", ui.Color.ToString());
                    ShowDarkColors(ui);
                }),
                ("", () => { }),
                ("See light colors", () => ShowLightColors(ui)),
                ("Back", () => {
                    ShowOptions(ui);
                }),
            });
        }

        // ---------- Scraping ----------
        static void ScrapeEvenko(Interface ui)
        {
            List<ISavable> result = new List<ISavable>();
            ProgressTracker progress = new ProgressTracker();

            Thread thread = new Thread(() => {
                result = Scraper.ScrapeEvenko(progress);

                if (progress.Status == ProgressTracker.Statuses.Completed)
                    ShowSaveOptions(ui, result);
            });

            progress.OnAbortion(() =>
            {
                thread.Join(100);
                ShowScrape(ui);
            });

            thread.Start();
            ShowProgress(ui, progress, TimeSpan.FromMilliseconds(250));
        }

        // ---------- Utils ----------
        static void SaveData(Interface ui, List<ISavable> data, SaveFormat format)
        {
            ProgressTracker progress = new ProgressTracker();

            string filepath = Config.Read("Options", "OutputFolder");
            string filename = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            List<string> rows = new List<string>();

            switch (format)
            {
                case SaveFormat.JSON:
                    filename += ".json";
                    rows = data.Select(e => e.ToJSON()).ToList();
                    break;

                case SaveFormat.SQL:
                    filename += ".sql";
                    rows = data.Select(e => e.ToSQL("ajouterEvenement")).ToList();
                    break;
            }

            Thread thread = new Thread(() =>
            {
                progress.Message = "Saving to file";

                try
                {
                    DirectoryInfo dir = new DirectoryInfo(filepath);
                    dir.Create();

                    FileInfo file = new FileInfo($"{filepath}/{filename}");
                    file.Directory.Create();

                    StreamWriter writer = new StreamWriter($"{filepath}/{filename}");
                    for (int i = 0; i < rows.Count(); i++)
                    {
                        if (progress.Status == ProgressTracker.Statuses.Aborted)
                        {
                            writer.Close();
                            File.Delete(filename);
                            return;
                        }

                        progress.Progression = (double)i / rows.Count;
                        progress.Data = rows[i];

                        writer.WriteLine(rows[i]);
                    }
                    writer.Close();
                    progress.Status = ProgressTracker.Statuses.Completed;
                }
                catch (Exception e)
                {
                    progress.Data = e.Message;
                    progress.Status = ProgressTracker.Statuses.Error;
                }
            });

            progress.OnAbortion(() =>
            {
                thread.Join(100);
                ShowSaveOptions(ui, data);
            });

            progress.OnCompletion(() =>
            {
                FileInfo file = new FileInfo($"{filepath}/{filename}");

                thread.Join(100);
                ui.Clear();
                ui.SkipLine(1);
                ui.WriteLine("Status: Completed");
                ui.WriteLine($"File saved at {file.FullName}");
                ui.SkipLine(1);
                ui.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Back", () => ShowSaveOptions(ui, data)),
                });
            });

            thread.Start();
            ShowProgress(ui, progress, TimeSpan.FromMilliseconds(250));
        }

        static void PushData(Interface ui, List<ISavable> data, string procedure)
        {
            ProgressTracker progress = new ProgressTracker();
            MySqlConnection connection = new MySqlConnection(DB.ConnectionString);

            Thread thread = new Thread(() =>
            {
                progress.Message = "Pushing to database";

                SQL.TryExecute(connection, () =>
                {
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < data.Count; i++)
                    {
                        if (progress.Status == ProgressTracker.Statuses.Aborted)
                            return;

                        progress.Progression = (double)i / data.Count;
                        progress.Data = data[i].ToSQL(procedure);
                        builder.Append(data[i].ToSQL(procedure));
                    }

                    progress.Message = "Executing query";
                    MySqlCommand command = new MySqlCommand(builder.ToString(), connection);
                    command.ExecuteNonQuery();
                }, out Exception? exception);
                
                if (exception != null)
                {
                    progress.Data = exception.Message;
                    progress.Status = ProgressTracker.Statuses.Error;
                    return;
                }

                progress.Status = ProgressTracker.Statuses.Completed;
            });

            progress.OnAbortion(() =>
            {
                thread.Join(100);
                ShowSaveOptions(ui, data);
                Debug.WriteLine("Aborted");
                ShowSaveOptions(ui, data);
            });

            progress.OnCompletion(() =>
            {
                thread.Join(100);
                ui.Clear();
                ui.SkipLine(1);
                ui.WriteLine("Status: Completed");
                ui.WriteLine("Data pushed to database");
                ui.SkipLine(1);
                ui.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Back", () => ShowSaveOptions(ui, data)),
                });
            });

            thread.Start();
            ShowProgress(ui, progress, TimeSpan.FromMilliseconds(250));
        }

        static bool IsColorDark(ConsoleColor color)
        {
            return color == ConsoleColor.Black || color == ConsoleColor.DarkBlue || color == ConsoleColor.DarkGreen || color == ConsoleColor.DarkCyan || color == ConsoleColor.DarkRed || color == ConsoleColor.DarkMagenta || color == ConsoleColor.DarkYellow || color == ConsoleColor.Gray;
        }
    }
}