using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using JNogueira.Discord.Webhook.Client;

internal class Program
{
	private const int STD_INPUT_HANDLE = -10;

	private static TimeSpan dzien = new TimeSpan(1, 0, 0, 0);

	[DllImport("kernel32.dll", SetLastError = true)]
	internal static extern IntPtr GetStdHandle(int nStdHandle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CancelIoEx(IntPtr handle, IntPtr lpOverlapped);

	private static async void wiadomosc(bool poczatek, string webhook)
	{
		DiscordWebhookClient client = new DiscordWebhookClient(webhook);
		if (poczatek)
		{
			DiscordMessage message2 = new DiscordMessage(" ", "SerwerHajkenka", "https://pbs.twimg.com/profile_images/1355087978033504259/ghm06InW_400x400.png", tts: false, new DiscordMessageEmbed[1]
			{
				new DiscordMessageEmbed("Status serwerka :thumbsup:", 2, null, null, null, new DiscordMessageEmbedField[1]
				{
					new DiscordMessageEmbedField("Serwerek Hajkenka", "ZA CHWILE BEDZIE DZIAŁAŁ")
				}, new DiscordMessageEmbedThumbnail("https://pbs.twimg.com/profile_images/1355087978033504259/ghm06InW_400x400.png"), new DiscordMessageEmbedImage("https://avatars3.githubusercontent.com/u/24236993?s=460&v=4"))
			});
			await client.SendToDiscord(message2);
		}
		else
		{
			DiscordMessage message = new DiscordMessage(" ", "SerwerHajkenka", "https://pbs.twimg.com/profile_images/1355087978033504259/ghm06InW_400x400.png", tts: false, new DiscordMessageEmbed[1]
			{
				new DiscordMessageEmbed("Status serwerka :thumbsdown:", 2, null, null, null, new DiscordMessageEmbedField[1]
				{
					new DiscordMessageEmbedField("Serwerek Hajkenka", "JUZ NIE DZIAŁA")
				}, new DiscordMessageEmbedThumbnail("https://pbs.twimg.com/profile_images/1355087978033504259/ghm06InW_400x400.png"), new DiscordMessageEmbedImage("https://image.shutterstock.com/shutterstock/photos/126009806/display_1500/stock-photo-portrait-of-a-sad-man-126009806.jpg"))
			});
			await client.SendToDiscord(message);
		}
	}

	private static void backup(bool forced, string wName)
	{
		string path = "C:\\Users\\" + Environment.UserName + "\\AppData\\LocalLow\\IronGate\\Valheim\\worlds";
		DateTime teraz = DateTime.Now;
		string path2 = (forced ? (Directory.GetCurrentDirectory() + "\\Backups\\F" + wName + " - " + teraz.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")) + " " + teraz.Hour + "h" + teraz.Minute + "m") : (Directory.GetCurrentDirectory() + "\\Backups\\" + wName + " - " + teraz.ToString("d", CultureInfo.CreateSpecificCulture("de-DE")) + " " + teraz.Hour + "h" + teraz.Minute + "m"));
		string var = path + "\\" + wName;
		Directory.CreateDirectory(path2);
		File.Copy(var + ".db", path2 + "\\" + wName + ".db");
		File.Copy(var + ".fwl", path2 + "\\" + wName + ".fwl");
		Console.WriteLine("Skopiowano zapis świata");
	}

	private static void birthServer(string sciezka)
	{
		Process p = new Process();
		p.StartInfo.WorkingDirectory = sciezka;
		p.StartInfo.FileName = sciezka + "\\start_headless_server.bat";
		p.StartInfo.CreateNoWindow = true;
		p.StartInfo.RedirectStandardInput = true;
		p.StartInfo.RedirectStandardOutput = true;
		p.StartInfo.UseShellExecute = false;
		p.OutputDataReceived += ProcessOutputDataHandler;
		p.Start();
		p.BeginOutputReadLine();
	}

	private static void killServer()
	{
		Process i = new Process();
		i.StartInfo.FileName = "cmd.exe";
		i.StartInfo.CreateNoWindow = true;
		i.StartInfo.RedirectStandardInput = true;
		i.StartInfo.UseShellExecute = false;
		i.Start();
		i.StandardInput.WriteLine("taskkill /IM valheim_server.exe");
		i.Close();
	}

	private static void killPC()
	{
		Process i = new Process();
		i.StartInfo.FileName = "cmd.exe";
		i.StartInfo.CreateNoWindow = true;
		i.StartInfo.RedirectStandardInput = true;
		i.StartInfo.UseShellExecute = false;
		i.Start();
		i.StandardInput.WriteLine("shutdown -s -t 30");
		i.Close();
	}

	private static void help(TimeSpan czasZadany, bool komputerpapa)
	{
		Console.WriteLine("\nhelp - pokazuje to co widzisz\nfbackup - forsuje backup kiedy serwer jest uruchominy (zly pomysl)\nchange - zmienia godzine zamkniecia serwera");
		Console.WriteLine("komputerpapa - zdecyduj czy program ma takze wylaczyc komputer");
		Console.WriteLine("kill - zamyka serwer\n");
		Console.WriteLine("Server zostanie wylaczony o godzinie " + czasZadany.Hours + ":" + czasZadany.Minutes.ToString("00"));
		if (komputerpapa)
		{
			Console.WriteLine("Komputer zostanie wylaczony.");
		}
		else
		{
			Console.WriteLine("Komputer nie zostanie wylaczony.");
		}
	}

	public static void ProcessOutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
	{
		if (!string.IsNullOrEmpty(outLine.Data) && outLine.Data.Length > 4)
		{
			string str1 = outLine.Data.Substring(0, 5);
			string str2 = "(File";
			if (!string.Equals(str1, str2))
			{
				Console.WriteLine(outLine.Data);
			}
		}
	}

	public static TimeSpan ObliczCzas(TimeSpan czasZadany)
	{
		DateTime teraz = DateTime.Now;
		TimeSpan czasTeraz = new TimeSpan(teraz.Hour, teraz.Minute, teraz.Second);
		if (czasZadany.CompareTo(czasTeraz) < 0)
		{
			return dzien.Subtract(czasTeraz).Add(czasZadany);
		}
		return czasZadany.Subtract(czasTeraz);
	}

	public static bool ZmienKillPC(bool komputerpapa)
	{
		if (komputerpapa)
		{
			Console.WriteLine("Komputer sie wylaczy");
		}
		else
		{
			Console.WriteLine("Komputer sie nie wylaczy");
		}
		Console.WriteLine("Czy chcesz to zmienic? y/n");
		string oof = Console.ReadLine();
		if (oof == "y" && komputerpapa)
		{
			komputerpapa = false;
			Console.WriteLine("Komputer sie nie wylaczy");
		}
		else if (oof == "y" && !komputerpapa)
		{
			komputerpapa = true;
			Console.WriteLine("Komputer sie wylaczy");
		}
		else
		{
			Console.WriteLine("Aha.");
		}
		return komputerpapa;
	}

	public static void ChangeTimeOfKill(ref CancellationTokenSource source, ref TimeSpan czasZadany, ref TimeSpan wynik, string nazwa)
	{
		Console.WriteLine("Nowy czas MUSI byc inny od poprzedniego\nNajpierw podaj godzine, potem minuty.");
		TimeSpan czasStary = czasZadany;
		while (czasStary == czasZadany)
		{
			try
			{
				int godzina = int.Parse(Console.ReadLine());
				int minuta = int.Parse(Console.ReadLine());
				czasZadany = new TimeSpan(godzina, minuta, 0);
				if (czasStary == czasZadany)
				{
					Console.WriteLine("Cos zle wpisales. Sprobuj jeszcze raz.");
					continue;
				}
				Console.WriteLine("Server zostanie wylaczony o godzinie " + czasZadany.Hours + ":" + czasZadany.Minutes.ToString("00"));
				wynik = ObliczCzas(czasZadany);
			}
			catch (FormatException)
			{
				Console.WriteLine("Cos zle wpisales. Sprobuj jeszcze raz.");
			}
		}
		source.Cancel();
	}

	private static void taskSkedudyl(TimeSpan wynik, object obj, string nazwa, ref CancellationTokenSource petlaSource)
	{
		CancellationToken ct = (CancellationToken)obj;
		bool cancellationTriggered = ct.WaitHandle.WaitOne(wynik);
		if (!ct.IsCancellationRequested)
		{
			killServer();
			Thread.Sleep(10000);
			backup(forced: false, nazwa);
			petlaSource.Cancel();
			IntPtr handle = GetStdHandle(-10);
			CancelIoEx(handle, IntPtr.Zero);
		}
	}

	private static void Main(string[] args)
	{
		string[] plik = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\dane.txt");
        string[] webhook = File.ReadAllLines(Directory.GetCurrentDirectory() + "\\webhook");
		birthServer(plik[4]);
		wiadomosc(poczatek: true, webhook[0]);
		bool komputerpapa = Convert.ToBoolean(Convert.ToInt16(plik[3]));
		TimeSpan czasZadany = new TimeSpan(int.Parse(plik[0]), int.Parse(plik[1]), 0);
		TimeSpan wynik = ObliczCzas(czasZadany);
		CancellationTokenSource skedudylSource = new CancellationTokenSource();
		CancellationTokenSource petlaSource = new CancellationTokenSource();
		Thread t1 = new Thread((ThreadStart)delegate
		{
			taskSkedudyl(wynik, skedudylSource.Token, plik[2], ref petlaSource);
		});
		t1.Start();
		try
		{
			help(czasZadany, komputerpapa);
			while (!petlaSource.Token.IsCancellationRequested)
			{
				switch (Console.ReadLine())
				{
				case "help":
					help(czasZadany, komputerpapa);
					break;
				case "fbackup":
					backup(forced: true, plik[2]);
					break;
				case "change":
					ChangeTimeOfKill(ref skedudylSource, ref czasZadany, ref wynik, plik[2]);
					skedudylSource = new CancellationTokenSource();
					t1 = new Thread((ThreadStart)delegate
					{
						taskSkedudyl(wynik, skedudylSource.Token, plik[2], ref petlaSource);
					});
					t1.Start();
					break;
				case "kill":
					killServer();
					Thread.Sleep(10000);
					backup(forced: false, plik[2]);
					skedudylSource.Cancel();
					petlaSource.Cancel();
					komputerpapa = false;
					break;
				case "komputerpapa":
					komputerpapa = ZmienKillPC(komputerpapa);
					break;
				}
			}
		}
		catch (OperationCanceledException)
		{
		}
		wiadomosc(poczatek: false, webhook[0]);
		Thread.Sleep(5000);
		if (komputerpapa)
		{
			killPC();
		}
	}
}
