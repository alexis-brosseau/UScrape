
namespace UScrape
{
    class Program
    {
        private static IniFile Config { get; } = new IniFile("config.ini");

        static void Main(string[] args)
        {
            string header = "╔═════════════════════════════════════════════════════════╗\r\n║██╗   ██╗███████╗ ██████╗██████╗  █████╗ ██████╗ ███████╗║\r\n║██║   ██║██╔════╝██╔════╝██╔══██╗██╔══██╗██╔══██╗██╔════╝║\r\n║██║   ██║███████╗██║     ██████╔╝███████║██████╔╝█████╗  ║\r\n║██║   ██║╚════██║██║     ██╔══██╗██╔══██║██╔═══╝ ██╔══╝  ║\r\n║╚██████╔╝███████║╚██████╗██║  ██║██║  ██║██║     ███████╗║\r\n║ ╚═════╝ ╚══════╝ ╚═════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝     ╚══════╝║\r\n╚═════════════════════════════════════════════════════════╝";
            Interface menu = new Interface(header, Enum.Parse<ConsoleColor>(Config.Read("UIColor", "Options")), 0);
            menu.Start();
            ShowMenu(menu);
        }

        // ---------- Menus ----------
        static void ShowMenu(Interface menu)
        {
            menu.Clear();
            menu.SkipLine(1);
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Scrape", () => ShowScrape(menu)),
                ("Options", () => ShowOptions(menu)),
                ("", () => { }),
                ("Exit", () => {
                    menu.Clear();
                    menu.WriteLine("Exiting...");
                    menu.Stop();
                }),
            });
        }

        static void ShowScrape(Interface menu)
        {
            menu.Clear();
            menu.SkipLine(1);
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Scrape Evenko", () => ScrapeEvenko(menu)),
                ("", () => { }),
                ("Back", () => ShowMenu(menu)),
            });
        }

        static void ShowProgress(Interface menu, ProgressTracker progress)
        {
            ShowProgress(menu, progress, TimeSpan.Zero);
        }

        static void ShowProgress(Interface menu, ProgressTracker progress, TimeSpan interval)
        {
            menu.Clear();
            menu.SkipLine(1);
            menu.WriteLine("Status: Running");
            menu.SkipLine(2);

            progress.OnError(() =>
            {
                menu.Clear();
                menu.SkipLine(1);
                menu.WriteLine("Status: Error");
                menu.WriteLine($"{progress.Message} - {(progress.Progression * 100).ToString("0.##")}%");
                menu.WriteLine($"{progress.Data}");
                menu.SkipLine(1);
                menu.ShowNavigation(new List<(string, Action)>
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

                menu.UpdateLine($"{progress.Message} - {(progress.Progression * 100).ToString("0.##")}%", menu.HeaderHeight + 3);

                for (int i = 0; i < progress.Data.Length / Console.WindowWidth; i++)
                    menu.SkipLine(1);

                menu.UpdateLine($"{progress.Data}", menu.HeaderHeight + 4);
                menu.SkipLine(1);
                menu.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Abort", () => {
                        progress.Abort();
                    }),
                });
            }, interval);
        }

        static void ShowSaveOptions(Interface menu, List<ISavable> data)
        {
            List<string> a = data.Select(e => e.ToSQL("evenement")).ToList();

            menu.Clear();
            menu.SkipLine(1);
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Save to Json", () => SaveToFile(menu, data, SaveFormat.JSON)),
                ("Save to SQL", () => SaveToFile(menu, data, SaveFormat.SQL)),
                ("Push to database (Not implemented yet)", () => ShowSaveOptions(menu, data)),
                ("", () => { }),
                ("Back", () => ShowScrape(menu)),
            });
        }

        static void ShowOptions(Interface menu)
        {
            menu.Clear();
            menu.SkipLine(1);
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Output folder", () => ShowOutputFolder(menu)),
                ("UI's color", () =>
                {
                    if (IsColorDark(menu.Color))
                        ShowDarkColors(menu);
                    else
                        ShowLightColors(menu);
                }),
                ("", () => { }),
                ("Back", () => ShowMenu(menu)),
            });
        }

        static void ShowOutputFolder(Interface menu)
        {
            menu.Clear();
            menu.SkipLine(1);
            menu.WriteLine($"Current folder: {Config.Read("OutputFolder", "Options")}\n");
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Change", () => { 
                    menu.UpdateLine("Enter new folder path: ", menu.HeaderHeight + 1);
                    menu.ReadLine((value) =>
                    {
                        Config.Write("OutputFolder", value, "Options");
                        ShowOutputFolder(menu);
                    });
                }),
                ("", () => { }),
                ("Back", () => ShowOptions(menu)),
            }); 
        }

        static void ShowLightColors(Interface menu)
        {
            string mode = IsColorDark(menu.Color) ? "Dark" : "Light";

            menu.Clear();
            menu.WriteLine($"Current color: {menu.Color}");
            menu.WriteLine($"Current palette: {mode}\n");
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("Red", () =>
                {
                    menu.Color = ConsoleColor.Red;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("Green", () =>
                {
                    menu.Color = ConsoleColor.Green;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("Blue", () =>
                {
                    menu.Color = ConsoleColor.Blue;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("Yellow", () =>
                {
                    menu.Color = ConsoleColor.Yellow;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("Cyan", () =>
                {
                    menu.Color = ConsoleColor.Cyan;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("Magenta", () =>
                {
                    menu.Color = ConsoleColor.Magenta;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("White", () =>
                {
                    menu.Color = ConsoleColor.White;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowLightColors(menu);
                }),
                ("", () => { }),
                ("See dark colors", () => ShowDarkColors(menu)),
                ("Back", () => {
                    ShowOptions(menu);
                }),
            });
        }

        static void ShowDarkColors(Interface menu)
        {
            string mode = IsColorDark(menu.Color) ? "Dark" : "Light";

            menu.Clear();
            menu.WriteLine($"Current color: {menu.Color}");
            menu.WriteLine($"Current palette: {mode}\n");
            menu.ShowNavigation(new List<(string, Action)>
            {
                ("DarkRed", () =>
                {
                    menu.Color = ConsoleColor.DarkRed;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("DarkGreen", () =>
                {
                    menu.Color = ConsoleColor.DarkGreen;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("DarkBlue", () =>
                {
                    menu.Color = ConsoleColor.DarkBlue;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("DarkYellow", () =>
                {
                    menu.Color = ConsoleColor.DarkYellow;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("DarkCyan", () =>
                {
                    menu.Color = ConsoleColor.DarkCyan;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("DarkMagenta", () =>
                {
                    menu.Color = ConsoleColor.DarkMagenta;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("Gray", () =>
                {
                    menu.Color = ConsoleColor.Gray;
                    Config.Write("UIColor", menu.Color.ToString(), "Options");
                    ShowDarkColors(menu);
                }),
                ("", () => { }),
                ("See light colors", () => ShowLightColors(menu)),
                ("Back", () => {
                    ShowOptions(menu);
                }),
            });
        }

        // ---------- Scraping ----------
        static void ScrapeEvenko(Interface menu)
        {
            List<ISavable> result = new List<ISavable>();
            ProgressTracker progress = new ProgressTracker();

            Thread thread = new Thread(() => {
                result = Scraper.ScrapeEvenko(progress);

                if (progress.Status == ProgressTracker.Statuses.Completed)
                    ShowSaveOptions(menu, result);
            });

            progress.OnAbortion(() =>
            {
                thread.Join(100);
                ShowScrape(menu);
            });

            thread.Start();
            ShowProgress(menu, progress, TimeSpan.FromMilliseconds(250));
        }

        // ---------- Utils ----------
        static void SaveToFile(Interface menu, List<ISavable> data, SaveFormat format)
        {
            ProgressTracker progress = new ProgressTracker();

            string filepath = Config.Read("OutputFolder", "Options");
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
                    rows = data.Select(e => e.ToSQL("")).ToList();
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
                ShowSaveOptions(menu, data);
            });

            progress.OnCompletion(() =>
            {
                FileInfo file = new FileInfo($"{filepath}/{filename}");

                thread.Join(100);
                menu.Clear();
                menu.SkipLine(1);
                menu.WriteLine("Status: Completed");
                menu.WriteLine($"File saved at {file.FullName}");
                menu.SkipLine(1);
                menu.ShowNavigation(new List<(string, Action)>
                {
                    ("", () => { }),
                    ("Back", () => ShowSaveOptions(menu, data)),
                });
            });

            thread.Start();
            ShowProgress(menu, progress, TimeSpan.FromMilliseconds(250));
        }

        static bool IsColorDark(ConsoleColor color)
        {
            return color == ConsoleColor.Black || color == ConsoleColor.DarkBlue || color == ConsoleColor.DarkGreen || color == ConsoleColor.DarkCyan || color == ConsoleColor.DarkRed || color == ConsoleColor.DarkMagenta || color == ConsoleColor.DarkYellow || color == ConsoleColor.Gray;
        }
    }
}