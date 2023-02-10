using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Professionbot;
using System.Reflection;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Linq;
using Discord.Rest;
using Microsoft.VisualBasic;

public class Program
{
    public static Task Main(string[] args) => new Program().MainAsync();

    private Task Log(LogMessage msg)
    {
        Console.WriteLine(msg.ToString());
        return Task.CompletedTask;
    }
    

    private DiscordSocketClient _client;
    public CommandService _commands;
    private IServiceProvider _services;

    private SocketGuild guild;

    private ulong LogChannelID;
    private SocketTextChannel Logchannel;
    public List<string[]> players = new List<string[]>();
    public async Task MainAsync()
    {

        DiscordSocketConfig config = new DiscordSocketConfig()
        {
            GatewayIntents = GatewayIntents.All
        };

        _client = new DiscordSocketClient(config);
        _commands = new CommandService();
        _services = new ServiceCollection()
            .AddSingleton(_client)
            .AddSingleton(_commands)
            .BuildServiceProvider();

        var token = "MTA3MzU4ODYzMDYwMzQ1MjQ0Nw.G_fqLT.662P2OphuEtD1ynZG2CGFt7OayMqM7qhLu3Ijg";
        _client.Log += _client_Log;
        await RegisterCommandsAsync();
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await Task.Delay(-1);
    }

    private Task _client_Log(LogMessage arg)
    {
        Console.WriteLine(arg);
        return Task.CompletedTask;
    }

    public async Task RegisterCommandsAsync()
    {
        _client.MessageReceived += HandleCommandAsync;
        await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
    }

    public async Task HandleCommandAsync(SocketMessage arg)
    {
        var message = arg as SocketUserMessage;
        var context = new SocketCommandContext(_client, message);
        var Author = message.Author;
        var channel = _client.GetChannel(LogChannelID) as SocketTextChannel;

        Console.WriteLine("---------------\nUser: " + message.Author.Username + " mit ID " + message.Author.Id + "\nhat geschrieben:" + "\n" +  message.ToString());
        if (message.Author.IsBot) return;
        int argPos = 0;
        if (message.HasStringPrefix("/",ref argPos))
        {
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess) Console.WriteLine(result.ErrorReason);
            if (result.Error.Equals(CommandError.UnmetPrecondition)) await message.Channel.SendMessageAsync(result.ErrorReason);
        }

        var text = message.ToString().ToLower();

        if (text == "/berufe")
        {
            ulong messageChannelId = message.Channel.Id;
            
            var textChannel = _client.GetChannel(messageChannelId) as SocketTextChannel;
            guild = _client.GetGuild(textChannel.Category.GuildId);
            string msg = "`Spieler      Beruf      Spezialisierung      Notizen\n";
            if (players.Count == 0)
            {
                await textChannel.SendMessageAsync("Keine Chars eingetragen!");
                message.DeleteAsync();
                return;
            }
            for(int i = 0; i < players.Count; i++)
            {
                msg += players[i][0].PadRight(12) + " " + players[i][1].PadRight(10) + " " + players[i][2].PadRight(20) + " " + players[i][3] + "\n";
            }
            msg += "`";
            await textChannel.SendMessageAsync(msg);

            message.DeleteAsync();
        }
        if (text.StartsWith("/add"))
        {
            if (arg.ToString().Split(" ").Length < 4)
            {
                await Author.SendMessageAsync("Falsche Anzahl an Argumenten:\n/add <Charactername> <Beruf> <Spezialisierung>\noder\nadd <Charactername> <Beruf> <Spezialisierung> <Notes>");
            }else
            {

                if (arg.ToString().Split(" ").Length == 4)
                    players.Add(new string[4] { arg.ToString().Split(" ")[1], arg.ToString().Split(" ")[2], arg.ToString().Split(" ")[3], "NA" });
                else
                {
                    int argLentgth = arg.ToString().Length;
                    int noteIndex = arg.ToString().IndexOf(arg.ToString().Split(" ")[4]);
                    players.Add(new string[4] { arg.ToString().Split(" ")[1], arg.ToString().Split(" ")[2], arg.ToString().Split(" ")[3], arg.ToString().Substring(noteIndex, argLentgth-noteIndex) });
                }
                await Author.SendMessageAsync("Spieler **" + arg.ToString().Split(" ")[1] + "** mit Beruf **" + arg.ToString().Split(" ")[2] + "** hinzugefügt!");
            }
            

            message.DeleteAsync();
        }
        if (text == "/help")
        {
            await Author.SendMessageAsync("**/Berufe** \n    *Gibt Liste mit allen Chars und deren Berufen zurück*\n-------------------------------------------------------"
                + "\n**/add <Charactername> <Beruf> <Spezialisierung> <Notes>**"
                + "\n    *Fügt Character zur Berufsliste hinzu*"
                +"\n`Beruf    : Spezialiserungen"
                + "\n_______________________________________________________"
                + "\nVZ       : All"
                + "\nJuwe     : Schmuck, Gems, All"
                + "\nSchneider: All"
                + "\nSchmied  : Waffen, Rüstungen, All"
                + "\nLederer  : Ketten, Leder, All"
                + "\nIngi     : All"
                + "\nInschrift: Waffen, Trinkets, Treatise, All`"
                + "\n-------------------------------------------------------"
                + "\n**/request <Beruf> <Spezialisierung>**"
                + "\n*    Pingt alle mit passendem Beruf und Spezialierung an*");



            message.DeleteAsync();
        }
    }
}

